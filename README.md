[![.github/workflows/CallPublicVistronRepoChecks.yml](https://github.com/vistron-gmbh/Prism.OptionalDependencies/actions/workflows/CallPublicVistronRepoChecks.yml/badge.svg)](https://github.com/vistron-gmbh/Prism.OptionalDependencies/actions/workflows/CallPublicVistronRepoChecks.yml)
# Prism.OptionalDependencies

*This package provides some custom Prism ModuleCatalogs which allows optional module dependencies for Prism Modules as well as an `OptionalDependencyHelper` to achieve the same with any custom catalog.*

## Why optional dependencies?

In case you need a Module to be loaded before another but do not want a mandatory dependency which would cause a fault if the wanted module is missing. An example would be a Module which can provide additional functionality if a certain hardware is connected. The module would initialize differently through DI if the appropriate interface has an implementer or not.

Without optional dependencies one would need to implement service classes which provide an event to signal everyone that the hardware is now connected so even modules initialized before know about it. This approach works but makes the code more complicated and harder to read.

> Please Note: After consideration I decided that the mentioned alternative is most of the time the better approach because otherwise the depending Module needs to know the exact Names of the targets which is not always the case. But you might know parts of their names therefore this package shall be extended with another tag like `[match]` where all modules are added as dependencies which match the given string. Doing this e.g. all modules with the string "Controller" can be added as a dependency. But this problem might be better solved with a DI Service like mentioned above.

## How it is done

The easiest way to achieve optional dependencies was to not adjust the handling of Modules itself by adding a new `OptionalDependency Property` but just post-Evaluate the Modules inside the catalog after the catalog is build and using the existing strings to tag them.

1. To make a dependency optional just add `[optional]` to your Module Name inside the Dependency. 

   1. Example Configuration Catalog:

      ```xml
       <dependency moduleName="DataProvider[optional]" />
      ```

      

   2. Example Directory Catalog

      ```csharp
      [ModuleDependency("DataProvider[optional]")]
      ```

      

2. Use one of the CustomModuleCatalogs. They will load all modules as default but then scan for the `[optional]` Tag. It will discard any dependencies which are missing and keep the ones existing.

## OptionalDependencyHelper

The helper will accept a list of Modules which it will scan for the [optional] tag and keep the existing dependencies while discarding the missing ones.



## Versions

The package is build with Prism 7 and .NET Framework 4.8. But there should be now problem to target prism 8 or other .NET versions. But I did not test it with any other.

## Contact

repos.blattner@vistron.de 
