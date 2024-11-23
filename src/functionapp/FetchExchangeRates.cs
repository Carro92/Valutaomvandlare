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
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", "options", Route = null)] HttpRequestMessage req,
        ILogger log)
    {
        log.LogInformation("Handling request...");

        // Hämta CORS-konfiguration från miljövariabel
        string corsOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");

        // Hantera OPTIONS-förfrågningar
        if (req.Method == HttpMethod.Options)
        {
            var optionsResponse = new HttpResponseMessage(HttpStatusCode.OK);
            optionsResponse.Headers.Add("Access-Control-Allow-Origin", corsOrigins);
            optionsResponse.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            optionsResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            return optionsResponse;
        }

        // Hantera GET-förfrågningar
        if (req.Method == HttpMethod.Get)
        {
            string apiKey = Environment.GetEnvironmentVariable("OPENEXCHANGERATES_API_KEY");
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

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(result.ToString(), System.Text.Encoding.UTF8, "application/json")
            };

            responseMessage.Headers.Add("Access-Control-Allow-Origin", corsOrigins);
            return responseMessage;
        }

        // Hantera POST-förfrågningar
        if (req.Method == HttpMethod.Post)
        {
            var requestData = await req.Content.ReadAsStringAsync();
            var data = JObject.Parse(requestData);

            double amount = double.Parse(data["amount"].ToString());
            string baseCurrency = data["baseCurrency"].ToString();
            string targetCurrency = data["targetCurrency"].ToString();

            string apiKey = Environment.GetEnvironmentVariable("OPENEXCHANGERATES_API_KEY");
            string url = $"https://openexchangerates.org/api/latest.json?app_id={apiKey}";

            var response = await client.GetStringAsync(url);
            var exchangeData = JObject.Parse(response);

            double baseRate = double.Parse(exchangeData["rates"][baseCurrency].ToString());
            double targetRate = double.Parse(exchangeData["rates"][targetCurrency].ToString());
            double convertedAmount = (amount / baseRate) * targetRate;

            var result = new JObject
            {
                ["baseCurrency"] = baseCurrency,
                ["targetCurrency"] = targetCurrency,
                ["convertedAmount"] = convertedAmount
            };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(result.ToString(), System.Text.Encoding.UTF8, "application/json")
            };

            responseMessage.Headers.Add("Access-Control-Allow-Origin", corsOrigins);
            return responseMessage;
        }

        return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
    }
}
