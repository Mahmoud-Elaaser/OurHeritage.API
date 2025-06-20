using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OurHeritage.Core.Entities;

namespace OurHeritage.Core.Configuration
{
    public class RepostConfiguration : IEntityTypeConfiguration<Repost>
    {
        public void Configure(EntityTypeBuilder<Repost> builder)
        {
            builder
                .HasOne(r => r.CulturalArticle)
                .WithMany()
                .HasForeignKey(r => r.CulturalArticleId)
                .OnDelete(DeleteBehavior.ClientCascade);
        }
    }
}
