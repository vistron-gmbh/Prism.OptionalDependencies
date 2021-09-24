using Prism.Modularity.OptionalDependencies.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Prism.Modularity.OptionalDependencies
{
    /// <summary>
    /// Identifies the [optional] tag inside strings which are used as module dependencies and will either remove the dependency if the needed module is not available or keep it with the optional tag removed.
    /// </summary>
    public static class OptionalDependencyHelper
    {
        private static Regex _OPTIONAL_TAG_REGEX = new Regex("(.*?)(\\[optional\\])");  //Captures the name itself and a possible optional tag in the form of [optional]

        /// <summary>
        /// Checks if the optional tag is set and will remove it from the dependency if found.
        /// </summary>
        /// <param name="dependencyName"></param>
        /// <returns></returns>
        public static bool IsOptional(ref string dependencyName)
        {
            var match = _OPTIONAL_TAG_REGEX.Match(dependencyName);  //Captures the name itself and a possible optional tag in the form of [optional]
            if (match.Success)
            {
                dependencyName = match.Groups[1].Value;
                return true;
            }
            else
                return false;
        }

        public static void HandleOptionalDependencies(IEnumerable<IModuleInfo> modules)
        {
            var knownModules = modules.Select(x => x.ModuleName).ToList();
            foreach (var module in modules)
                ReplaceOptionalDependencies(module, knownModules);
        }

        private static void ReplaceOptionalDependencies(IModuleInfo module, ICollection<string> knownModules)
        {
            module.DependsOn.ForEachIndexed((dependency, i) =>
            {
                string current = dependency;
                if (IsOptional(ref current))
                {
                    module.DependsOn.Remove(dependency); //We must remove it in any case at it contains the tag
                    if (knownModules.Contains(current)) //We only add it back (without the tag) when the dependency is actually solvable.
                        module.DependsOn[i] = current;
                }
            });
        }
    }
}
