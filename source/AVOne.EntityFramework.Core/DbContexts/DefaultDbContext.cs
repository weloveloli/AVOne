using Furion.DatabaseAccessor;
using Microsoft.EntityFrameworkCore;

namespace AVOne.EntityFramework.Core;

[AppDbContext("AVOne", DbProvider.Sqlite)]
public class DefaultDbContext : AppDbContext<DefaultDbContext>
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options)
    {
    }
}
