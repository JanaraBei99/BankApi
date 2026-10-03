using BankApi.Data;
using BankApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AccountsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
    {
        return await _context.Accounts.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Account>> GetAccount(int id)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account == null)
        {
            return NotFound();
        }

        return account;
    }

    [HttpPost]
    public async Task<ActionResult<Account>> CreateAccount(Account account)
    {
        var client = await _context.Clients.FindAsync(account.ClientId);

        if (client == null)
        {
            return BadRequest("Client does not exist.");
        }

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetAccount),
            new { id = account.Id },
            account);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(
        int id,
        UpdateAccountRequest account)
    {
        var existingAccount = await _context.Accounts.FindAsync(id);

        if (existingAccount == null)
        {
            return NotFound();
        }

        if (existingAccount.Currency != account.Currency &&
            existingAccount.Balance != 0)
        {
            return BadRequest(
                "Currency cannot be changed while account has a balance.");
        }

        existingAccount.AccountNumber = account.AccountNumber;
        existingAccount.Currency = account.Currency;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var account = await _context.Accounts.FindAsync(id);

        if (account == null)
        {
            return NotFound();
        }

        if (account.Balance != 0)
        {
            return BadRequest(
                "Account can be closed only when balance is zero.");
        }

        var cards = await _context.Cards
            .Where(card => card.AccountId == id)
            .ToListAsync();

        var hasActiveCards = cards.Any(card => card.Status == "Active");

        if (hasActiveCards)
        {
            return BadRequest(
                "Account cannot be closed while it has active cards.");
        }

        _context.Cards.RemoveRange(cards);
        _context.Accounts.Remove(account);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}