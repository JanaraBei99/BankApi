using BankApi.Data;
using BankApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Services;

public class TransferService
{
    private readonly AppDbContext _context;
    private readonly ICurrencyRateService _currencyRateService;

    public TransferService(
        AppDbContext context,
        ICurrencyRateService currencyRateService)
    {
        _context = context;
        _currencyRateService = currencyRateService;
    }

    public async Task TransferAsync(TransferRequest request)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException(
                "Transfer amount must be greater than zero.");
        }

        var fromAccount = await _context.Accounts
            .FindAsync(request.FromAccountId);

        var toAccount = await _context.Accounts
            .FindAsync(request.ToAccountId);

        if (fromAccount == null)
        {
            throw new KeyNotFoundException(
                "Source account not found.");
        }

        if (toAccount == null)
        {
            throw new KeyNotFoundException(
                "Destination account not found.");
        }

        if (fromAccount.Id == toAccount.Id)
        {
            throw new ArgumentException(
                "Source and destination accounts must be different.");
        }

        if (fromAccount.Balance < request.Amount)
        {
            throw new InvalidOperationException(
                "Insufficient funds.");
        }

        decimal amountToCredit;

        if (fromAccount.Currency == toAccount.Currency)
        {
            amountToCredit = request.Amount;
        }
        else
        {
            decimal amountInKgs;

            if (fromAccount.Currency == "KGS")
            {
                amountInKgs = request.Amount;
            }
            else
            {
                var fromRate = await _currencyRateService
                    .GetRateAsync(fromAccount.Currency);

                amountInKgs = request.Amount * fromRate.Buy;
            }

            if (toAccount.Currency == "KGS")
            {
                amountToCredit = amountInKgs;
            }
            else
            {
                var toRate = await _currencyRateService
                    .GetRateAsync(toAccount.Currency);

                amountToCredit = amountInKgs / toRate.Sell;
            }
        }

        amountToCredit = Math.Round(
            amountToCredit,
            2,
            MidpointRounding.AwayFromZero);

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            fromAccount.Balance -= request.Amount;
            toAccount.Balance += amountToCredit;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}