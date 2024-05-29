namespace WebAPI.Models;

public class User
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string PasswordHash { get; set; }

    public User(string userName, string passwordHash)
    {
        UserName = userName;
        PasswordHash = passwordHash;
    }
}