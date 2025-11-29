using Domain.Entities.IdentityModule;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Presistance.IdentityDb.IdentityConfigurations
{
    public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(rt => rt.Id);
            builder.Property(rt => rt.TokenHash).IsRequired();
            builder.HasOne<User>()
                .WithMany(u=>u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(rt=>rt.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        }
    }
}
