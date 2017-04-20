namespace csx
{
    using System;
    using System.IO;
    using System.Reflection;

    public class Skaffolder
    {
        public void InitializerFolder(string pathToScript)
        {
            string baseDirectory = Path.GetDirectoryName(typeof(Skaffolder).GetTypeInfo().Assembly.CodeBase);
            string currentDirectory = Directory.GetCurrentDirectory();
            string vsCodeDirectory = Path.Combine(currentDirectory, ".vscode");
            if (!Directory.Exists(vsCodeDirectory))
            {
                Directory.CreateDirectory(vsCodeDirectory);
            }

            string pathToLaunchFile = Path.Combine(vsCodeDirectory, "launch.json");

        }
    }
}