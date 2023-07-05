using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Vistron.Logging;
using Vistron.Tools.Assemblies;

namespace Prism.Modularity.OptionalDependencies
{
    /// <summary>
    /// Supports Optional tags in the module dependencies like so: ModuleName[optional]
    /// This Configuration based catalog also handles attribute declared dependencies correctly.
    /// </summary>
    public class CustomConfigurationModuleCatalog : ConfigurationModuleCatalog
    {
        private readonly ILogger _logger;

        public CustomConfigurationModuleCatalog()
        {
            _logger = new NullLogger();
        }
        public CustomConfigurationModuleCatalog(ILogger logger)
        {
            _logger = logger;
        }

        protected override void InnerLoad()
        {
            base.InnerLoad();
            AddAttributeDependencies(Modules); //With this the catalog supports also dependencies declared in the type itself
            OptionalDependencyHelper.HandleOptionalDependencies(Modules); //Replaces the optional dependencies with mandatory ones if the needed module is available.
        }

#if NET5_0_OR_GREATER

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
#else

        //The net48 Version uses AppDomains to inspect the Module Attributes therefore not interfering with the module load order. The load order was changed before 1.2.0.

        private void AddAttributeDependencies(IEnumerable<IModuleInfo> modules)
        {
            var temporaryAppDomain = AppDomain.CreateDomain("TemporaryOptionalDependencyDomain");

            foreach (IModuleInfo module in modules)
                AddAttributeDependencies(module, temporaryAppDomain);

            AppDomain.Unload(temporaryAppDomain);
        }

        private void AddAttributeDependencies(IModuleInfo module, AppDomain temporaryAppDomain)
        {
            try
            {
                temporaryAppDomain.SetData("module", module); //There are other ways to get data into the method, e.g. by building a serializable Host class, but this seems simpler.
                temporaryAppDomain.DoCallBack(AddAttributeDependenciesCrossAppDomain); //One cannot use lamba here as the classes generated are not serializable.
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }

        private static void AddAttributeDependenciesCrossAppDomain() //This method needs to be static and parameterless so we can serialize it into the temp appdomain
        {
            var module = (IModuleInfo)AppDomain.CurrentDomain.GetData("module"); //This is called inside a callback of another domain, therefore this call gets to this temp domain

            AddAttributeDependencies(module);
        }

        private static void AddAttributeDependencies(IModuleInfo module)
        {
            // Load and inspect the assembly
            Uri uri = new Uri(module.Ref);
            var testCurrent = AppDomain.CurrentDomain.FriendlyName;

            //var assemblyName = AssemblyName.GetAssemblyName(uri.LocalPath);
            var assembly = Assembly.LoadFrom(uri.LocalPath);

            var moduleTypeCleaned = new string(module.ModuleType.TakeWhile(x => x != ',').ToArray()); //We need to remove the Namespace end for some reason.

            Type moduleType = assembly.GetType(moduleTypeCleaned);

            var moduleDependencyAttributes =
                CustomAttributeData.GetCustomAttributes(moduleType).Where(
                cad => cad.Constructor.DeclaringType.FullName == typeof(ModuleDependencyAttribute).FullName);

            foreach (CustomAttributeData cad in moduleDependencyAttributes)
                module.DependsOn.Add((string)cad.ConstructorArguments[0].Value);
        }
#endif
    }
}