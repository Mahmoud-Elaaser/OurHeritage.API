using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OurHeritage.Core.Entities;

namespace OurHeritage.Core.Configuration
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder
                .Property(c => c.Content)
                .IsRequired();

            builder
                .HasOne(c => c.CulturalArticle)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.CulturalArticleId);


            builder
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId);


            builder.HasIndex(t => t.CulturalArticleId);
            builder.HasIndex(t => t.UserId);
        }
    }
}
