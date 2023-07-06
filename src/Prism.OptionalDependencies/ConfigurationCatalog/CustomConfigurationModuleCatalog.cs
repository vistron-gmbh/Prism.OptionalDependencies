using Prism.Modularity;
using Prism.Modularity.OptionalDependencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vistron.Logging;

namespace Prism.Modularity.OptionalDependencies
{
    public partial class CustomConfigurationModuleCatalog : ConfigurationModuleCatalog
    {
        private readonly ILogger _logger;

        [Obsolete("Please use the logger ctor. It logs exceptions during module inspection.")]
        public CustomConfigurationModuleCatalog()
        {
        }

        public CustomConfigurationModuleCatalog(ILogger logger)
        {
            _logger = logger;
        }

        protected override void InnerLoad()
        {
            base.InnerLoad();
            AddAttributeDependencies(Modules, _logger); //With this the catalog supports also dependencies declared in the type itself
            OptionalDependencyHelper.HandleOptionalDependencies(Modules); //Replaces the optional dependencies with mandatory ones if the needed module is available.
        }

        private void AddAttributeDependencies(IEnumerable<IModuleInfo> modules, ILogger logger)
        {
            try
            {
                AddAttributeDependencies(modules);
            }
            catch (AggregateException ex)
            {
                logger?.LogException(ex);
            }
        }
    }
}