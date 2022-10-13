# How to use the scripts

In order to add CarbonAware packages to a new application, it is necessarely to build them first from the source repository. After cloning, run the following scripts

## Create Packages

```sh
cd <repo>/ms-internal/lib-integration/scripts
./create_packages.sh /mypackages
```
`create_packages.sh` script will generate all the nuget package files and they will be located under `/mypackages`


## Add Packages to a Project

Assuming there is a C# project already, using `add_packages.sh` script allows to have all the packages needed in order to integrate with the library.

```
./add_packages.sh /myproject/myapp.csprj /mypackages
```

There could be errors adding packages that are not available yet (i.e  `Microsoft.Extensions.Configuration`), so those need to be added using `dotnet add package Microsoft.Extensions.Configuration` command for instance.
