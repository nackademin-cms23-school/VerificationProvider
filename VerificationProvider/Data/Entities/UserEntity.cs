using Microsoft.AspNetCore.Identity;

namespace VerificationProvider.Data.Entities;

public class UserEntity : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public bool Verified { get; set; }
}
