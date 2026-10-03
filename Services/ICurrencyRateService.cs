namespace BankApi.Services;

public interface ICurrencyRateService
{
    Task<CurrencyRate> GetRateAsync(string currency);
}