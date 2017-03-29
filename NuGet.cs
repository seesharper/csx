namespace csx
{
    using Logging;
    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Frameworks;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;

    public class Nuget
    {
        private static ILog log = Log.Create<Nuget>();
        private readonly NuGetFramework currentFramework;
        private readonly IEnumerable<PackageSource> packageSources;
        private const string PackagesFolder = "packages"; 
        private readonly PackagePathResolver packagePathResolver = new PackagePathResolver(PackagesFolder);

        public Nuget()
        {
            currentFramework = GetFramework();
            packageSources = GetPackageSources();
        }

        /// <summary>
        /// Installs a Nuget package and returns a list of all dll's to be added as metadata references.
        /// </summary>
        /// <param name="packageName">The name of the package to install.</param>
        /// <param name="version">The version of the package to install.</param>
        /// <returns></returns>
        public IEnumerable<string> Install(string nuGetReference)
        {

            PackageIdentity packageIdentity = ParseNugetReference(nuGetReference);
            if (packageIdentity == null)
            {
                return Enumerable.Empty<string>();
            }


            log.Info($"Installing package {packageIdentity.Id}, Version {packageIdentity.Version}");

            PackageSearchResult packageSearchResult = Search(packageIdentity);

            if (packageSearchResult == null)
            {
                return Enumerable.Empty<string>();
            }

            InstallPackage(packageIdentity, packageSearchResult);


            var installPath = packagePathResolver.GetInstallPath(packageIdentity);
            var packageFileName = packagePathResolver.GetPackageFileName(packageIdentity);
            PackageArchiveReader packageArchiveReader = new PackageArchiveReader(Path.Combine(installPath, packageFileName));
            var supportedFrameworks = packageArchiveReader.GetSupportedFrameworks();
            FrameworkReducer reducer = new FrameworkReducer();
            //Fallback framework

            var nearest = reducer.GetNearest(GetFramework(), supportedFrameworks);
            if (nearest == null)
            {
                nearest = supportedFrameworks.ToArray()[1];
            }


            var frameworkSpecificGroup = packageArchiveReader.GetLibItems().SingleOrDefault(i => i.TargetFramework == nearest);
            var files = frameworkSpecificGroup.Items.Select(i => i.ToLower()).Where(i => i.EndsWith("dll") && !i.EndsWith("resources.dll"));
            return files.Select(f => Path.GetFullPath(Path.Combine(installPath, f)));
        }

        private static PackageIdentity ParseNugetReference(string nuGetReference)
        {
            var regex = new Regex(@":(.+)\/(.+)");
            var match = regex.Match(nuGetReference);
            if (match.Success)
            {
                var packageName = match.Groups[1].Value;
                var version = match.Groups[2].Value;
                return new PackageIdentity(packageName, NuGetVersion.Parse(version));
            }
            return null;
        }



        private static void InstallPackage(PackageIdentity packageIdentity, PackageSearchResult packageSearchResult)
        {
            var result = Command.Execute("nuget",
                $"install {packageIdentity.Id} -source {packageSearchResult.Source.Source} -outputdirectory packages -version {packageIdentity.Version.ToString()}", ".");
            Console.WriteLine(result);
        }

        private static IEnumerable<PackageSource> GetPackageSources()
        {
            var defaultSettings = Settings.LoadDefaultSettings(Directory.GetCurrentDirectory());
            PackageSourceProvider packageSourceProvider = new PackageSourceProvider(defaultSettings);
            var packageSources = packageSourceProvider.LoadPackageSources().Where(ps => ps.IsEnabled);
            log.Info("Package sources;");
            foreach (var packageSource in packageSources)
            {
                log.Info($"{packageSource.Name} {packageSource.SourceUri}");
            }
            return packageSources;            
        }

        private static NuGetFramework GetFramework()
        {
            NugetFrameworkProvider p = new NugetFrameworkProvider();            
            var frameWork = p.GetFramework();
            log.Info($"Current framework: {frameWork.DotNetFrameworkName}");
            return frameWork;            
        }
        
        private PackageSearchResult Search(PackageIdentity packageIdentity)
        {
            foreach (var packageSource in packageSources)
            {
                var result = Search(packageSource, packageIdentity.Id, packageIdentity.Version.ToString());
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private static PackageSearchResult Search(PackageSource packageSource, string packageName, string version)
        {
            var logger = new NuGetLogger();
            //PackageSource packageSource = new PackageSource("packages");                        
            List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());  // Add v3 API support                                     
            SourceRepository sourceRepository = new SourceRepository(packageSource, providers);            
            PackageMetadataResource packageMetadataResource = sourceRepository.GetResource<PackageMetadataResource>();                                    
            var result = packageMetadataResource.GetMetadataAsync(packageName,false,false, logger, CancellationToken.None).Result;
            if (result.Any())
            {
                return new PackageSearchResult(){PackageSearchMetadata = result.LastOrDefault(), Source = packageSource};
            }
            return null;
        }

        private class PackageSearchResult
        {
            public PackageSource Source { get; set; }

            public IPackageSearchMetadata PackageSearchMetadata { get; set; }
        }
    }

    public class NuGetLogger : ILogger
    {
        public void LogDebug(string data)
        {
            Console.WriteLine(data);
        }

        public void LogError(string data)
        {
            Console.WriteLine(data);
        }

        public void LogErrorSummary(string data)
        {
            Console.WriteLine(data);
        }

        public void LogInformation(string data)
        {
            Console.WriteLine(data);
        }

        public void LogInformationSummary(string data)
        {
            Console.WriteLine(data);
        }

        public void LogMinimal(string data)
        {
            Console.WriteLine(data);
        }

        public void LogVerbose(string data)
        {
            Console.WriteLine(data);
        }

        public void LogWarning(string data)
        {
            Console.WriteLine(data);
        }
    }
}