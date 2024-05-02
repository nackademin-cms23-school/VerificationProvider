using System.ComponentModel.DataAnnotations;

namespace VerificationProvider.Data.Entities;

public class VerificationRequestEntity
{
    [Key]
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
    public DateTime ExpirationDate { get; set; } = DateTime.Now.AddMinutes(5);
}
