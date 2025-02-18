using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OurHeritage.Core.Entities;

namespace OurHeritage.Core.Configuration
{
    public class HandiCraftConfiguration : IEntityTypeConfiguration<HandiCraft>
    {
        public void Configure(EntityTypeBuilder<HandiCraft> builder)
        {
            builder
                .HasMany(h => h.Favorite)
                .WithOne(f => f.HandiCraft)
                .HasForeignKey(f => f.HandiCraftId);

            builder
                .HasOne(h => h.User)
                .WithMany(u => u.HandiCrafts)
                .HasForeignKey(h => h.UserId);

            builder
                .HasMany(h => h.Orders)
                .WithMany(o => o.HandiCrafts)
                .UsingEntity<Dictionary<string, object>>(
                    "OrderHandiCraft",
                    j => j.HasOne<Order>().WithMany().HasForeignKey("OrderId"),
                    j => j.HasOne<HandiCraft>().WithMany().HasForeignKey("HandiCraftId"),
                    j => j.HasKey("OrderId", "HandiCraftId")
                );
        }
    }
}
