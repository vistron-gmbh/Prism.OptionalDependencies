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

namespace Prism.Modularity.OptionalDependencies
{
    /// <summary>
    /// Supports Optional tags in the module dependencies like so: ModuleName[optional]
    /// This Configuration based catalog also handles attribute declared dependencies correctly.
    /// </summary>
    public partial class CustomConfigurationModuleCatalog : ConfigurationModuleCatalog
    {
        //The net48 Version uses AppDomains to inspect the Module Attributes therefore not interfering with the module load order. The load order was changed before 1.2.0.

        private static void AddAttributeDependencies(IEnumerable<IModuleInfo> modules)
        {
            var temporaryAppDomain = AppDomain.CreateDomain("TemporaryOptionalDependencyDomain");
            List<Exception> exceptions = new List<Exception>();
            foreach (IModuleInfo module in modules)
            {
                try
                {
                    AddAttributeDependencies(module, temporaryAppDomain);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            AppDomain.Unload(temporaryAppDomain);

            if (exceptions.Any())
                throw new AggregateException("Exceptions during attribute dependency addition", exceptions);
        }

        private static void AddAttributeDependencies(IModuleInfo module, AppDomain temporaryAppDomain)
        {
            temporaryAppDomain.SetData("module", module); //There are other ways to get data into the method, e.g. by building a serializable Host class, but this seems simpler.
            temporaryAppDomain.DoCallBack(AddAttributeDependenciesCrossAppDomain); //One cannot use lamba here as the classes generated are not serializable.
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
    }
}