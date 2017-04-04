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
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Dotnet.Script.NuGetMetadataResolver;
    using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
    using Microsoft.DotNet.InternalAbstractions;
    using Microsoft.Extensions.DependencyModel;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;

    class Program
    {
        static void Main(string[] args)
        {                                                                      
            var interactiveAssemblyLoader = new InteractiveAssemblyLoader();            
            string pathToScript = Path.GetFullPath(args[0]);
            

            string codeAsPlainText = null;            
            using(var fileStream = new FileStream(pathToScript, FileMode.Open))
            {
                // We need to create a SourceText instance with an encoding
                var encodedSourceText = SourceText.From(fileStream, Encoding.UTF8);
                codeAsPlainText = encodedSourceText.ToString();
            }

            var scriptOptions = CreateScriptOptions(pathToScript);


            var globals = new InteractiveScriptGlobals(Console.Out, CSharpObjectFormatter.Instance);
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

        private static ScriptOptions CreateScriptOptions(string pathToScript)
        {
            string[] imports = {
                "System",
                "System.IO",
                "System.Collections.Generic",
                "System.Console",
                "System.Diagnostics",
                "System.Dynamic",
                "System.Linq",
                "System.Linq.Expressions",
                "System.Text",
                "System.Threading.Tasks"
            };
            
            var scriptOptions = ScriptOptions.Default;
            return scriptOptions
                .WithEmitDebugInformation(true)
                .WithFileEncoding(Encoding.UTF8)
                .WithMetadataResolver(CreateNuGetMetadataResolver(pathToScript, scriptOptions))
                .WithFilePath(pathToScript)
                .WithImports(imports);
        }

        private static NuGetMetadataReferenceResolver CreateNuGetMetadataResolver(string pathToScript, ScriptOptions scriptOptions)
        {
            var loggerFactory = new LoggerFactory();            
            loggerFactory.AddProvider(
                new ConsoleLoggerProvider(
                    (text, logLevel) => logLevel >= LogLevel.Information, true));            
            string rootFolder = Path.GetDirectoryName(pathToScript);
            return NuGetMetadataReferenceResolver.Create(scriptOptions.MetadataResolver,
                NugetFrameworkProvider.GetFrameworkNameFromAssembly(),loggerFactory, rootFolder);
        }
    }


  
}
