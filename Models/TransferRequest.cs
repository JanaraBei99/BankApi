using System.ComponentModel.DataAnnotations;

namespace BankApi.Models;

public class TransferRequest
{
    [Range(1, int.MaxValue)]
    public int FromAccountId { get; set; }

    [Range(1, int.MaxValue)]
    public int ToAccountId { get; set; }

    [Range(typeof(decimal), "0.01", "1000000000")]
    public decimal Amount { get; set; }
}