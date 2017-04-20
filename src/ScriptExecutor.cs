﻿namespace csx
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using Dotnet.Script.NuGetMetadataResolver;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
    using Microsoft.CodeAnalysis.Scripting;
    using Microsoft.CodeAnalysis.Scripting.Hosting;
    using Microsoft.CodeAnalysis.Text;
    using Microsoft.DotNet.InternalAbstractions;
    using Microsoft.DotNet.ProjectModel;
    using Microsoft.Extensions.DependencyModel;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;

    public class ScriptExecutor
    {
        private readonly IScriptProjectProvider scriptProjectProvider;
        private readonly ILogger logger;

        public ScriptExecutor(IScriptProjectProvider scriptProjectProvider, ILoggerFactory loggerFactory)
        {
            this.scriptProjectProvider = scriptProjectProvider;
            this.logger = loggerFactory.CreateLogger<ScriptExecutor>();
        }

        public void Execute(string pathToScript)
        {
            var interactiveAssemblyLoader = new InteractiveAssemblyLoader();
            
            string codeAsPlainText = null;
            using (var fileStream = new FileStream(pathToScript, FileMode.Open))
            {
                // We need to create a SourceText instance with an encoding
                var encodedSourceText = SourceText.From(fileStream, Encoding.UTF8);
                codeAsPlainText = encodedSourceText.ToString();
            }

            var scriptOptions = CreateScriptOptions(pathToScript);


            var globals = new InteractiveScriptGlobals(Console.Out, CSharpObjectFormatter.Instance);
            //foreach (var arg in args)
            //{
            //    globals.Args.Add(arg);
            //}

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

            var diag = script.GetCompilation().GetDiagnostics();
            Console.WriteLine(script.RunAsync(globals, CancellationToken.None).Result);
        }


        private ScriptOptions CreateScriptOptions(string pathToScript)
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
            string pathToProjectJson = scriptProjectProvider.CreateProject(targetDirectory);

            List<string> runtimeDependencies = new List<string>();
            var context = ProjectContext.CreateContextForEachTarget(pathToProjectJson);
            var exporter = context.First().CreateExporter("Release");
            var dependencies = exporter.GetDependencies();
            foreach (var projectDependency in dependencies)
            {
                var runtimeAssemblies = projectDependency.RuntimeAssemblyGroups;

                foreach (var runtimeAssembly in runtimeAssemblies.GetDefaultAssets())
                {
                    var runtimeAssemblyPath = runtimeAssembly.ResolvedPath;
                    logger.LogInformation($"Discovered runtime dependency for '{runtimeAssemblyPath}'");
                    runtimeDependencies.Add(runtimeAssemblyPath);                    
                }
            }
            return options.AddReferences(runtimeDependencies);
        }
    }
}