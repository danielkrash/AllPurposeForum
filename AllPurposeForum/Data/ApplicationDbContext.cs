using AllPurposeForum.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AllPurposeForum.Data
{
    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<Topic> Topics { get; set; }
        /*public DbSet<CommentStatus> CommentStatuses { get; set; }*/
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Topic>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.DeletedAt).HasDefaultValueSql("NULL");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.ApplicationUser)
                    .WithMany(e => e.Topics)
                    .HasForeignKey(e => e.ApplicationUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            builder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.DeletedAt).HasDefaultValueSql("NULL");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(e => e.Topic)
                    .WithMany(e => e.Posts)
                    .HasForeignKey(e => e.TopicId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.ApplicationUser)
                    .WithMany(e => e.Posts)
                    .HasForeignKey(e => e.ApplicationUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            builder.Entity<PostComment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.DeletedAt).HasDefaultValueSql("NULL");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Acceptence).HasDefaultValue(true);
                entity.HasOne(e => e.Post)
                    .WithMany(e => e.PostComments)
                    .HasForeignKey(e => e.Id)
                    .OnDelete(DeleteBehavior.Cascade);
                /*entity.HasOne(e => e.CommentStatus)
                    .WithMany(e => e.PostComments)
                    .HasForeignKey(e => e.CommentStatusId)
                    .OnDelete(DeleteBehavior.Restrict);*/
            });
            /*builder.Entity<CommentStatus>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.DeletedAt).HasDefaultValueSql("NULL");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });*/
            OnModelCreatingPartial(builder);
        }
        
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
