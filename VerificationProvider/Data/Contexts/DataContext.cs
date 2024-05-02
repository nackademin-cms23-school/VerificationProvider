using Microsoft.EntityFrameworkCore;
using VerificationProvider.Data.Entities;

namespace VerificationProvider.Data.Contexts;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<VerificationRequestEntity> VerificationRequests { get; set; }
}
