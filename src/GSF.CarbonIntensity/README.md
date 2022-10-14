# Library design considerations

This document helps to have the concept on how GSF CarbonAware SDK lib was conceived. 

## Namespace

Given the fact this is going to be a library exposing functionality to consumers, it is [recommended](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces) to use the following namespace naming schema: `<Company>.(<Product>|<Technology>)[.<Feature>][.<Subnamespace>]`. For GSF CarbonAware SDK this the following schema:

- **Company**: ***GSF***
- **Product**: ***CarbonIntensity***
- **Feature**: ***Model***, ***Handlers***, ...

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
| GSF.CarbonIntensity.Handlers |
| GSF.CarbonIntensity.Model |
| GSF.CarbonIntensity.Parameters |


## Features

### Model

There are two main classes that represents the data fetched from the datasources (i.e `Static Json`, [WattTime](https://www.watttime.org) and [ElectricityMap](https://www.electricitymaps.com)):

- `EmissionsData`
- `EmissionsForecast`

These would be provided by the **Handlers** as return types driven by a **Parameters** class that the consumer will instantiate using a **builder helper** class.

### Handlers

There would be two handlers for each of the data types returned:

- `EmissionsHandler`
- `ForecastHandler`

Each would be responsible of interacting on its own domain. For instance `EmissionsHandler` would have a method `GetEmissionsDataAsync` to pull `EmissionsData` instances from a configured data source.
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
// An application Consumer construct should inject a GSF handler like the following example
public class ConsumerApp(IEmissionsHandler handler, ILogger<ConsumerApp> logger)
{
    ....
    this._handler = handler;
    this._logger = logger;
    ....
}

public Task<double> GetRating()
{
    ....
    return await this._handler.GetEmissionsDataAsync(...).Rating;
}
```

## References

https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/
