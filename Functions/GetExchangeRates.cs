using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table; // För Table Storage

public static class GetExchangeRates
{
    // Skapa en HttpClient-instans för att göra API-anrop
    private static readonly HttpClient httpClient = new HttpClient();

    // Funktion som hämtar växelkurser
    [FunctionName("GetExchangeRates")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Fetching exchange rates...");

        string apiUrl = "https://api.exchangerate-api.com/v4/latest/USD"; // Exempel-URL för ett valutakurs-API
        HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            // Läs svaret som en sträng
            string responseBody = await response.Content.ReadAsStringAsync();

            // Deserialisera JSON-svaret till ett dynamiskt objekt
            var exchangeData = JsonConvert.DeserializeObject(responseBody);

            // Spara växelkurser till Azure Table Storage
            await SaveExchangeRatesToTableStorage(exchangeData, log);

            // Returnera växelkurserna som svar på API-anropet
            return new OkObjectResult(exchangeData);
        }
        else
        {
            log.LogError("Failed to fetch exchange rates.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    // Metod för att spara växelkurser i Table Storage
    private static async Task SaveExchangeRatesToTableStorage(dynamic exchangeData, ILogger log)
    {
        // Hämta anslutningssträngen från miljövariabler
        string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        
        // Skapa en anslutning till Table Storage
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
        CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        
        // Hämta referens till tabellen "CurrencyRates" eller skapa den om den inte finns
        CloudTable table = tableClient.GetTableReference("CurrencyRates");
        await table.CreateIfNotExistsAsync();

        // Loopa igenom varje valuta och spara den i tabellen
        foreach (var rate in exchangeData.rates)
        {
            // Skapa ett nytt CurrencyEntity-objekt som representerar raden i Table Storage
            var currencyEntity = new CurrencyEntity("ExchangeRate", rate.Key)
            {
                Rate = rate.Value
            };

            // Infoga eller uppdatera posten i Table Storage
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(currencyEntity);
            await table.ExecuteAsync(insertOrMergeOperation);
        }

        log.LogInformation("Exchange rates saved to Table Storage.");
    }

    // Klass som representerar en entitet i Table Storage
    public class CurrencyEntity : TableEntity
    {
        // Konstruktor som definierar PartitionKey och RowKey
        public CurrencyEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public CurrencyEntity() { }

        // Egenskap för växelkursen
        public double Rate { get; set; }
    }
}

internal class TableClientConfiguration
{
    public TableClientConfiguration()
    {
    }
}