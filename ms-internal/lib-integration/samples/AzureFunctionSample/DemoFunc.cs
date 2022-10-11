using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Model;

namespace myfunc
{
    public class DemoFunc
    {

        private readonly ICarbonAwareAggregator _aggregator;
        public DemoFunc(ICarbonAwareAggregator aggregator)
        {
            this._aggregator = aggregator;
        }

        [FunctionName("DemoFunc")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;
            
            var parameters = new CarbonAwareParameters()
            {
                SingleLocation = new Location { Name = "westus"},
                Start = DateTimeOffset.Parse("2022-03-01T15:30:00Z"),
                End = DateTimeOffset.Parse("2022-03-01T18:30:00Z")
            };
            var result = await _aggregator.CalculateAverageCarbonIntensityAsync(parameters);
            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {result}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
