using System.Globalization;
using System.Text.RegularExpressions;

namespace BankApi.Services;

public class KicbCurrencyRateService : ICurrencyRateService
{
    private readonly HttpClient _httpClient;

    private const string KicbCurrencyUrl =
        "https://kicb.net/en/currency/?tab=non-cash";

    public KicbCurrencyRateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CurrencyRate> GetRateAsync(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException(
                "Currency is required.");
        }

        currency = currency.Trim().ToUpperInvariant();

        if (currency == "KGS")
        {
            return new CurrencyRate
            {
                Currency = "KGS",
                Buy = 1,
                Sell = 1
            };
        }

        string html;

        try
        {
            html = await _httpClient.GetStringAsync(KicbCurrencyUrl);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(
                "Currency rate service is temporarily unavailable.");
        }

        var pattern =
            $@"<p[^>]*>\s*{Regex.Escape(currency)}\s*" +
            @"<small>.*?</small>.*?</p>\s*</td>\s*" +
            @"<td[^>]*>\s*<p[^>]*>(?<buy>[\d.,]+)</p>\s*" +
            @"</td>\s*" +
            @"<td[^>]*>\s*<p[^>]*>(?<sell>[\d.,]+)</p>";

        var match = Regex.Match(
            html,
            pattern,
            RegexOptions.IgnoreCase |
            RegexOptions.Singleline);

        if (!match.Success)
        {
            throw new KeyNotFoundException(
                $"Currency {currency} was not found.");
        }

        var buyText = match.Groups["buy"].Value
            .Replace(",", ".");

        var sellText = match.Groups["sell"].Value
            .Replace(",", ".");

        if (!decimal.TryParse(
                buyText,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var buy))
        {
            throw new InvalidOperationException(
                $"Invalid buy rate for currency {currency}.");
        }

        if (!decimal.TryParse(
                sellText,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var sell))
        {
            throw new InvalidOperationException(
                $"Invalid sell rate for currency {currency}.");
        }

        return new CurrencyRate
        {
            Currency = currency,
            Buy = buy,
            Sell = sell
        };
    }
}