using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FetchExchangeRates.function
{
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FetchExchangeRates.function
{
    public static class FetchExchangeRates
    {
        private static readonly HttpClient client = new HttpClient();

        [FunctionName("FetchExchangeRates")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Fetching exchange rates from Open Exchange Rates API.");

            // Hämta API-nyckeln från miljövariabeln
            string apiKey = Environment.GetEnvironmentVariable("OPENEXCHANGERATES_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                log.LogError("API key is missing.");
                return new BadRequestObjectResult("API key is missing.");
            }

            string url = $"https://openexchangerates.org/api/latest.json?app_id={apiKey}";

            // Anropa API:et
            HttpResponseMessage response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return new BadRequestObjectResult("Failed to fetch exchange rates.");
            }

            // Hämta och bearbeta svaret från API:et
            string json = await response.Content.ReadAsStringAsync();
            var exchangeRates = JsonConvert.DeserializeObject(json);

            return new OkObjectResult(exchangeRates);
        }
    }
}
}
