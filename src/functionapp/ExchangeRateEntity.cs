using Azure;
using Azure.Data.Tables;
using System;

public class ExchangeRateEntity : ITableEntity
{
    public string PartitionKey { get; set; }  // Basvaluta, t.ex. "USD"
    public string RowKey { get; set; }  // Målvaluta, t.ex. "SEK"
    public DateTimeOffset? Timestamp { get; set; }  // När entiteten senast uppdaterades
    public string Rate { get; set; }  // Växelkursen mellan basvaluta och målvaluta
    public string ConvertedAmount { get; set; }  // Det konverterade beloppet
    public ETag ETag { get; set; } = ETag.All;  // Används för att hantera entitetens uppdateringar
}
