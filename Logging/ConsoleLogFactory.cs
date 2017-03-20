using System;
using System.Collections.Generic;
using System.Text;

namespace csx.Logging
{
    using System.Diagnostics;

    public class ConsoleLogFactory : ILogFactory
    {
        /// <summary>
        ///     Gets an <see cref="ILog" /> instance.
        /// </summary>
        /// <param name="type">The <see cref="Type" /> for which to create an <see cref="ILog" /> instance.</param>
        /// <returns>An <see cref="ILog" /> instance that targets the given <paramref name="type" />.</returns>
        public ILog GetLogger(Type type)
        {
            return new Log(
                s => Console.WriteLine($"Info: {s}"),
                s => Console.WriteLine($"Debug: {s}"),
                (s, exception) =>
                    Console.WriteLine(exception == null ? $"Error: {s}" : $"Error: {s} {exception}"), s => Debug.WriteLine($"Warning: {s}"));
        }
    }
}
