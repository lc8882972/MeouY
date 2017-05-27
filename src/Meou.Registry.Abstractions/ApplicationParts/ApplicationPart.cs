using System;
using System.Collections.Generic;
using System.Text;

namespace Meou.Registry.Abstractions
{
    /// <summary>
    /// A part of an MVC application.
    /// </summary>
    public abstract class ApplicationPart
    {
        /// <summary>
        /// Gets the <see cref="ApplicationPart"/> name.
        /// </summary>
        public abstract string Name { get; }
    }
}
