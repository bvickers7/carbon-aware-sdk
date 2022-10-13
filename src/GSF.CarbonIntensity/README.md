# Library design consideration

This document helps to clarify some of the decisions taken with regards GSF CarbonAware SDK library.

## Namespace

Given the fact this is going to be a library exposing functionality to consumers, it is [recommended](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces) to use the following naming schema: `<Company>.(<Product>|<Technology>)[.<Feature>][.<Subnamespace>]`. For GSF CarbonAware SDK, the following schema is proposed:

- ***Company***: **GSF**
- ***Product***: **CarbonIntensity**
- ***Feature***: **Model**, **Managers**, ...

An example of a namespace would be: `namespace GSF.CarbonIntensity.Model` and a class (record, interface, ...) that belongs to that namespace would be:

```c#
namespace GSF.CarbonIntensity.Model;

public record EmissionsData
{
    ....
}
```

The following namespaces are considered:

| namespace   |
| ----------- |
| GSF.CarbonIntensity.Exceptions |
| GSF.CarbonIntensity.Configuration |
| GSF.CarbonIntensity.Managers |
| GSF.CarbonIntensity.Model |
| GSF.CarbonIntensity.Parameters |


## Features

### Model

There are two main classes that represents the data fetched from the datasources (i.e `Static Json`, `WattTime` and `ElectricityMap`):

- `EmissionsData`
- `EmissionsForecast`

These would be provided by the **Managers** as return types driven by a **Parameters** class that the consumer will instantiate using a **builder helper** class.

### Managers

There would be two managers for each of the data types returned:

- `EmissionsManager`
- `ForecastManager`

Each would be responsible of interacting on its own domain. For instance `EmissionsManager` would have a method `GetEmissionsDataAsync` to pull `EmissionsData` instances from a configured data source.
(Note: The current core implementation is using `async/await` paradigm, which would be the default for GSF SDK library too).

### Parameters

// TODO!!

### Error Handling

`CarbonIntensityException` class will be used to report errors to the consumer. It would follow the `Exception` class approach, where messages and details will be provided as part of error reporting.

### Dependency Injection

Using C# practices on how to register services, the library would be available through `Microsoft.Extensions.DependencyInjection` extension. For instance a consumer would be able to call:

```c#
// Using DI Services to register GSF SDK library
services.AddCarbonIntensityServices(configuration);
```
```c#
// An application Consumer construct should inject a GSF manager like the following example
public class ConsumerApp(IEmissionsManager manager, ILogger<ConsumerApp> logger)
{
    ....
    this._manager = manager;
    this._logger = logger;
    ....
}

public Task<double> GetRating()
{
    ....
    return await this._manager.GetEmissionsDataAsync(...).Rating;
}
```

## References

https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/
