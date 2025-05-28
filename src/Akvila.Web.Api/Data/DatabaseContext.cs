using Akvila.Web.Api.Domains.Settings;
using Akvila.Web.Api.Domains.User;
using Microsoft.EntityFrameworkCore;

namespace Akvila.Web.Api.Data;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options) {
    public DbSet<User> Users { get; set; }
    public DbSet<Settings> Settings { get; set; }
}