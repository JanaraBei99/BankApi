using System.ComponentModel.DataAnnotations;

namespace BankApi.Models;

public class Phone
{
    public int Id { get; set; }

    [Required]
    [Phone]
    public string Number { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        "^(Mobile|Home|Work)$",
        ErrorMessage = "Phone type must be Mobile, Home or Work.")]
    public string Type { get; set; } = string.Empty;

    [Required]
    public int ClientId { get; set; }
}