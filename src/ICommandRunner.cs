namespace csx
{
    /// <summary>
    /// Represents a class that is capable of running a command.
    /// </summary>
    public interface ICommandRunner
    {
        /// <summary>
        /// Executes the command identified by the <paramref name="commandPath"/>
        /// with the given <paramref name="arguments"/>.
        /// </summary>
        /// <param name="commandPath">The command to be executed.</param>
        /// <param name="arguments">The arguments to be passed to the command.</param>
        string Execute(string commandPath, string arguments);
    }
}