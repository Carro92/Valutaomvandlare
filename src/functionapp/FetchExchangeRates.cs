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
        log.LogInformation("Fetching exchange rates...");

        try
        {
            // Hämta API-nyckeln från miljövariabel
            string apiKey = Environment.GetEnvironmentVariable("OPENEXCHANGERATES_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                log.LogError("OPENEXCHANGERATES_API_KEY is not set.");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("API key is missing.")
                };
            }

            // Skapa URL för API-anropet
            string url = $"https://openexchangerates.org/api/latest.json?app_id={apiKey}";

            // Anropa Open Exchange Rates API
            var response = await client.GetStringAsync(url);
            var data = JObject.Parse(response);

            var rates = data["rates"];

            // Lista över de populäraste valutorna att filtrera
            var popularCurrencies = new[] { "EUR", "GBP", "SEK", "USD", "AUD", "JPY", "CAD", "CHF", "NOK", "THB" };

            // Filtrera kurser för de populära valutorna
            var filteredRates = new JObject();
            foreach (var currency in popularCurrencies)
            {
                if (rates[currency] != null)
                {
                    filteredRates[currency] = rates[currency];
                }
            }

            // Skapa ett svar med de filtrerade kurserna
            var result = new JObject
            {
                ["base"] = data["base"],
                ["rates"] = filteredRates
            };

            // Skapa HTTP-svaret
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(result.ToString(), System.Text.Encoding.UTF8, "application/json")
            };

            // Lägg till CORS-headers
            responseMessage.Headers.Add("Access-Control-Allow-Origin", "*");
            responseMessage.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            responseMessage.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            log.LogInformation("Exchange rates fetched successfully.");
            return responseMessage;
        }
        catch (Exception ex)
        {
            log.LogError($"Error fetching exchange rates: {ex.Message}");
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent($"Error fetching exchange rates: {ex.Message}")
            };
        }
    }
}
