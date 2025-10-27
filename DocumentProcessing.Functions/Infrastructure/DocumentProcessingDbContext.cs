using Microsoft.EntityFrameworkCore;
using DocumentProcessing.Functions.Models.Domain;

namespace DocumentProcessing.Functions.Infrastructure;

public class DocumentProcessingDbContext : DbContext
{
    public DocumentProcessingDbContext(DbContextOptions<DocumentProcessingDbContext> options) : base(options)
    {
    }

    public DbSet<Submission> Submissions { get; set; }
    public DbSet<Communication> Communications { get; set; }
    public DbSet<SubmissionStatusEntry> SubmissionStatusEntries { get; set; }
    public DbSet<CommunicationStatusEntry> CommunicationStatusEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Submission entity
        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BlobUrl).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.XmlContent).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            
            // Configure ValidationErrors as JSON
            entity.Property(e => e.ValidationErrors)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<ValidationError>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<ValidationError>()
                );

            // Ignore domain events in persistence
            entity.Ignore(e => e.DomainEvents);

            // Configure relationships
            entity.HasMany(e => e.Communications)
                .WithOne(c => c.Submission)
                .HasForeignKey(c => c.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Communication entity
        modelBuilder.Entity<Communication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentContent).HasColumnType("nvarchar(max)");
            entity.Property(e => e.DocumentType).HasMaxLength(100);
            entity.Property(e => e.Recipient).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Subject).HasMaxLength(1000);
            entity.Property(e => e.Status).HasConversion<string>();
            
            // Configure ValidationErrors as JSON
            entity.Property(e => e.ValidationErrors)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<ValidationError>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<ValidationError>()
                );

            // Ignore domain events in persistence
            entity.Ignore(e => e.DomainEvents);
        });

        // Configure SubmissionStatusEntry entity
        modelBuilder.Entity<SubmissionStatusEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.StatusDescription).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);

            entity.HasOne(e => e.Submission)
                .WithMany()
                .HasForeignKey(e => e.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure CommunicationStatusEntry entity
        modelBuilder.Entity<CommunicationStatusEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.StatusDescription).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);

            entity.HasOne(e => e.Communication)
                .WithMany()
                .HasForeignKey(e => e.CommunicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Submission)
                .WithMany()
                .HasForeignKey(e => e.SubmissionId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}