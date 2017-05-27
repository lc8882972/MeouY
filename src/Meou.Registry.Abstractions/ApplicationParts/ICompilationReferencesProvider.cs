using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Registry.Abstractions
{
    /// <summary>
    /// Exposes one or more reference paths from an <see cref="ApplicationPart"/>.
    /// </summary>
    public interface ICompilationReferencesProvider
    {
        /// <summary>
        /// Gets reference paths used to perform runtime compilation.
        /// </summary>
        IEnumerable<string> GetReferencePaths();
    }
}
