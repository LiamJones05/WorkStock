using Microsoft.EntityFrameworkCore;
using Workstock.Api.Models;

namespace Workstock.Api.Data;

public class WorkstockDbContext(DbContextOptions<WorkstockDbContext> options)
    : DbContext(options)
{
    public DbSet<Organisation> Organisations => Set<Organisation>();
}