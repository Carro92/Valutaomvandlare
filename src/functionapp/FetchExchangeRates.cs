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
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
        ILogger log)
    {
        string apiKey = Environment.GetEnvironmentVariable("OPENEXCHANGERATES_API_KEY");
        string url = "https://openexchangerates.org/api/latest.json?app_id=" + apiKey;

        var response = await client.GetStringAsync(url);
        var data = JObject.Parse(response);

        var rates = data["rates"];
        
        // Lista över de 10 mest populära valutorna som du vill visa
        var popularCurrencies = new[] { "EUR", "GBP", "SEK", "USD", "AUD", "JPY", "CAD", "CHF", "NOK", "THB" };

        // Skapa en ny lista med de populära valutorna
        var filteredRates = new JObject();
        foreach (var currency in popularCurrencies)
        {
            if (rates[currency] != null)
            {
                filteredRates[currency] = rates[currency];
            }
        }

        // Svara med de filtrerade valutorna
        var result = new JObject
        {
            ["base"] = data["base"],
            ["rates"] = filteredRates
        };

        // Returnera de topp 10 valutorna som JSON
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(result.ToString(), System.Text.Encoding.UTF8, "application/json")
        };
    }
}
