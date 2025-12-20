using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.DomainConfig;

public class ImageConfig: IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Url)
            .IsRequired();
        builder.HasIndex(i => i.PublicId)
            .IsUnique();
        builder.Property(i => i.PublicId)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(i => i.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.HasOne(i => i.User)
            .WithMany(u => u.Images)
            .HasForeignKey(i => i.UserId)
            .IsRequired(false) // we enforce it to be true in the application layer
            .OnDelete(DeleteBehavior.SetNull); // so that we can clean up later or not XD
        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}