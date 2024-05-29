using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Регистрация DbContextOptions в контейнере служб
builder.Services.AddDbContext<UserContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddScoped<UserContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var app = builder.Build();

using var scope = app.Services.CreateScope(); 
await using var dbContext = scope.ServiceProvider.GetRequiredService<UserContext>();
await dbContext.Database.EnsureCreatedAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();