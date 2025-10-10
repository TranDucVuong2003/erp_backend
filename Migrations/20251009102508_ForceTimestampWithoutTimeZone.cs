using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class ForceTimestampWithoutTimeZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Force tất cả cột DateTime sang timestamp without time zone
            migrationBuilder.Sql(@"
                -- Customers table
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""BirthDate"" TYPE timestamp without time zone USING ""BirthDate""::timestamp without time zone;
                
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""EstablishedDate"" TYPE timestamp without time zone USING ""EstablishedDate""::timestamp without time zone;
                
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""CreatedAt"" TYPE timestamp without time zone USING ""CreatedAt""::timestamp without time zone;
                
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""UpdatedAt"" TYPE timestamp without time zone USING ""UpdatedAt""::timestamp without time zone;
                
                -- Users table (nếu có)
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Users') THEN
                        ALTER TABLE ""Users"" 
                        ALTER COLUMN ""CreatedAt"" TYPE timestamp without time zone USING ""CreatedAt""::timestamp without time zone;
                    END IF;
                END $$;
                
                -- Deals table (nếu có)
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Deals') THEN
                        ALTER TABLE ""Deals"" 
                        ALTER COLUMN ""CreatedAt"" TYPE timestamp without time zone USING ""CreatedAt""::timestamp without time zone;
                        
                        ALTER TABLE ""Deals"" 
                        ALTER COLUMN ""UpdatedAt"" TYPE timestamp without time zone USING ""UpdatedAt""::timestamp without time zone;
                        
                        IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Deals' AND column_name = 'ExpectedCloseDate') THEN
                            ALTER TABLE ""Deals"" 
                            ALTER COLUMN ""ExpectedCloseDate"" TYPE date USING ""ExpectedCloseDate""::date;
                        END IF;
                        
                        IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Deals' AND column_name = 'ActualCloseDate') THEN
                            ALTER TABLE ""Deals"" 
                            ALTER COLUMN ""ActualCloseDate"" TYPE date USING ""ActualCloseDate""::date;
                        END IF;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback về timestamp with time zone
            migrationBuilder.Sql(@"
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""BirthDate"" TYPE timestamp with time zone USING ""BirthDate""::timestamp with time zone;
                
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""EstablishedDate"" TYPE timestamp with time zone USING ""EstablishedDate""::timestamp with time zone;
                
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""CreatedAt"" TYPE timestamp with time zone USING ""CreatedAt""::timestamp with time zone;
                
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""UpdatedAt"" TYPE timestamp with time zone USING ""UpdatedAt""::timestamp with time zone;
            ");
        }
    }
}
