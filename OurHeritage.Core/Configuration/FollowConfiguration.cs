using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OurHeritage.Core.Entities;

namespace OurHeritage.Core.Configuration
{
    public class FollowConfiguration : IEntityTypeConfiguration<Follow>
    {
        public void Configure(EntityTypeBuilder<Follow> builder)
        {
            builder.HasKey(f => new { f.FollowerId, f.FollowingId });

            builder
                .HasOne(u => u.Follower)
                .WithMany()
                .HasForeignKey(u => u.FollowerId);

            builder
                .HasOne(u => u.Following)
                .WithMany()
                .HasForeignKey(f => f.FollowingId);
        }
    }
}
