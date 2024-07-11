namespace BackendAPI.Data;

using BackendAPI.Models.Tables;
using Microsoft.EntityFrameworkCore;

public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options)
        : base(options)
    {

    }

    public DbSet<CaseModel>? CaseModels { get; set; }
    public DbSet<CaseFollowModel>? CaseFollowModel { get; set; }


    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     base.OnConfiguring(optionsBuilder);
    // }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        
        // Configure CaseTable
        modelBuilder.Entity<CaseModel>(entity =>
        {
            entity.ToTable("CaseTable");

            entity.HasKey(e => e.DataId);
            entity.HasIndex(e => e.CaseID).IsUnique();

            entity.Property(e => e.CaseSubject)
                .IsRequired()
                .HasMaxLength(180);

            entity.Property(e => e.CaseID)    
                .IsRequired()
                .HasMaxLength(80);

            entity.Property(e => e.CaseSev)
                .IsRequired()
                .HasMaxLength(8);

            entity.Property(e => e.CaseStatus)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.CurrentCaseOwner)
                .IsRequired()
                .HasMaxLength(22);

            entity.Property(e => e.FollowedTime)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            entity.Property(e => e.Remark)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Resolution)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.IsArchive)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.IsClosed)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.LastSyncedTime)
                .IsRequired(false);

            entity.Property(e => e.CurrentSyncedTime)
                .IsRequired(false);
        });

        modelBuilder.Entity<CaseFollowModel>(entity =>
        {
            entity.ToTable("CaseFollowTable");

            entity.HasKey(e => e.DataId);

            entity.Property(e => e.CaseSubject)
                .IsRequired()
                .HasMaxLength(180);

            entity.Property(e => e.CaseID)
                .IsRequired()
                .HasMaxLength(80);

            entity.Property(e => e.CaseSev)
                .IsRequired()
                .HasMaxLength(8);

            entity.Property(e => e.CaseStatus)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.CurrentCaseOwner)
                .IsRequired()
                .HasMaxLength(22);

            entity.Property(e => e.FollowedTime)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            entity.Property(e => e.Remark)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.WhoFollowed)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Resolution)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.IsArchive)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.IsClosed)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.LastSyncedTime)
                .IsRequired(false);

            entity.Property(e => e.CurrentSyncedTime)
                .IsRequired(false);
            entity.Ignore(e => e.IsSynced);
        });
    }
}