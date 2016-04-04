using Microsoft.Data.Entity;
using ShikashiAPI.Model;

namespace ShikashiAPI
{
    public class PersistenceContext : DbContext
    {
        public DbSet<APIKey> APIKey { get; set; }
        public DbSet<InviteKey> InviteKey { get; set; }
        public DbSet<UploadedContent> UploadedContent { get; set; }
        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<APIKey>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<InviteKey>()
                .HasKey(p => p.Key);
            
            modelBuilder.Entity<UploadedContent>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<UploadedContent>()
                .Property(p => p.Uploaded).ValueGeneratedOnAdd();

            modelBuilder.Entity<User>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<User>()
                .HasAlternateKey(p => p.Email);

            modelBuilder.Entity<User>()
                .Property(p => p.Id).ValueGeneratedOnAdd();
        }
    }
}
