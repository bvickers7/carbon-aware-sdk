using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using GSF.CarbonIntensity.Managers;
using GSF.CarbonIntensity.Parameters;

namespace myfunc
{
    public class DemoFunc
    {
        private readonly IEmissionsManager _manager;
        public DemoFunc(IEmissionsManager manager)
        {
            this._manager = manager;
        }

        [FunctionName("DemoFunc")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            
            const string startDate = "2022-03-01T15:30:00Z";
            const string endDate = "2022-03-01T18:30:00Z";
            const string location = "eastus";
            var builder = new EmissionsParametersBuilder();
            var param = builder.AddLocations(new string[] { location } )
                .AddStartTime(DateTimeOffset.Parse(startDate))
                .AddEndTime(DateTimeOffset.Parse(endDate))
                .Build();

            var result = await _manager.GetRatingAsync(param);
            var responseMessage = $"For location {location} Starting at: {startDate} Ending at: {endDate} the Carbon Emissions Rating is: {result}.";

            return new OkObjectResult(responseMessage);
        }
    }
}
