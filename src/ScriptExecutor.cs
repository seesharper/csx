using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;

namespace csx
{   
    public class ScriptExecutor
    {
        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);


        private readonly IScriptProjectProvider _scriptProjectProvider;
        private readonly IRuntimeDependencyResolver _runtimeDependencyResolver;
        private readonly ILogger logger;

        public ScriptExecutor(IScriptProjectProvider scriptProjectProvider, IRuntimeDependencyResolver runtimeDependencyResolver, ILoggerFactory loggerFactory)
        {
            this._scriptProjectProvider = scriptProjectProvider;
            _runtimeDependencyResolver = runtimeDependencyResolver;
            this.logger = loggerFactory.CreateLogger<ScriptExecutor>();
        }

        public void Execute(string pathToScript, string[] args)
        {
            //LoadLibrary(
            //    @"C:\Users\bri\.nuget\packages\runtime.win7-x64.runtime.native.system.data.sqlclient.sni\4.3.0\runtimes\win7-x64\native\sni.dll");

            if (!Path.IsPathRooted(pathToScript))
            {
                pathToScript = Path.GetFullPath(pathToScript);
            }
            string codeAsPlainText = null;
            using (var fileStream = new FileStream(pathToScript, FileMode.Open))
            {
                // We need to create a SourceText instance with an encoding
                var encodedSourceText = SourceText.From(fileStream, Encoding.UTF8);
                codeAsPlainText = encodedSourceText.ToString();
            }
            
            
            var scriptOptions = CreateScriptOptions(pathToScript);

            var globals = new CommandLineScriptGlobals(Console.Out, CSharpObjectFormatter.Instance);
            foreach (var arg in args)
            {
                globals.Args.Add(arg);
            }

            logger.LogInformation("Creating script");
            var interactiveAssemblyLoader = new InteractiveAssemblyLoader();            
           
            var script = CSharpScript.Create(codeAsPlainText, scriptOptions, typeof(CommandLineScriptGlobals),
                interactiveAssemblyLoader);
                                                                        
            var warnings = script.GetCompilation().GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Warning);
            foreach (var warning in warnings)
            {                
                logger.LogWarning(warning.ToString());
            }

            var errors = script.GetCompilation().GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error);
            foreach (var error in errors)
            {
                logger.LogError(error.ToString());
            }


            RunScript(script, globals);
        }

       


        private void RunScript(Script<object> script, CommandLineScriptGlobals globals)
        {
            var scriptState = script.RunAsync(globals, exception => true).Result;
            if (scriptState.Exception != null)
            {
                logger.LogError(scriptState.Exception.ToString());
                throw scriptState.Exception;
            }           
        }

       

        private ScriptOptions CreateScriptOptions(string pathToScript)
        {
            string[] imports =
            {
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
            
            scriptOptions = AddMetadataReferences(scriptOptions, pathToScript);
            return scriptOptions
                .WithEmitDebugInformation(true)
                .WithFileEncoding(Encoding.UTF8)
                .WithFilePath(pathToScript)
                .WithImports(imports)                               
                .WithMetadataResolver(new NuGetMetadataReferenceResolver(ScriptMetadataResolver.Default));
        }

        private ScriptOptions AddMetadataReferences(ScriptOptions options, string pathToScript)
        {
            var targetDirectory = Path.GetDirectoryName(pathToScript);
            string pathToProjectFile = _scriptProjectProvider.CreateProject(targetDirectory);
            List<RuntimeDependency> runtimeDependencies = _runtimeDependencyResolver.GetRuntimeDependencies(pathToProjectFile).ToList();           
            var references = runtimeDependencies.Select(r => MetadataReference.CreateFromFile(r.Path));
            AssemblyLoadContext.Default.Resolving +=
                (context, assemblyName) => MapUnresolvedAssemblyToRuntimeLibrary(runtimeDependencies ,context, assemblyName);
            return options.WithReferences(references);           
        }

        private Assembly MapUnresolvedAssemblyToRuntimeLibrary(IList<RuntimeDependency> runtimeDependencies, AssemblyLoadContext loadContext, AssemblyName assemblyName)
        {
            var runtimeDependency = runtimeDependencies.SingleOrDefault(r => r.Name == assemblyName.Name);
            if (runtimeDependency != null)
            {
                logger.LogInformation($"Unresolved assembly {assemblyName}. Loading from resolved runtime dependencies at path: {runtimeDependency.Path}");
                return loadContext.LoadFromAssemblyPath(runtimeDependency.Path);
            }
            return null;
        }
    }   
}
