using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace minkod
{
    public static class StoreConversionHistory
    {
        [FunctionName("StoreConversionHistory")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Processing conversion history...");

            // Läs JSON från begäran
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Validera ingångsdata
            string baseCurrency = data?.baseCurrency;
            string targetCurrency = data?.targetCurrency;
            double rate = data?.rate ?? 0;
            DateTime conversionDate = DateTime.UtcNow;

            if (string.IsNullOrEmpty(baseCurrency) || string.IsNullOrEmpty(targetCurrency) || rate <= 0)
            {
                return new BadRequestObjectResult("Invalid input. Please provide baseCurrency, targetCurrency, and rate.");
            }

            // Hämta anslutningssträngen från miljövariabler
            string connectionString = Environment.GetEnvironmentVariable("TableStorageConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                log.LogError("TableStorageConnectionString is not set.");
                return new StatusCodeResult(500);
            }

            // Anslut till Azure Table Storage
            var tableClient = new TableClient(connectionString, "ConversionHistory");

            // Skapa tabellen om den inte finns
            try
            {
                await tableClient.CreateIfNotExistsAsync();
                log.LogInformation("Table 'ConversionHistory' is ready.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error creating or accessing the table: {ex.Message}");
                return new StatusCodeResult(500);
            }

            // Skapa en ny entitet
            var conversionEntity = new ConversionEntity
            {
                PartitionKey = baseCurrency,
                RowKey = $"{targetCurrency}-{Guid.NewGuid()}", // Kombinerar TargetCurrency med GUID, // Unikt ID för raden
                TargetCurrency = targetCurrency,
                Rate = rate,
                ConversionDate = conversionDate,
                Timestamp = DateTimeOffset.UtcNow
            };

            // Lägg till entiteten i tabellen
            try
            {
                await tableClient.AddEntityAsync(conversionEntity);
                log.LogInformation("Conversion history saved successfully.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error saving conversion history: {ex.Message}");
                return new StatusCodeResult(500);
            }

            return new OkObjectResult($"Conversion from {baseCurrency} to {targetCurrency} saved.");
        }

        public class ConversionEntity : ITableEntity
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public string TargetCurrency { get; set; }
            public double Rate { get; set; }
            public DateTime ConversionDate { get; set; }
            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; } = ETag.All;
        }
    }
}
