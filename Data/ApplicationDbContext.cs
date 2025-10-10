using Microsoft.EntityFrameworkCore;
using erp_backend.Models;

namespace erp_backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Deal> Deals { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

			// Configure Customer entity
			modelBuilder.Entity<Customer>(entity =>
			{
				entity.HasKey(e => e.Id);
				
				// Common fields
				entity.Property(e => e.CustomerType).IsRequired().HasMaxLength(20);
				entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("potential");
				entity.Property(e => e.Notes).HasMaxLength(2000);
				entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
				entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
				entity.Property(e => e.IsActive).HasDefaultValue(true);

				// Individual fields  
				entity.Property(e => e.Name).HasMaxLength(100);
				entity.Property(e => e.Address).HasMaxLength(500);
				entity.Property(e => e.BirthDate).HasColumnType("timestamp with time zone");
				entity.Property(e => e.IdNumber).HasMaxLength(50);
				entity.Property(e => e.PhoneNumber).HasMaxLength(20);
				entity.Property(e => e.Email).HasMaxLength(150);
				entity.Property(e => e.Domain).HasMaxLength(100);

				// Company fields
				entity.Property(e => e.CompanyName).HasMaxLength(200);
				entity.Property(e => e.CompanyAddress).HasMaxLength(500);
				entity.Property(e => e.EstablishedDate).HasColumnType("timestamp with time zone");
				entity.Property(e => e.TaxCode).HasMaxLength(50);
				entity.Property(e => e.CompanyDomain).HasMaxLength(100);

				// Representative info
				entity.Property(e => e.RepresentativeName).HasMaxLength(100);
				entity.Property(e => e.RepresentativePosition).HasMaxLength(100);
				entity.Property(e => e.RepresentativeIdNumber).HasMaxLength(50);
				entity.Property(e => e.RepresentativePhone).HasMaxLength(20);
				entity.Property(e => e.RepresentativeEmail).HasMaxLength(150);

				// Technical contact
				entity.Property(e => e.TechContactName).HasMaxLength(100);
				entity.Property(e => e.TechContactPhone).HasMaxLength(20);
				entity.Property(e => e.TechContactEmail).HasMaxLength(150);

				// Indexes
				entity.HasIndex(e => e.Email);
				entity.HasIndex(e => e.CustomerType);
				entity.HasIndex(e => e.Status);
				entity.HasIndex(e => e.PhoneNumber);
			});

			// Configure Deal entity
			modelBuilder.Entity<Deal>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Value).HasColumnType("decimal(15,2)").IsRequired();
                entity.Property(e => e.Priority).HasConversion<string>().HasDefaultValue(DealPriority.Medium);
                entity.Property(e => e.Stage).HasConversion<string>().HasDefaultValue(DealStage.Lead);
                entity.Property(e => e.Probability).HasDefaultValue(0);
                entity.Property(e => e.Notes).HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");

                // Foreign Key relationships
                entity.HasOne(d => d.Customer)
                      .WithMany()
                      .HasForeignKey(d => d.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.AssignedUser)
                      .WithMany()
                      .HasForeignKey(d => d.AssignedTo)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(d => d.CreatedBy)
                      .OnDelete(DeleteBehavior.SetNull);

                // Indexes
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.Stage);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.AssignedTo);
            });
        }
    }
}