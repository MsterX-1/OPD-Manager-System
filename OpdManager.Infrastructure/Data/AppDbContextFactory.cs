using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OpdManager.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=MSTERX\\MSTERX;Database=OPD;User Id=sa;Password=MsterXx123;TrustServerCertificate=True;")
            .Options;
        return new AppDbContext(options);
    }
}
