using System.ComponentModel.DataAnnotations;

namespace BankApi.Models;

public class Account
{
    public int Id { get; set; }

    [Required]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        "^(KGS|USD|EUR|RUB|KZT)$",
        ErrorMessage = "Currency must be KGS, USD, EUR, RUB or KZT.")]
    public string Currency { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Balance { get; set; }

    [Required]
    public int ClientId { get; set; }
}