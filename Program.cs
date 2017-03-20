using System;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Runtime.Loader;
using System.Collections.Immutable;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace csx
{
    class Program
    {
        static void Main(string[] args)
        {                                                                      
            var interactiveAssemblyLoader = new InteractiveAssemblyLoader(); 
                                    
            string pathToScript = args[0];
                        
            SourceText encodedSourceText = null;
            string codeAsPlainText = null;            
            using(var fileStream = new FileStream(pathToScript, FileMode.Open))
            {
                // We need to create a SourceText instance with an encoding
                encodedSourceText = SourceText.From(fileStream, Encoding.UTF8);
                codeAsPlainText = encodedSourceText.ToString();        
            }

            var scriptOptions = ScriptOptions.Default.WithFilePath(pathToScript);  
            scriptOptions = scriptOptions.WithMetadataResolver(new NuGetMetadataResolver(scriptOptions.MetadataResolver));
            var script = CSharpScript.Create(codeAsPlainText, scriptOptions, null, interactiveAssemblyLoader);

            // Get the Compilation that gives us full access to the Roslyn Scriping API
            var compilation = script.GetCompilation();

            SyntaxTree syntaxTree = compilation.SyntaxTrees.First();

            // The problem with CSharpScript.Create is that it does not allow 
            // to specify the encoding needed to emit the debug information.
            // Might need to open up an issue on this in the Roslyn repo.
            
            // First hack
            var encodingField = syntaxTree.GetType().GetField("_encodingOpt",BindingFlags.Instance | BindingFlags.NonPublic);
            encodingField.SetValue(syntaxTree, Encoding.UTF8);

            // Second hack

            var lazyTextField = syntaxTree.GetType().GetField("_lazyText",BindingFlags.Instance | BindingFlags.NonPublic);
            lazyTextField.SetValue(syntaxTree, encodedSourceText);

            // Next we need to write out the dynamic assembly

            EmitResult emitResult;
            using(var peStream = new MemoryStream())
            {
                using(var pdbStream = new MemoryStream())
                {
                    // https://github.com/dotnet/roslyn/blob/version-2.0.0-beta4/src/Compilers/Core/Portable/Compilation/Compilation.cs#L478
                    var referenceManager = compilation.Invoke<object>("GetBoundReferenceManager", BindingFlags.NonPublic);
                    
                    
                    var referencedAssemblies =
                        // https://github.com/dotnet/roslyn/blob/version-2.0.0-beta4/src/Compilers/Core/Portable/ReferenceManager/CommonReferenceManager.State.cs#L34
                        referenceManager.Invoke<IEnumerable<KeyValuePair<MetadataReference, IAssemblySymbol>>>("GetReferencedAssemblies", BindingFlags.NonPublic);

                   
                    

                    
                    var refs = compilation.References;    
                    
                    foreach (var referencedAssembly in referencedAssemblies)
                    {
                        var path = (referencedAssembly.Key as PortableExecutableReference)?.FilePath;
                        if (path != null)
                        {
                            interactiveAssemblyLoader.RegisterDependency(referencedAssembly.Value.Identity, path);
                        }
                    }  


                    var emitOptions = new EmitOptions()
                    .WithDebugInformationFormat(DebugInformationFormat.PortablePdb);
                    
                    emitResult = compilation.Emit(peStream, pdbStream,null,null,null,emitOptions);
                    if (emitResult.Success)
                    {
                        peStream.Position = 0;
                        pdbStream.Position = 0;
                        var assembly =
                        
                        interactiveAssemblyLoader.Invoke<Stream, Stream, Assembly>(
                            "LoadAssemblyFromStream", BindingFlags.NonPublic,
                            peStream, pdbStream);

                        var type = assembly.GetType("Submission#0");
                        var method = type.GetMethod("<Factory>", BindingFlags.Static | BindingFlags.Public);
                        var submissionStates = new object[2];
                        submissionStates[0] = null;
                        method.Invoke(null, new[] { submissionStates });
                    }                             
                }
            }            
        }            
    }


    public class NuGetMetadataResolver : MetadataReferenceResolver
    {
        private MetadataReferenceResolver resolver;
        private Nuget nuget = new Nuget();

        public NuGetMetadataResolver(MetadataReferenceResolver resolver)
        {
            this.resolver = resolver;        
        }
        
        public override bool Equals(object other)
        {
            return other.Equals(this);
        }

        public override int GetHashCode()
        {
            return resolver.GetHashCode();
        }

         public override bool ResolveMissingAssemblies
         {
             get
             {
                return this.resolver.ResolveMissingAssemblies;       
             }
         }

         public override PortableExecutableReference ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity)
         {
            return this.resolver.ResolveMissingAssembly(definition, referenceIdentity);
         }

        public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
        {
            
            if (reference.Contains("nuget"))
            {
                var array = ImmutableArray<PortableExecutableReference>.Empty;
                var files = nuget.Install(reference);
                foreach (var file in files)
                {
                    var metadataReference = PortableExecutableReference.CreateFromFile(file);
                    array = array.Add(metadataReference);
                }
                                
                return array;            
            }

            var result =  this.resolver.ResolveReference(reference,baseFilePath,properties);
            return result;
        }



    }
}
