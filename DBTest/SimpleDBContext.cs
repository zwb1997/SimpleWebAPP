namespace DBTest;

using DBTest.Models;
using Microsoft.EntityFrameworkCore;

public class SimpleDBContext : DbContext
{

    //public DbSet<Inventory> Inventories { get; set; }

    public DbSet<FollowedCaseTable> CaseFollowTable { get; set; }

    public SimpleDBContext(DbContextOptions<SimpleDBContext> options)
        : base(options)
    {
    }

    #region required
    /// <summary>
    /// redundant for now
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<Inventory>(entity =>
        //{
        //    entity.ToTable("Inventory");  // Ensure the table name matches
        //    entity.HasKey(e => e.Id);     // Explicitly set the primary key
        //    entity.Property(e => e.Id).HasColumnName("id"); // Ensure the column name matches
        //    entity.Property(e => e.Name).HasColumnName("name");
        //    entity.Property(e => e.Quantity).HasColumnName("quantity");
        //});


        base.OnModelCreating(modelBuilder);

        // Configure CaseFollowTable
        modelBuilder.Entity<FollowedCaseTable>(entity =>
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
        }
    #endregion

}
