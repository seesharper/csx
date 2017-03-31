using System;
using System.Text;
using System.Reflection;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace csx
{
    using System.Threading;
    using Dotnet.Script.NuGetMetadataResolver;
    using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
    using Microsoft.DotNet.InternalAbstractions;
    using Microsoft.Extensions.DependencyModel;

    class Program
    {
        static void Main(string[] args)
        {                                                                      
            var interactiveAssemblyLoader = new InteractiveAssemblyLoader();            
            string pathToScript = Path.GetFullPath(args[0]);
            string rootFolder = Path.GetDirectoryName(pathToScript);


            string codeAsPlainText = null;            
            using(var fileStream = new FileStream(pathToScript, FileMode.Open))
            {
                // We need to create a SourceText instance with an encoding
                var encodedSourceText = SourceText.From(fileStream, Encoding.UTF8);
                codeAsPlainText = encodedSourceText.ToString();
            }

            var scriptOptions = ScriptOptions.Default.WithFilePath(pathToScript);
            
            var resolver = NuGetMetadataReferenceResolver.Create(scriptOptions.MetadataResolver, NugetFrameworkProvider.GetFrameworkNameFromAssembly(), rootFolder);
            
            scriptOptions = scriptOptions.WithMetadataResolver(resolver);
            scriptOptions = scriptOptions.WithEmitDebugInformation(true);
            scriptOptions = scriptOptions.WithFileEncoding(Encoding.UTF8);            
            

            InteractiveScriptGlobals globals = new InteractiveScriptGlobals(Console.Out, CSharpObjectFormatter.Instance);
            foreach (var arg in args)
            {
                globals.Args.Add(arg);
            }

            var runtimeId = RuntimeEnvironment.GetRuntimeIdentifier();
            var inheritedAssemblyNames = DependencyContext.Default.GetRuntimeAssemblyNames(runtimeId).Where(x =>
                x.FullName.StartsWith("system.", StringComparison.OrdinalIgnoreCase) ||
                x.FullName.StartsWith("microsoft.codeanalysis", StringComparison.OrdinalIgnoreCase) ||
                x.FullName.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase));

            foreach (var inheritedAssemblyName in inheritedAssemblyNames)
            {                
                var assembly = Assembly.Load(inheritedAssemblyName);
                scriptOptions = scriptOptions.AddReferences(assembly);
            }

            var script = CSharpScript.Create(codeAsPlainText, scriptOptions, typeof(InteractiveScriptGlobals), interactiveAssemblyLoader);
            Console.WriteLine(script.RunAsync(globals, CancellationToken.None).Result);                       
        }            
    }


  
}
