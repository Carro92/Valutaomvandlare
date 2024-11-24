using Azure.Data.Tables;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Azure.WebJobs.Extensions.Http;

public static class FetchExchangeRates
{
    private static readonly HttpClient client = new HttpClient();

    [FunctionName("FetchExchangeRates")]
    public static async Task<HttpResponseMessage> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", "options", Route = null)] HttpRequestMessage req,
        ILogger log)
    {
        log.LogInformation("Handling request...");

        // Hantera OPTIONS-förfrågningar för CORS
        if (req.Method == HttpMethod.Options)
        {
            var optionsResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            optionsResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            optionsResponse.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            optionsResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            return optionsResponse;
        }

        // Hantera GET-förfrågningar för att hämta växelkurser
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

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(result.ToString(), System.Text.Encoding.UTF8, "application/json")
            };

            responseMessage.Headers.Add("Access-Control-Allow-Origin", "*");
            return responseMessage;
        }

        // Hantera POST-förfrågningar för att lagra omvandlad valuta i Azure Table Storage
        if (req.Method == HttpMethod.Post)
        {
            var requestData = await req.Content.ReadAsStringAsync();
            var data = JObject.Parse(requestData);

            string baseCurrency = data["baseCurrency"].ToString();
            string targetCurrency = data["targetCurrency"].ToString();
            double amount = double.Parse(data["amount"].ToString());

            string apiKey = Environment.GetEnvironmentVariable("OPENEXCHANGERATES_API_KEY");
            string url = $"https://openexchangerates.org/api/latest.json?app_id={apiKey}";

            var response = await client.GetStringAsync(url);
            var exchangeData = JObject.Parse(response);

            double baseRate = double.Parse(exchangeData["rates"][baseCurrency].ToString());
            double targetRate = double.Parse(exchangeData["rates"][targetCurrency].ToString());
            double convertedAmount = (amount / baseRate) * targetRate;

            // Skicka till Table Storage
            await StoreExchangeRatesData(baseCurrency, targetCurrency, targetRate, convertedAmount, log);

            var result = new JObject
            {
                ["baseCurrency"] = baseCurrency,
                ["targetCurrency"] = targetCurrency,
                ["convertedAmount"] = convertedAmount
            };

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(result.ToString(), System.Text.Encoding.UTF8, "application/json")
            };

            responseMessage.Headers.Add("Access-Control-Allow-Origin", "*");
            return responseMessage;
        }

        return new HttpResponseMessage(System.Net.HttpStatusCode.MethodNotAllowed);
    }

    // Funktion för att lagra omvandlade växelkurser i Table Storage
    private static async Task StoreExchangeRatesData(string baseCurrency, string targetCurrency, double targetRate, double convertedAmount, ILogger log)
    {
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var tableClient = new TableClient(connectionString, "ExchangeRates");
        await tableClient.CreateIfNotExistsAsync();

        var entity = new ExchangeRateEntity
        {
            PartitionKey = baseCurrency,
            RowKey = targetCurrency,
            Rate = targetRate.ToString(),
            ConvertedAmount = convertedAmount.ToString()
        };

        await tableClient.UpsertEntityAsync(entity);
        log.LogInformation($"Stored exchange rate for {baseCurrency} to {targetCurrency}: {targetRate}, Converted amount: {convertedAmount}");
    }
}
