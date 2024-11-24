using Azure.Data.Tables;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;

public static class StoreExchangeRates
{
    [FunctionName("StoreExchangeRates")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("StoreExchangeRates function triggered.");

        // Hämta JSON från förfrågan
        string requestBody = await req.ReadAsStringAsync();
        if (string.IsNullOrEmpty(requestBody))
        {
            log.LogError("Request body is empty.");
            return new BadRequestObjectResult("Request body cannot be empty.");
        }

        dynamic exchangeRates;
        try
        {
            exchangeRates = JsonConvert.DeserializeObject<dynamic>(requestBody);
        }
        catch (Exception ex)
        {
            log.LogError($"Error deserializing JSON: {ex.Message}");
            return new BadRequestObjectResult("Invalid JSON format.");
        }

        if (exchangeRates == null || exchangeRates.rates == null)
        {
            log.LogError("Missing 'rates' in request body.");
            return new BadRequestObjectResult("'rates' data is missing in the request body.");
        }

        // Spara växelkurser i ExchangeRates tabellen
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var tableClient = new TableClient(connectionString, "ExchangeRates");
        await tableClient.CreateIfNotExistsAsync();

        foreach (var rate in exchangeRates.rates)
        {
            if (rate.Name == null || rate.Value == null)
            {
                log.LogError("Missing 'Name' or 'Value' in rate.");
                continue;
            }

            var entity = new ExchangeRateEntity
            {
                PartitionKey = exchangeRates["base"].ToString(),
                RowKey = rate.Name.ToString(),
                Rate = rate.Value.ToString(),
                ConvertedAmount = rate.Value.ToString()  // Assuming you're storing the rate as a placeholder for converted amount
            };

            try
            {
                await tableClient.UpsertEntityAsync(entity);
                log.LogInformation($"Saved rate for {rate.Name}: {rate.Value}");
            }
            catch (Exception ex)
            {
                log.LogError($"Error saving rate for {rate.Name}: {ex.Message}");
            }
        }

        return new OkResult();
    }
}
