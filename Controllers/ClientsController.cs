using BankApi.Data;
using BankApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClientsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<Client>>> GetClients(
        string? search,
        string? sortBy,
        int page = 1,
        int pageSize = 10)
    {
        if (page < 1)
        {
            return BadRequest(
                "Page must be greater than or equal to 1.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(
                "PageSize must be between 1 and 100.");
        }

        var query = _context.Clients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(client =>
                EF.Functions.Like(
                    client.FirstName,
                    $"%{search}%") ||

                EF.Functions.Like(
                    client.LastName,
                    $"%{search}%") ||

                EF.Functions.Like(
                    client.Patronymic,
                    $"%{search}%"));
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            switch (sortBy.ToLowerInvariant())
            {
                case "firstname":
                    query = query.OrderBy(client => client.FirstName);
                    break;

                case "lastname":
                    query = query.OrderBy(client => client.LastName);
                    break;

                case "birthdate":
                    query = query.OrderBy(client => client.BirthDate);
                    break;

                default:
                    return BadRequest(
                        "SortBy must be firstName, lastName or birthDate.");
            }
        }

        var totalCount = await query.CountAsync();

        var clients = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize);

        var result = new PagedResult<Client>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Items = clients
        };

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> GetClient(int id)
    {
        var client = await _context.Clients.FindAsync(id);

        if (client == null)
        {
            return NotFound();
        }

        return client;
    }

    [HttpPost]
    public async Task<ActionResult<Client>> CreateClient(Client client)
    {
        _context.Clients.Add(client);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetClient),
            new { id = client.Id },
            client);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClient(
        int id,
        Client client)
    {
        if (id != client.Id)
        {
            return BadRequest();
        }

        var existingClient = await _context.Clients.FindAsync(id);

        if (existingClient == null)
        {
            return NotFound();
        }

        existingClient.FirstName = client.FirstName;
        existingClient.LastName = client.LastName;
        existingClient.Patronymic = client.Patronymic;
        existingClient.Email = client.Email;
        existingClient.BirthDate = client.BirthDate;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var client = await _context.Clients.FindAsync(id);

        if (client == null)
        {
            return NotFound();
        }

        var hasPhones = await _context.Phones
            .AnyAsync(phone => phone.ClientId == id);

        var hasAccounts = await _context.Accounts
            .AnyAsync(account => account.ClientId == id);

        if (hasPhones || hasAccounts)
        {
            return BadRequest(
                "Client cannot be deleted while it has related phones or accounts.");
        }

        _context.Clients.Remove(client);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}