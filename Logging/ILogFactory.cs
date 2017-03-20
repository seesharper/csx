using System;
using System.Collections.Generic;
using System.Text;

namespace csx.Logging
{
    using System;

    /// <summary>
    /// Represents a class that is capable of creating an <see cref="ILog" /> instance.
    /// </summary>
    public interface ILogFactory
    {
        /// <summary>
        /// Gets an <see cref="ILog" /> instance.
        /// </summary>
        /// <param name="type">The <see cref="Type" /> for which to create an <see cref="ILog" /> instance.</param>
        /// <returns>An <see cref="ILog" /> instance that targets the given <paramref name="type" />.</returns>
        ILog GetLogger(Type type);
    }
}
