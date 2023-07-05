using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Modularity.OptionalDependencies
{
    public partial class CustomConfigurationModuleCatalog : ConfigurationModuleCatalog
    {
        //The net6 Version uses AssemblyLoadContext to inspect the Module Attributes therefore not interfering with the module load order. The load order was changed before 1.2.0.

        private void AddAttributeDependencies(IEnumerable<IModuleInfo> modules)
        {
            foreach (IModuleInfo module in modules)
                AddAttributeDependencies(module);
        }

        private void AddAttributeDependencies(IModuleInfo module)
        {
            try
            {
                var moduleTypeCleaned = new string(module.ModuleType.TakeWhile(x => x != ',').ToArray()); //We need to remove the Namespace end for some reason.
                AssemblyLoadContext customLoadContext = new AssemblyLoadContext("tempLoadContextModuleDependencyCheck", true);

                var assembly = customLoadContext.LoadFromAssemblyPath(module.Ref);

                Type moduleType = assembly.GetType(moduleTypeCleaned);

                var moduleDependencyAttributes =
                    CustomAttributeData.GetCustomAttributes(moduleType).Where(
                    cad => cad.Constructor.DeclaringType.FullName == typeof(ModuleDependencyAttribute).FullName);

                foreach (CustomAttributeData cad in moduleDependencyAttributes)
                    module.DependsOn.Add((string)cad.ConstructorArguments[0].Value);

                customLoadContext.Unload();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
    }
}