using BankApi.Data;
using BankApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CardsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Card>>> GetCards()
    {
        return await _context.Cards.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Card>> GetCard(int id)
    {
        var card = await _context.Cards.FindAsync(id);

        if (card == null)
        {
            return NotFound();
        }

        return card;
    }

    [HttpPost]
    public async Task<ActionResult<Card>> CreateCard(Card card)
    {
        var account = await _context.Accounts
            .FindAsync(card.AccountId);

        if (account == null)
        {
            return BadRequest("Account does not exist.");
        }

        if (card.ExpirationDate.Date <= DateTime.Today)
        {
            return BadRequest(
                "Card expiration date must be in the future.");
        }

        card.Status = "Active";

        try
        {
            _context.Cards.Add(card);

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("Card number already exists.");
        }

        return CreatedAtAction(
            nameof(GetCard),
            new { id = card.Id },
            card);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCard(
        int id,
        Card card)
    {
        if (id != card.Id)
        {
            return BadRequest();
        }

        var existingCard = await _context.Cards.FindAsync(id);

        if (existingCard == null)
        {
            return NotFound();
        }

        if (existingCard.Status == "Blocked")
        {
            return BadRequest(
                "Blocked card cannot be modified. Unblock the card first.");
        }

        var currentAccount = await _context.Accounts
            .FindAsync(existingCard.AccountId);

        if (currentAccount == null)
        {
            return BadRequest("Current account does not exist.");
        }

        var newAccount = await _context.Accounts
            .FindAsync(card.AccountId);

        if (newAccount == null)
        {
            return BadRequest("Account does not exist.");
        }

        if (currentAccount.ClientId != newAccount.ClientId)
        {
            return BadRequest(
                "Card can only be assigned to an account of the same client.");
        }

        if (card.ExpirationDate.Date <= DateTime.Today)
        {
            return BadRequest(
                "Card expiration date must be in the future.");
        }

        existingCard.CardNumber = card.CardNumber;
        existingCard.ExpirationDate = card.ExpirationDate;
        existingCard.Type = card.Type;
        existingCard.AccountId = card.AccountId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}/block")]
    public async Task<IActionResult> BlockCard(int id)
    {
        var card = await _context.Cards.FindAsync(id);

        if (card == null)
        {
            return NotFound();
        }

        if (card.Status == "Blocked")
        {
            return BadRequest("Card is already blocked.");
        }

        card.Status = "Blocked";

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}/unblock")]
    public async Task<IActionResult> UnblockCard(int id)
    {
        var card = await _context.Cards.FindAsync(id);

        if (card == null)
        {
            return NotFound();
        }

        if (card.Status == "Active")
        {
            return BadRequest("Card is already active.");
        }

        card.Status = "Active";

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCard(int id)
    {
        var card = await _context.Cards.FindAsync(id);

        if (card == null)
        {
            return NotFound();
        }

        _context.Cards.Remove(card);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}