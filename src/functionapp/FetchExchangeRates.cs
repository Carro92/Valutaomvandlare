using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public static class FetchExchangeRates
{
    private static readonly HttpClient HttpClient = new HttpClient();

    [FunctionName("FetchExchangeRates")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Fetching exchange rates...");

        string apiKey = Environment.GetEnvironmentVariable("OPENEXCHANGERATES_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            return new BadRequestObjectResult("API key is not configured.");
        }

        string apiUrl = $"https://openexchangerates.org/api/latest.json?app_id={apiKey}";

        try
        {
            var response = await HttpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(responseBody);

            // Lista över de populära valutorna som ska visas
            var popularCurrencies = new[] { "EUR", "GBP", "SEK", "USD", "AUD", "JPY", "CAD", "CHF", "NOK", "THB" };

            // Filtrera ut de specifika valutorna
            var rates = data["rates"]
                .ToObject<Dictionary<string, decimal>>()
                .Where(rate => popularCurrencies.Contains(rate.Key)) // Filtrera endast de populära valutorna
                .ToDictionary(rate => rate.Key, rate => rate.Value);

            return new OkObjectResult(new { rates });
        }
        catch (Exception ex)
        {
            log.LogError($"Error fetching exchange rates: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
