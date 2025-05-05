using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OurHeritage.Core.Entities;

namespace OurHeritage.Core.Configuration
{
    public class ConversationUserConfiguration : IEntityTypeConfiguration<ConversationUser>
    {
        public void Configure(EntityTypeBuilder<ConversationUser> builder)
        {
            builder
                .HasKey(cu => new { cu.UserId, cu.ConversationId });

            builder
                .HasOne(cu => cu.User)
                .WithMany()
                .HasForeignKey(cu => cu.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            builder
                .HasOne(cu => cu.Conversation)
                .WithMany(c => c.Participants)
                .HasForeignKey(cu => cu.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasIndex(t => t.ConversationId);
            builder.HasIndex(t => t.UserId);
        }
    }
}
