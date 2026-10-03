using BankApi.Data;
using BankApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhonesController : ControllerBase
{
    private readonly AppDbContext _context;

    public PhonesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Phone>>> GetPhones()
    {
        return await _context.Phones.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Phone>> GetPhone(int id)
    {
        var phone = await _context.Phones.FindAsync(id);

        if (phone == null)
        {
            return NotFound();
        }

        return phone;
    }

    [HttpPost]
    public async Task<ActionResult<Phone>> CreatePhone(Phone phone)
    {
        var client = await _context.Clients.FindAsync(phone.ClientId);

        if (client == null)
        {
            return BadRequest("Client does not exist.");
        }

        _context.Phones.Add(phone);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetPhone),
            new { id = phone.Id },
            phone);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePhone(
        int id,
        Phone phone)
    {
        if (id != phone.Id)
        {
            return BadRequest();
        }

        var existingPhone = await _context.Phones.FindAsync(id);

        if (existingPhone == null)
        {
            return NotFound();
        }

        var client = await _context.Clients.FindAsync(phone.ClientId);

        if (client == null)
        {
            return BadRequest("Client does not exist.");
        }

        existingPhone.Number = phone.Number;
        existingPhone.Type = phone.Type;
        existingPhone.ClientId = phone.ClientId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePhone(int id)
    {
        var phone = await _context.Phones.FindAsync(id);

        if (phone == null)
        {
            return NotFound();
        }

        _context.Phones.Remove(phone);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}