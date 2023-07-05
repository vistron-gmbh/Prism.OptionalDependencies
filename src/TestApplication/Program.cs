using Prism.Modularity;
using Prism.Modularity.OptionalDependencies;
using System;
using System.IO;
using System.Threading;

namespace TestApplication
{
    public class Program
    {
        private static void Main(string[] args)
        {
            CustomConfigurationModuleCatalog customCatalog = new CustomConfigurationModuleCatalog();

            var workflowPath = @"D:\temp\GMD Test\20-9904-VIF-GapMeasuring\bin\Debug\modules\Vistron.Workflow.Core\Vistron.Workflow.Core.dll";
            workflowPath = Path.GetFullPath(workflowPath);

            ModuleInfo worklfowCoreModule = new ModuleInfo("WorklfowCore", "Vistron.Workflow.Core.WorkflowCoreModule, Vistron.Workflow.Core")
            {
                InitializationMode = InitializationMode.WhenAvailable,
                Ref = workflowPath
            };

            customCatalog.AddModule(worklfowCoreModule);

            customCatalog.Initialize();

            Console.ReadKey();
        }
    }
}