using System.Linq.Expressions;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Domain.Common;
using AssignmentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentHub.Infrastructure.Persistence;

/// <summary>
/// Also implements IUnitOfWork directly — a single SaveChangesAsync call is EF Core's own
/// transaction boundary, so a separate wrapper class would add nothing but indirection.
/// </summary>
public class AppDbContext : DbContext, IUnitOfWork
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, IDateTimeProvider dateTimeProvider)
        : base(options)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<ClassSubject> ClassSubjects => Set<ClassSubject>();
    public DbSet<StudentClass> StudentClasses => Set<StudentClass>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<GradeAuditLog> GradeAuditLogs => Set<GradeAuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Setting> Settings => Set<Setting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Postgres-native optimistic concurrency token for Assignment (see
        // Configurations/AssignmentConfiguration.cs) — gated to the Npgsql provider only, since
        // "xmin" is a real Postgres system column with no SQLite equivalent, and the integration
        // test host runs this same model against a SQLite in-memory database.
        if (Database.IsNpgsql())
        {
            modelBuilder.Entity<Assignment>()
                .Property<uint>("xmin")
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsRowVersion();
        }

        // Every entity implementing ISoftDelete is transparently filtered wherever it's queried
        // (Query()/GetByIdAsync in every repository) — Application-layer code never has to
        // remember to add "!x.IsDeleted" itself. Explicit .IgnoreQueryFilters() opts back in
        // if a future admin "show deleted" view is ever needed.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, [modelBuilder]);
            }
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDelete
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(BuildNotDeletedExpression<TEntity>());
    }

    private static Expression<Func<TEntity, bool>> BuildNotDeletedExpression<TEntity>()
        where TEntity : class, ISoftDelete
    {
        return entity => !entity.IsDeleted;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = _dateTimeProvider.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = utcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
