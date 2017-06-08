namespace csx
{
    using System;
    using System.IO;
    using System.Reflection;

    public class Skaffolder
    {
        public void InitializerFolder()
        {            
            string currentDirectory = Directory.GetCurrentDirectory();
            string vsCodeDirectory = Path.Combine(currentDirectory, ".vscode");
            if (!Directory.Exists(vsCodeDirectory))
            {
                Directory.CreateDirectory(vsCodeDirectory);
            }

            string pathToLaunchFile = Path.Combine(vsCodeDirectory, "launch.json");
            if (!File.Exists(pathToLaunchFile))
            {
                string baseDirectory = Path.GetDirectoryName(new Uri(typeof(Skaffolder).GetTypeInfo().Assembly.CodeBase).LocalPath);
                string csxPath = Path.Combine(baseDirectory, "csx.exe").Replace(@"\","/");
                string lauchFileTemplate = ReadResourceFile("launch.json.template");
                string launchFileContent = lauchFileTemplate.Replace("PATH_TO_CSX", csxPath);
                WriteFile(pathToLaunchFile, launchFileContent);
            }

            string pathToTasksFile = Path.Combine(vsCodeDirectory, "tasks.json");
            if (!File.Exists(pathToTasksFile))
            {
                string taskFileTemplate = ReadResourceFile("tasks.json.template");
                WriteFile(pathToTasksFile, taskFileTemplate);
            }

            string pathToOmniSharpJson = Path.Combine(currentDirectory, "omnisharp.json");
            if (!File.Exists(pathToOmniSharpJson))
            {
                var omniSharpFileTemplate = ReadResourceFile("omnisharp.json.template");
                WriteFile(pathToOmniSharpJson, omniSharpFileTemplate);
            }
        }

        public void CreateNewScriptFile(string file)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            var pathToScriptFile = Path.Combine(currentDirectory, file);
            if (!File.Exists(pathToScriptFile))
            {
                var scriptFileTemplate = ReadResourceFile("script.csx.template");
                WriteFile(pathToScriptFile, scriptFileTemplate);
            }
        }

        private static string ReadResourceFile(string name)
        {
            var resourceStream = typeof(Skaffolder).GetTypeInfo().Assembly.GetManifestResourceStream($"csx.Templates.{name}");
            using (var streamReader = new StreamReader(resourceStream))
            {
                return streamReader.ReadToEnd();
            }
        }

        private void WriteFile(string path, string content)
        {
            using (var fileStream = new FileStream(path,FileMode.Create))
            {
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(content);
                }
            }
        }
    }
}