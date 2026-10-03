using BankApi.Data;
using Microsoft.EntityFrameworkCore;
using BankApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=bank.db"));

builder.Services.AddHttpClient<ICurrencyRateService, KicbCurrencyRateService>();
builder.Services.AddScoped<TransferService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();