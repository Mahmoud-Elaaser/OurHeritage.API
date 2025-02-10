using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OurHeritage.Core.Entities;

namespace OurHeritage.Core.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {

            builder
                .Property(u => u.FirstName)
                .HasMaxLength(60);

            builder
                .Property(u => u.DateJoined)
                .IsRequired();
        }
    }
}
