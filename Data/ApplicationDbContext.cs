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
        public DbSet<SaleOrder> SaleOrders { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Addon> Addons { get; set; }
        public DbSet<JwtToken> JwtTokens { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketCategory> TicketCategories { get; set; }
        public DbSet<TicketLog> TicketLogs { get; set; }
        public DbSet<TicketLogAttachment> TicketLogAttachments { get; set; }
        public DbSet<Tax> Taxes { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<SaleOrderService> SaleOrderServices { get; set; }
        public DbSet<SaleOrderAddon> SaleOrderAddons { get; set; }
        public DbSet<Category_service_addons> CategoryServiceAddons { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteService> QuoteServices { get; set; }
        public DbSet<QuoteAddon> QuoteAddons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("User");
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
            });

            // Configure JwtToken entity
            modelBuilder.Entity<JwtToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.Expiration).HasColumnType("timestamp with time zone");

                // Foreign key relationship
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Index
                entity.HasIndex(e => e.Token);
                entity.HasIndex(e => e.UserId);
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

			// Configure Service entity
			modelBuilder.Entity<Service>(entity =>
			{
				entity.HasKey(e => e.Id);
				
				entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
				entity.Property(e => e.Description).HasMaxLength(1000);
				entity.Property(e => e.Price).HasColumnType("decimal(15,2)").IsRequired();
				entity.Property(e => e.Category).HasMaxLength(50);
				entity.Property(e => e.IsActive).HasDefaultValue(true);
				entity.Property(e => e.Notes).HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");

				// Foreign Key relationship with Tax
				entity.HasOne(e => e.Tax)
					  .WithMany()
					  .HasForeignKey(e => e.TaxId)
					  .OnDelete(DeleteBehavior.Restrict);

				// Foreign Key relationship with Category_service_addons
				entity.HasOne(e => e.CategoryServiceAddons)
					  .WithMany(c => c.Services)
					  .HasForeignKey(e => e.CategoryId)
					  .OnDelete(DeleteBehavior.SetNull);

				// Indexes
				entity.HasIndex(e => e.Name);
				entity.HasIndex(e => e.Category);
				entity.HasIndex(e => e.IsActive);
				entity.HasIndex(e => e.TaxId);
				entity.HasIndex(e => e.CategoryId);
			});

			// Configure Addon entity
			modelBuilder.Entity<Addon>(entity =>
			{
				entity.HasKey(e => e.Id);
				
				entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
				entity.Property(e => e.Description).HasMaxLength(1000);
				entity.Property(e => e.Price).HasColumnType("decimal(15,2)").IsRequired();
				entity.Property(e => e.Type).HasMaxLength(50);
				entity.Property(e => e.IsActive).HasDefaultValue(true);
				entity.Property(e => e.Notes).HasMaxLength(2000);
				entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
				entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");

				// Foreign Key relationship with Tax
				entity.HasOne(e => e.Tax)
					  .WithMany()
					  .HasForeignKey(e => e.TaxId)
					  .OnDelete(DeleteBehavior.Restrict);

				// Foreign Key relationship with Category_service_addons
				entity.HasOne(e => e.CategoryServiceAddons)
					  .WithMany(c => c.Addons)
					  .HasForeignKey(e => e.CategoryId)
					  .OnDelete(DeleteBehavior.SetNull);

				// Indexes
				entity.HasIndex(e => e.Name);
				entity.HasIndex(e => e.Type);
				entity.HasIndex(e => e.IsActive);
				entity.HasIndex(e => e.TaxId);
				entity.HasIndex(e => e.CategoryId);
			});

			// Configure SaleOrder entity
			modelBuilder.Entity<SaleOrder>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Value).HasColumnType("decimal(15,2)").IsRequired();
                entity.Property(e => e.Probability).HasDefaultValue(0);
                entity.Property(e => e.Notes).HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");

                // Foreign Key relationships
                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(e => e.Service)
					  .WithMany()
					  .HasForeignKey(e => e.ServiceId)
					  .OnDelete(DeleteBehavior.SetNull);

				entity.HasOne(e => e.Addon)
					  .WithMany()
					  .HasForeignKey(e => e.AddonId)
					  .OnDelete(DeleteBehavior.SetNull);

                // Indexes
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.ServiceId);
                entity.HasIndex(e => e.AddonId);
                entity.HasIndex(e => e.Value);
                entity.HasIndex(e => e.Probability);
            });

            // Configure Tax entity
            modelBuilder.Entity<Tax>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Rate).HasColumnType("decimal(5,2)").IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
            });

            // Configure Contract entity
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Draft");
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.SubTotal).HasColumnType("decimal(15,2)").IsRequired();
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(15,2)").IsRequired();
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(15,2)").IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(2000);
                entity.Property(e => e.Expiration).HasColumnType("timestamp with time zone");
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");

                // Foreign Key relationships
                entity.HasOne(e => e.SaleOrder)
                      .WithMany()
                      .HasForeignKey(e => e.SaleOrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.SaleOrderId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Expiration);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configure TicketCategory entity
            modelBuilder.Entity<TicketCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
                
                // Indexes
                entity.HasIndex(e => e.Name);
            });

            // Configure Ticket entity
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.UrgencyLevel).HasDefaultValue(1);
                entity.Property(e => e.ClosedAt).HasColumnType("timestamp with time zone");
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");

                // Foreign Key relationships
                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AssignedTo)
                      .WithMany()
                      .HasForeignKey(e => e.AssignedToId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.CreatedBy)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedById)
                      .OnDelete(DeleteBehavior.SetNull);

                // Indexes
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.AssignedToId);
                entity.HasIndex(e => e.CreatedById);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.UrgencyLevel);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configure TicketLog entity
            modelBuilder.Entity<TicketLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");

                // Foreign Key relationships
                entity.HasOne(e => e.Ticket)
                      .WithMany()
                      .HasForeignKey(e => e.TicketId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.TicketId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configure TicketLogAttachment entity
            modelBuilder.Entity<TicketLogAttachment>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FileType).HasMaxLength(100);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Foreign Key relationship
                entity.HasOne(e => e.TicketLog)
                      .WithMany(tl => tl.Attachments)
                      .HasForeignKey(e => e.TicketLogId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.TicketLogId);
                entity.HasIndex(e => e.Category);
            });

            // Configure SaleOrderService entity (junction table)
            modelBuilder.Entity<SaleOrderService>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(15,2)").IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");

                // Foreign Key relationships
                entity.HasOne(e => e.SaleOrder)
                      .WithMany(s => s.SaleOrderServices)
                      .HasForeignKey(e => e.SaleOrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Service)
                      .WithMany()
                      .HasForeignKey(e => e.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.SaleOrderId);
                entity.HasIndex(e => e.ServiceId);
                entity.HasIndex(e => new { e.SaleOrderId, e.ServiceId }).IsUnique();
            });

            // Configure SaleOrderAddon entity (junction table)
            modelBuilder.Entity<SaleOrderAddon>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(15,2)").IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");

                // Foreign Key relationships
                entity.HasOne(e => e.SaleOrder)
                      .WithMany(s => s.SaleOrderAddons)
                      .HasForeignKey(e => e.SaleOrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Addon)
                      .WithMany()
                      .HasForeignKey(e => e.AddonId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.SaleOrderId);
                entity.HasIndex(e => e.AddonId);
                entity.HasIndex(e => new { e.SaleOrderId, e.AddonId }).IsUnique();
            });

            // Configure Category_service_addons entity
            modelBuilder.Entity<Category_service_addons>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");

                // Indexes
                entity.HasIndex(e => e.Name);
            });

            // Configure Quote entity
            modelBuilder.Entity<Quote>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.CustomServiceJson).HasMaxLength(2000).HasColumnName("CustomService");
                entity.Property(e => e.FilePath).HasMaxLength(1000);
                entity.Property(e => e.Amount).HasColumnType("decimal(15,2)").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");

                // Foreign Key relationships
                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Service)
                      .WithMany()
                      .HasForeignKey(e => e.ServiceId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Addon)
                      .WithMany()
                      .HasForeignKey(e => e.AddonId)
                      .OnDelete(DeleteBehavior.SetNull);

                // ? THÊM: Foreign key relationship v?i User (CreatedByUser)
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);

                // ? THÊM: Foreign key relationship v?i Category_service_addons
                entity.HasOne(e => e.CategoryServiceAddon)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryServiceAddonId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Indexes
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.ServiceId);
                entity.HasIndex(e => e.AddonId);
                entity.HasIndex(e => e.CreatedByUserId); // ? THÊM index
                entity.HasIndex(e => e.CategoryServiceAddonId); // ? THÊM index
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configure QuoteService entity (junction table)
            modelBuilder.Entity<QuoteService>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");

                // Foreign Key relationships
                entity.HasOne(e => e.Quote)
                      .WithMany(q => q.QuoteServices)
                      .HasForeignKey(e => e.QuoteId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Service)
                      .WithMany()
                      .HasForeignKey(e => e.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.QuoteId);
                entity.HasIndex(e => e.ServiceId);
            });

            // Configure QuoteAddon entity (junction table)
            modelBuilder.Entity<QuoteAddon>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");

                // Foreign Key relationships
                entity.HasOne(e => e.Quote)
                      .WithMany(q => q.QuoteAddons)
                      .HasForeignKey(e => e.QuoteId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Addon)
                      .WithMany()
                      .HasForeignKey(e => e.AddonId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.QuoteId);
                entity.HasIndex(e => e.AddonId);
            });
        }
    }
}
