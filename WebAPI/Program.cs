using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using WebAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UserContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddSwaggerGen(o =>
{
    o.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standart Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer'",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    o.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWT:Token").Value)),
        ValidateIssuer = false,
        ValidateAudience = false
    });
builder.Services.AddControllers();
builder.Services.AddScoped<UserContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5174");
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });
});

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

    app.UseAuthentication();

    app.UseAuthorization();
    app.UseCors();
    app.MapControllers();

    app.Run();