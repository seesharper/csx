
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System.Text.RegularExpressions;
namespace csx
{
public class NugetFrameworkProvider : INuGetFrameworkProvider
    {
        public NuGetFramework GetFramework()
        {
            string frameworkName = GetFrameworkNameFromAssembly();
            NuGetFramework currentFramework = ParseFrameworkName(frameworkName);
            return currentFramework;
        }

        private static NuGetFramework ParseFrameworkName(string frameworkName)
        {
            return frameworkName == null
                            ? NuGetFramework.AnyFramework
                            : NuGetFramework.ParseFrameworkName(frameworkName, new DefaultFrameworkNameProvider());
        }

        private static string GetFrameworkNameFromAssembly()
        {
            var entryAssembly = Assembly.GetEntryAssembly();

            return Assembly.GetEntryAssembly().GetCustomAttributes()
                            .OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
                            .Select(x => x.FrameworkName)
                            .FirstOrDefault();
        }
    }

     public interface INuGetFrameworkProvider
    {
        NuGetFramework GetFramework();
    }
}