namespace BankApi.Services;

public class CurrencyRate
{
    public required string Currency { get; set; }

    public decimal Buy { get; set; }

    public decimal Sell { get; set; }
}