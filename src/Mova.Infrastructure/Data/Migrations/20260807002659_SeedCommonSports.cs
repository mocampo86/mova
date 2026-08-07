using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mova.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedCommonSports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"INSERT INTO ""Sports"" (""Id"", ""Name"", ""Status"") VALUES
                    (gen_random_uuid(), 'Fútbol', 'Active'),
                    (gen_random_uuid(), 'Básquetbol', 'Active'),
                    (gen_random_uuid(), 'Tenis', 'Active'),
                    (gen_random_uuid(), 'Pádel', 'Active'),
                    (gen_random_uuid(), 'Vóley', 'Active'),
                    (gen_random_uuid(), 'Futsal', 'Active'),
                    (gen_random_uuid(), 'Rugby', 'Active'),
                    (gen_random_uuid(), 'Hockey', 'Active'),
                    (gen_random_uuid(), 'Balonmano', 'Active'),
                    (gen_random_uuid(), 'Natación', 'Active'),
                    (gen_random_uuid(), 'Golf', 'Active'),
                    (gen_random_uuid(), 'Squash', 'Active')
                ON CONFLICT (""Name"") DO NOTHING;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException(
                "This data seed migration cannot be safely reverted because sports may already be associated with courts.");
        }
    }
}
