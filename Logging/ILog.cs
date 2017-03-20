using System;

namespace csx.Logging
{
    /// <summary>
    ///     Represents a logging class.
    /// </summary>
    public interface ILog
    {
        /// <summary>
        ///     Writes a debug message to the log.
        /// </summary>
        /// <param name="message">The message to be written to the log.</param>
        void Debug(string message);

        /// <summary>
        ///     Writes an informational message to the log.
        /// </summary>
        /// <param name="message">The message to be written to the log.</param>
        void Info(string message);

        /// <summary>
        ///     Writes an warning message to the log.
        /// </summary>
        /// <param name="message">The message to be written to the log.</param>
        void Warn(string message);

        /// <summary>
        ///     Writes an error message to the log.
        /// </summary>
        /// <param name="message">The message to be written to the log.</param>
        /// <param name="exception">Optionally the <see cref="Exception" /> that caused the error.</param>
        void Error(string message, Exception exception = null);
    }
}
