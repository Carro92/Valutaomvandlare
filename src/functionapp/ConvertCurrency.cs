using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class ConvertCurrency
{
    [FunctionName("ConvertCurrency")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Processing a currency conversion request...");

        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string baseCurrency = data?.baseCurrency;
            string targetCurrency = data?.targetCurrency;
            decimal amount = data?.amount;

            if (string.IsNullOrEmpty(baseCurrency) || string.IsNullOrEmpty(targetCurrency) || amount <= 0)
            {
                return new BadRequestObjectResult("Invalid input. Please provide baseCurrency, targetCurrency, and a valid amount.");
            }

            // Här kan vi hämta växelkurs från databasen eller API:n. För nu använder vi en exempelkonvertering.
            decimal conversionRate = 10.0m; // Exempelvärde
            decimal convertedAmount = amount * conversionRate;

            return new OkObjectResult(new
            {
                baseCurrency,
                targetCurrency,
                originalAmount = amount,
                convertedAmount
            });
        }
        catch (Exception ex)
        {
            log.LogError($"An error occurred: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
