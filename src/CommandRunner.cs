using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace csx
{
    /// <summary>
    /// A class that is capable of running a command.
    /// </summary>
    public class CommandRunner : ICommandRunner
    {
        private readonly ILogger logger;
        private readonly StringBuilder lastStandardErrorOutput = new StringBuilder();
        private readonly StringBuilder lastProcessOutput = new StringBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRunner"/> class.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create an <see cref="ILogger"/> instance.</param>
        public CommandRunner(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<CommandRunner>();
        }

        /// <inheritdoc />
        public string Execute(string commandPath, string arguments)
        {
            lastStandardErrorOutput.Clear();

            logger.LogInformation($"Executing {commandPath} {arguments}");
            var startInformation = CreateProcessStartInfo(commandPath, arguments);
            var process = CreateProcess(startInformation);            
            RunAndWait(process);
            logger.LogInformation(lastProcessOutput.ToString());            
            if (process.ExitCode != 0)
            {
                logger.LogError(lastStandardErrorOutput.ToString());
                throw new InvalidOperationException($"The command {commandPath} {arguments} failed to execute");
            }
            return lastProcessOutput.ToString();
        }

        private static ProcessStartInfo CreateProcessStartInfo(string commandPath, string arguments)
        {
            var startInformation = new ProcessStartInfo(commandPath);
            startInformation.CreateNoWindow = true;
            startInformation.Arguments = arguments;
            startInformation.RedirectStandardOutput = true;
            startInformation.RedirectStandardError = true;
            startInformation.UseShellExecute = false;
            return startInformation;
        }

        private Process CreateProcess(ProcessStartInfo startInformation)
        {
            var process = new Process();
            process.StartInfo = startInformation;
            process.ErrorDataReceived += (s, a) =>
            {
                if (!string.IsNullOrWhiteSpace(a.Data))
                {
                    lastStandardErrorOutput.AppendLine(a.Data);
                }
                
            };
            process.OutputDataReceived += (s, a) =>
            {                
                lastProcessOutput.AppendLine(a.Data);
            };
            return process;
        }

        private static void RunAndWait(Process process)
        {
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }
    }
}