using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OurHeritage.Core.Entities;

namespace OurHeritage.Core.Configuration
{
    public class MessageReadConfiguration : IEntityTypeConfiguration<MessageRead>
    {
        public void Configure(EntityTypeBuilder<MessageRead> builder)
        {
            //builder
            //    .HasKey(mr => new { mr.MessageId, mr.UserId });


            builder
                .HasOne(mr => mr.User)
                .WithMany()
                .HasForeignKey(mr => mr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(mr => mr.Message)
                .WithMany(m => m.ReadByUsers)
                .HasForeignKey(mr => mr.MessageId);
        }
    }
}
