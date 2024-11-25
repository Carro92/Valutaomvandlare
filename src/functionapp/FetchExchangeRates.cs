using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

public static class FetchExchangeRates
{
    private static readonly HttpClient client = new HttpClient();

    [FunctionName("FetchExchangeRates")]
    public static async Task<HttpResponseMessage> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "options", Route = null)] HttpRequestMessage req,
        ILogger log)
    {
        log.LogInformation("Handling request...");

        // Hantera OPTIONS-förfrågningar för CORS
        if (req.Method == HttpMethod.Options)
        {
            log.LogInformation("Handling OPTIONS request...");
            var optionsResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            optionsResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            optionsResponse.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            optionsResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type, x-functions-key");
            return optionsResponse;
        }

        // Hantera GET-förfrågningar
        if (req.Method == HttpMethod.Get)
        {
            log.LogInformation("Handling GET request...");
            string apiKey = Environment.GetEnvironmentVariable("OPENEXCHANGERATES_API_KEY");
            log.LogInformation($"Using API Key: {apiKey}");

            string url = $"https://openexchangerates.org/api/latest.json?app_id={apiKey}";

            var response = await client.GetStringAsync(url);
            var data = JObject.Parse(response);

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

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(result.ToString(), System.Text.Encoding.UTF8, "application/json")
            };

            responseMessage.Headers.Add("Access-Control-Allow-Origin", "*");
            return responseMessage;
        }

        // Hantera POST-förfrågningar
        if (req.Method == HttpMethod.Post)
        {
            log.LogInformation("Handling POST request...");
            string requestBody = await req.Content.ReadAsStringAsync();
            log.LogInformation($"Request Body: {requestBody}");

            dynamic data = JObject.Parse(requestBody);
            string baseCurrency = data?.baseCurrency;
            string targetCurrency = data?.targetCurrency;
            decimal amount = data?.amount;

            if (string.IsNullOrEmpty(baseCurrency) || string.IsNullOrEmpty(targetCurrency) || amount <= 0)
            {
                log.LogError("Invalid input data.");
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Invalid input data.")
                };
            }

            // Här kan du lägga till logik för valutakonvertering
            var rates = new Dictionary<string, decimal>
            {
                {"USD", 1m}, {"EUR", 0.95m}, {"SEK", 10.5m}, {"GBP", 0.8m}, {"THB", 34.5m}
            };

            if (!rates.ContainsKey(baseCurrency) || !rates.ContainsKey(targetCurrency))
            {
                log.LogError("Unsupported currency.");
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Unsupported currency.")
                };
            }

            decimal baseRate = rates[baseCurrency];
            decimal targetRate = rates[targetCurrency];
            decimal convertedAmount = amount * (targetRate / baseRate);

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(new JObject
                {
                    ["baseCurrency"] = baseCurrency,
                    ["targetCurrency"] = targetCurrency,
                    ["amount"] = amount,
                    ["convertedAmount"] = convertedAmount
                }.ToString(), System.Text.Encoding.UTF8, "application/json")
            };

            responseMessage.Headers.Add("Access-Control-Allow-Origin", "*");
            return responseMessage;
        }

        return new HttpResponseMessage(System.Net.HttpStatusCode.MethodNotAllowed);
    }
}
