using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Shared.Infrastructure.DbContext.SchoolDbContext.Migrations
{
    [DbContext(typeof(SchoolDbContext))]
    partial class SchoolDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            // Add entity configuration here when you add models
            // Example:
            // modelBuilder.Entity("Shared.Infrastructure.DbContext.SchoolDbContext.Entities.Student", b =>
            // {
            //     b.Property<int>("Id")
            //         .ValueGeneratedOnAdd()
            //         .HasColumnType("int");
            // });
#pragma warning restore 612, 618
        }
    }
}
