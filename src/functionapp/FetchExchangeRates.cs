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
    [FunctionName("FetchExchangeRates")]
    public static async Task<HttpResponseMessage> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
        ILogger log)
    {
        log.LogInformation("Fetching exchange rates...");

        string apiKey = Environment.GetEnvironmentVariable("OPENEXCHANGERATES_API_KEY");
        string url = $"https://openexchangerates.org/api/latest.json?app_id={apiKey}";

        HttpClient client = new HttpClient();
        var response = await client.GetAsync(url);
        var data = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(data);

        var filteredRates = new JObject();
        var popularCurrencies = new[] { "EUR", "GBP", "SEK", "USD", "AUD", "JPY", "CAD", "CHF", "NOK", "THB" };

        foreach (var currency in popularCurrencies)
        {
            if (json["rates"][currency] != null)
            {
                filteredRates[currency] = json["rates"][currency];
            }
        }

        var result = new JObject
        {
            ["base"] = json["base"],
            ["rates"] = filteredRates
        };

        // Skapa ett HTTP-svar med CORS-headers
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(result.ToString(), System.Text.Encoding.UTF8, "application/json")
        };

        // Lägg till CORS-stöd
        responseMessage.Headers.Add("Access-Control-Allow-Origin", "*"); // Tillåter alla origin
        responseMessage.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS");
        responseMessage.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

        return responseMessage;
    }
}
