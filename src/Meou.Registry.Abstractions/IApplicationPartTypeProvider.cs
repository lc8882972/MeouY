using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Meou.Registry.Abstractions
{
    /// <summary>
    /// Exposes a set of types from an <see cref="ApplicationPart"/>.
    /// </summary>
    public interface IApplicationPartTypeProvider
    {
        /// <summary>
        /// Gets the list of available types in the <see cref="ApplicationPart"/>.
        /// </summary>
        IEnumerable<TypeInfo> Types { get; }
    }
}
