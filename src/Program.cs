using System;

namespace csx
{
    using Dotnet.Script.NuGetMetadataResolver;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;
    using Microsoft.Extensions.CommandLineUtils;


    class Program
    {
        static void Main(string[] args)
        {
            var cli = new CommandLineApplication();
            cli.Description = "C# script runner for .Net Core with debug and NuGetCommand support";
            cli.HelpOption("-? | -h | --help");

                       
            cli.Command("init", config =>
            {
                config.Description = "Creates a new script and the launch.json file needed to debug the script.";
                var fileNameArgument = config.Argument("filename", "The script file name");
                config.OnExecute(() =>
                {
                    var skaffolder = new Skaffolder();
                    skaffolder.InitializerFolder(fileNameArgument.Value);
                    return 0;
                });
            });

            var file = cli.Argument("script", "The path to the script to be executed");

            cli.OnExecute(() =>
            {
                if (string.IsNullOrWhiteSpace(file.Value) )
                {
                    cli.ShowHelp();
                    return 0;
                }
                var scriptExecutor = CreateScriptExecutor();
                scriptExecutor.Execute(file.Value);                
                return 0;
            });
           

            cli.Execute(args);
        }

        private static ScriptExecutor CreateScriptExecutor()
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(
                new ConsoleLoggerProvider(
                    (text, logLevel) => logLevel >= LogLevel.Information, true));
            return new ScriptExecutor(ScriptProjectProvider.Create(loggerFactory),loggerFactory);

        }
    }  
}
