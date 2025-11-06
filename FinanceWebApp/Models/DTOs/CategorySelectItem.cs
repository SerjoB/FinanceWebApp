using FinanceWebApp.Models.Enums;

namespace FinanceWebApp.Models.DTOs;

public class CategorySelectItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
}