using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Models;
using WebAPI.Requests;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration, UserContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateuserRequest request, CancellationToken ct)
    {
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.password);
        var user = new User(request.Name, passwordHash);
        await _dbContext.Users.AddAsync(user, ct);
        await _dbContext.SaveChangesAsync(ct);

        return Ok(user);
    }
    [HttpPost("delete/({id})"),Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var customer = await _dbContext.Users.FindAsync(id);

        if (customer != null)
        {
            _dbContext.Users.Remove(customer);
            await _dbContext.SaveChangesAsync(ct);
            return Ok();
        }

        return NotFound();
    }

    
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserDto request, CancellationToken ct)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == request.Username, ct);

        if (user == null)
        {
            return BadRequest("User not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return BadRequest("Wrong password");
        }

        string token = CreateToken(user);
        return Ok(token);
    }

    private string CreateToken(User user)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JWT:Token").Value!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddSeconds(30),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
       Console.WriteLine(jwt);
        return jwt;
    }
}
