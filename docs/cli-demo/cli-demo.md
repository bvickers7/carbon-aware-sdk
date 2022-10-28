# CLI Demo
## Build the CLI
Use `dotnet build` to create the appropriate executable for your operating system. Navigate to the output directory to use your newly created executable. If output directory is not specified, the default path is ./bin/<configuration>/<framework>/.

>`GIF of running dotnet build and navigating to the output folder to display -h`

## Emissions Commands
Use `emissions` command to retrieve emissions data. Appropriate flag can be used to get specific data like best and average. 

### Emissions by location and time
Required parameter `location` must be provided, whereas `start-time` and `end-time` are optional 
`.\caw.exe emissions --location eastus --start-time 2022-09-01T12:45:11+00:00 --end-time 2022-09-02T12:45:11+00:00`

### Emissions for multiple locations
`.\caw.exe emissions --location eastus -location westus`

### Best emissions for the given location and time period
Use the flag `--best` to get the optimal emissions for a given location and time
`.\caw.exe emissions --location eastus --start-time 2022-09-01 --end-time 2022-09-03 --best`


### Average emissions for the given location and time period
Use the flag `--average` to get the optimal emissions for a given location and time
`.\caw.exe emissions --location eastus --start-time 2022-09-01 --end-time 2022-09-03 --average`

## Forecast Command
Use `emissions-forecasts` command to get forecast data. The option `requested-at` can be used to toggle between histrorical forecast and current forecast. If `requested-at` is not specified, the current forecast is retrieved and if specified, the historical forecast is retrieved.
