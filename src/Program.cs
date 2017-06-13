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
            var debugOption = cli.Option("-d | --debug", "Outputs debug messages to the console", CommandOptionType.NoValue);
                       
            cli.Command("init", config =>
            {
                config.Description = "Creates the launch.json file and the tasks.json file needed to launch and debug the script.";                
                config.OnExecute(() =>
                {
                    var skaffolder = new Skaffolder();
                    skaffolder.InitializerFolder();
                    return 0;
                });
            });

            cli.Command("new", config =>
            {
                config.Description = "Creates a new script file";
                var fileNameArgument = config.Argument("filename", "The script file name");                
                config.OnExecute(() =>
                {                    
                    var skaffolder = new Skaffolder();
                    if (fileNameArgument.Value == null)
                    {
                        config.ShowHelp();
                        return 0;
                    }
                    skaffolder.CreateNewScriptFile(fileNameArgument.Value);
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
                var scriptExecutor = CreateScriptExecutor(debugOption.HasValue());
                scriptExecutor.Execute(file.Value);                
                return 0;
            });
           

            cli.Execute(args);
        }

        private static ScriptExecutor CreateScriptExecutor(bool debug)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(
                new ConsoleLoggerProvider(
                    (text, logLevel) => logLevel >= (debug ? LogLevel.Debug : LogLevel.Warning), true));
            return new ScriptExecutor(ScriptProjectProvider.Create(loggerFactory),loggerFactory);

        }
    }  
}
