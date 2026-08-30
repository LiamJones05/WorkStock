using Microsoft.EntityFrameworkCore;
using Workstock.Api.Models;

namespace Workstock.Api.Data;

public sealed class WorkstockDbContext(DbContextOptions<WorkstockDbContext> options) : DbContext(options)
{
    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<JobStatus> JobStatuses => Set<JobStatus>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobAssignment> JobAssignments => Set<JobAssignment>();
    public DbSet<JobItem> JobItems => Set<JobItem>();
    public DbSet<JobNote> JobNotes => Set<JobNote>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (var type in modelBuilder.Model.GetEntityTypes())
        foreach (var property in type.GetProperties().Where(p => p.ClrType == typeof(decimal)))
        {
            property.SetPrecision(12);
            property.SetScale(2);
        }

        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<UserSession>().HasIndex(x => x.TokenHash).IsUnique();
        modelBuilder.Entity<Customer>().HasIndex(x => new { x.OrganisationId, x.Name });
        modelBuilder.Entity<Site>().HasIndex(x => new { x.OrganisationId, x.CustomerId });
        modelBuilder.Entity<JobStatus>().HasIndex(x => new { x.OrganisationId, x.Name }).IsUnique();
        modelBuilder.Entity<Job>().HasIndex(x => new { x.OrganisationId, x.JobNumber }).IsUnique();
        modelBuilder.Entity<Job>().HasIndex(x => new { x.OrganisationId, x.ScheduledStart });
        modelBuilder.Entity<Job>().HasIndex(x => new { x.OrganisationId, x.CustomerId });
        modelBuilder.Entity<JobAssignment>().HasIndex(x => new { x.JobId, x.UserId }).IsUnique();
        modelBuilder.Entity<Activity>().HasIndex(x => new { x.OrganisationId, x.EntityType, x.EntityId, x.CreatedAt });
        modelBuilder.Entity<Document>().HasIndex(x => new { x.OrganisationId, x.OwnerType, x.OwnerId });

        modelBuilder.Entity<Organisation>().Property(x => x.Name).HasMaxLength(160);
        modelBuilder.Entity<User>().Property(x => x.Email).HasMaxLength(320);
        modelBuilder.Entity<User>().Property(x => x.DisplayName).HasMaxLength(120);
        modelBuilder.Entity<Customer>().Property(x => x.Name).HasMaxLength(160);
        modelBuilder.Entity<Site>().Property(x => x.Name).HasMaxLength(160);
        modelBuilder.Entity<Job>().Property(x => x.JobNumber).HasMaxLength(32);
        modelBuilder.Entity<Job>().Property(x => x.Title).HasMaxLength(200);
        modelBuilder.Entity<JobItem>().Property(x => x.Name).HasMaxLength(200);

        modelBuilder.Entity<Customer>().HasOne<Organisation>().WithMany().HasForeignKey(x => x.OrganisationId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Site>().HasOne<Organisation>().WithMany().HasForeignKey(x => x.OrganisationId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<JobStatus>().HasOne<Organisation>().WithMany().HasForeignKey(x => x.OrganisationId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Job>().HasOne<Organisation>().WithMany().HasForeignKey(x => x.OrganisationId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Job>().HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Job>().HasOne(x => x.Site).WithMany().HasForeignKey(x => x.SiteId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Job>().HasOne(x => x.Status).WithMany().HasForeignKey(x => x.JobStatusId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<JobAssignment>().HasOne(x => x.Job).WithMany(x => x.Assignments).HasForeignKey(x => x.JobId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<JobAssignment>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<JobItem>().HasOne<Job>().WithMany(x => x.Items).HasForeignKey(x => x.JobId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<JobNote>().HasOne<Job>().WithMany().HasForeignKey(x => x.JobId).OnDelete(DeleteBehavior.Cascade);
    }
}
