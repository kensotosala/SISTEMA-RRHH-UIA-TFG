using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataAccessLayer.Data
{
    public class SistemaRhContextFactory : IDesignTimeDbContextFactory<SistemaRhContext>
    {
        public SistemaRhContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SistemaRhContext>();

            var connectionString = "Server=localhost;Database=sistema_rh;Uid=root;Pwd=1234";

            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            );

            return new SistemaRhContext(optionsBuilder.Options);
        }
    }
}