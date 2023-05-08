using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Modularity.OptionalDependencies
{
	/// <summary>
	/// Supports Optional tags in the module dependencies like so: ModuleName[optional]
	/// This Configuration based catalog also handles attribute declared dependencies correctly.
	/// </summary>
	public class CustomConfigurationModuleCatalog : ConfigurationModuleCatalog
	{
		protected override void InnerLoad()
		{
			base.InnerLoad();
			AddAttributeDependencies(Modules); //With this the catalog supports also dependencies declared in the type itself
			OptionalDependencyHelper.HandleOptionalDependencies(Modules); //Replaces the optional dependencies with mandatory ones if the needed module is available.
		}

		private void AddAttributeDependencies(IEnumerable<IModuleInfo> modules)
		{
			foreach (IModuleInfo module in modules)
				AddAttributeDependencies(module);
		}

		private static void AddAttributeDependencies(IModuleInfo module)
		{
			var type = Type.GetType(module.ModuleType);
			var moduleDependencyAttributes =
				CustomAttributeData.GetCustomAttributes(type).Where(
				cad => cad.Constructor.DeclaringType.FullName == typeof(ModuleDependencyAttribute).FullName);
			foreach (CustomAttributeData cad in moduleDependencyAttributes)
				module.DependsOn.Add((string)cad.ConstructorArguments[0].Value);
		}
	}
}