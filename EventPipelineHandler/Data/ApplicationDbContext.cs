using Microsoft.EntityFrameworkCore;

namespace EventPipelineHandler.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<EventAction> EventActions { get; set; }
        public DbSet<EventActionChain> EventActionChains { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }

    public class Customer : IEntityWithIdAndMetaData
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class IEntityWithIdAndMetaData
    {
        public Guid Id { get; set; }

        public required DateTime CreatedAt { get; set; }
        public required string CreatedBy { get; set; }
        public required DateTime UpdatedAt { get; set; }
        public required string UpdatedBy { get; set; }
    }
}
