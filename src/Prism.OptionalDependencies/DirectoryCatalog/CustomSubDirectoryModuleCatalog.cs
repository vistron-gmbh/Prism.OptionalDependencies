#if !NET5_0_OR_GREATER

using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Modularity.OptionalDependencies
{
    /// <summary>
    /// Supports Optional tags in the module dependencies like so: ModuleName[optional] and SubDirectories.
    /// </summary>
    public class CustomSubDirectoryModuleCatalog : SubDirectoryModuleCatalog
    {
        protected override void InnerLoad()
        {
            base.InnerLoad();
            OptionalDependencyHelper.HandleOptionalDependencies(Modules); //Replaces the optional dependencies with mandatory ones if the needed module is available.
        }
    }
}

#endif