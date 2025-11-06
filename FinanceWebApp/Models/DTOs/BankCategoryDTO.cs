using System.Text.Json.Serialization;
using FinanceWebApp.Models.Enums;

namespace FinanceWebApp.Models.DTOs;

public class BankCategoryDTO
{
    [JsonPropertyName("bankCategoryName")] 
    public string BankCategoryName { get; set; } = "";

    [JsonPropertyName("transactionType")] 
    public string TransactionType { get; set; } = "";
    [JsonPropertyName("existed")] 
    public string Existed { get; set; } = "";
}