using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpDirectory.Domain.Models;

namespace NpDirectory.Infrastructure.ModelConfigurations;

public class NaturalPersonConfiguration : IEntityTypeConfiguration<NaturalPerson>
{
    public void Configure(EntityTypeBuilder<NaturalPerson> builder)
    {
        builder.HasKey(np => np.Id);
        builder.HasIndex(np => np.PersonalNumber).IsUnique();
        builder.HasOne(np => np.City).WithMany().HasForeignKey(np => np.CityId);
    }
}