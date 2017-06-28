using System.IO;
using System.Reflection;

namespace csx
{
    public static class ResourceLoader
    {
        public static string ReadResourceFile(string name)
        {
            var resourceStream = typeof(ResourceLoader).GetTypeInfo().Assembly.GetManifestResourceStream($"csx.Templates.{name}");
            using (var streamReader = new StreamReader(resourceStream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}