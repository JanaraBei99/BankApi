using System.ComponentModel.DataAnnotations;

namespace BankApi.Models;

public class Card
{
    public int Id { get; set; }

    [Required]
    public string CardNumber { get; set; } = string.Empty;

    public DateTime ExpirationDate { get; set; }

    [Required]
    [RegularExpression(
        "^(Debit|Credit)$",
        ErrorMessage = "Card type must be Debit or Credit.")]
    public string Type { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        "^(Active|Blocked)$",
        ErrorMessage = "Card status must be Active or Blocked.")]
    public string Status { get; set; } = "Active";

    [Required]
    public int AccountId { get; set; }
}