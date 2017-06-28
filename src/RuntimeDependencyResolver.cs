using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;

namespace csx
{
    public interface IRuntimeDependencyResolver
    {
        IEnumerable<RuntimeDependency> GetRuntimeDependencies(string pathToProjectFile);
    }

    public class RuntimeDependencyResolver : IRuntimeDependencyResolver
    {
        private readonly ICommandRunner _commandRunner;
        
        private readonly ILogger _logger;

        // Note: Windows only, Mac and Linux needs something else?
        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        public RuntimeDependencyResolver(ICommandRunner commandRunner, ILoggerFactory loggerFactory)
        {
            _commandRunner = commandRunner;
            _logger = loggerFactory.CreateLogger<RuntimeDependencyResolver>();
        }

        private DependencyContext ReadDependencyContext(string pathToProjectFile)
        {
            Restore(pathToProjectFile);

            var pathToAssetsFiles = Path.Combine(Path.GetDirectoryName(pathToProjectFile), "obj", "project.assets.json");

            using (FileStream fs = new FileStream(pathToAssetsFiles, FileMode.Open, FileAccess.Read))
            {
                using (var contextReader = new DependencyContextJsonReader())
                {
                    return contextReader.Read(fs);
                }
            }
        }

        public IEnumerable<RuntimeDependency> GetRuntimeDependencies(string pathToProjectFile)
        {
            var pathToGlobalPackagesFolder = GetPathToGlobalPackagesFolder();
            var runtimeDepedencies = new HashSet<RuntimeDependency>();


            var pathToAssetsFiles = Path.Combine(Path.GetDirectoryName(pathToProjectFile), "obj", "project.assets.json");
            using (FileStream fs = new FileStream(pathToAssetsFiles, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new DependencyContextJsonReader())
                {
                    var context = reader.Read(fs);
                    var runtimeLibraries = context.RuntimeLibraries;
                                        
                    foreach (var runtimeLibrary in runtimeLibraries)
                    {
                        if (runtimeLibrary.NativeLibraryGroups.Count > 0)
                        {                            
                            foreach (var nativeLibraryGroup in runtimeLibrary.NativeLibraryGroups)
                            {                                
                                var fullPath = Path.Combine(pathToGlobalPackagesFolder, runtimeLibrary.Path, nativeLibraryGroup.AssetPaths[0]);
                                Console.WriteLine(fullPath);
                                _logger.LogInformation(fullPath);
                                LoadLibrary(fullPath);            
                            }
                        }

                        


                        if (runtimeLibrary.RuntimeAssemblyGroups.Count > 0)
                        {
                            var path = runtimeLibrary.Path;
                            foreach (var runtimeLibraryRuntimeAssemblyGroup in runtimeLibrary.RuntimeAssemblyGroups)
                            {
                                if (runtimeLibraryRuntimeAssemblyGroup.Runtime == "win" ||
                                    runtimeLibraryRuntimeAssemblyGroup.Runtime == "")
                                {
                                    path = Path.Combine(path, runtimeLibraryRuntimeAssemblyGroup.AssetPaths[0]);
                                    if (!path.EndsWith("_._"))
                                    {
                                        var fullPath = Path.Combine(pathToGlobalPackagesFolder, path);
                                        Console.WriteLine(fullPath);
                                        _logger.LogInformation(fullPath);
                                        runtimeDepedencies.Add(new RuntimeDependency(runtimeLibrary.Name, fullPath));
                                    }

                                }
                            }
                        }
                    }

                }
            }
            return runtimeDepedencies;
        }

        private void Restore(string pathToProjectFile)
        {
            _commandRunner.Execute("DotNet", $"restore {pathToProjectFile} -r win7-x64");
        }

        private string GetPathToGlobalPackagesFolder()
        {
            var result = _commandRunner.Execute("dotnet", "nuget locals global-packages -l");
            var match = Regex.Match(result, @"global-packages:\s*(.*)\r");
            return match.Groups[1].Captures[0].ToString();
        }

    }

    public class RuntimeDependency
    {
        public string Name { get; }
        public string Path { get; }

        public RuntimeDependency(string name, string path)
        {
            Name = name;
            Path = path;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Path.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var other = (RuntimeDependency)obj;
            return other.Name == Name && other.Path == Path;
        }
    }
}