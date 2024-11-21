using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

public static class FetchExchangeRates
{
    private static readonly HttpClient client = new HttpClient();

    [FunctionName("FetchExchangeRates")]
    public static async Task<HttpResponseMessage> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "options", Route = null)] HttpRequestMessage req,
        ILogger log)
    {
        log.LogInformation("Fetching exchange rates...");

        // Hämta CORS-konfiguration från miljövariabel
        string corsOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
        log.LogInformation("CORS allowed origins: " + corsOrigins);

        // Hantera OPTIONS-förfrågningar
        if (req.Method == HttpMethod.Options)
        {
            var optionsResponse = new HttpResponseMessage(HttpStatusCode.OK);
            
            // Tillåter flera origin-domäner
            optionsResponse.Headers.Add("Access-Control-Allow-Origin", corsOrigins); // Tillåter angivna domäner
            optionsResponse.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS"); // Tillåt metoder
            optionsResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type"); // Tillåt headers

            return optionsResponse;
        }

        // Hämta data från API
        string apiKey = Environment.GetEnvironmentVariable("OPENEXCHANGERATES_API_KEY");
        string url = $"https://openexchangerates.org/api/latest.json?app_id={apiKey}";

        var response = await client.GetStringAsync(url);
        var data = JObject.Parse(response);

        // Filtrera valutorna
        var popularCurrencies = new[] { "EUR", "GBP", "SEK", "USD", "AUD", "JPY", "CAD", "CHF", "NOK", "THB" };
        var filteredRates = new JObject();

        foreach (var currency in popularCurrencies)
        {
            if (data["rates"][currency] != null)
            {
                filteredRates[currency] = data["rates"][currency];
            }
        }

        var result = new JObject
        {
            ["base"] = data["base"],
            ["rates"] = filteredRates
        };

        // Returnera data med CORS-header
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(result.ToString(), System.Text.Encoding.UTF8, "application/json")
        };

        // Lägg till CORS-headers för att tillåta förfrågningar från angivna origin-domäner
        responseMessage.Headers.Add("Access-Control-Allow-Origin", corsOrigins);
        responseMessage.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS"); 
        responseMessage.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

        return responseMessage;
    }
}
