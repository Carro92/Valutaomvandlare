using Azure;
using Azure.Data.Tables;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class StoreExchangeRates
{
    [FunctionName("StoreExchangeRates")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
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

        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        if (string.IsNullOrEmpty(connectionString))
        {
            log.LogError("AzureWebJobsStorage is not set.");
            return new StatusCodeResult(500);
        }

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
                Rate = rate.Value.ToString()
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

public class ExchangeRateEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public string Rate { get; set; }

    // Korrekt typ för ETag enligt ITableEntity
    public ETag ETag { get; set; } = ETag.All;
}

