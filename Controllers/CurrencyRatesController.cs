using BankApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers;

[ApiController]
[Route("api/currency-rates")]
public class CurrencyRatesController : ControllerBase
{
    private readonly ICurrencyRateService _currencyRateService;

    public CurrencyRatesController(
        ICurrencyRateService currencyRateService)
    {
        _currencyRateService = currencyRateService;
    }

    [HttpGet("{currency}")]
    public async Task<IActionResult> GetRate(string currency)
    {
        try
        {
            var rate = await _currencyRateService.GetRateAsync(currency);

            return Ok(rate);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}