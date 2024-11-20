using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpDirectory.Domain.Models;

namespace NpDirectory.Infrastructure.ModelConfigurations;

public class PhoneNumberConfiguration : IEntityTypeConfiguration<PhoneNumber>
{
    public void Configure(EntityTypeBuilder<PhoneNumber> builder)
    {
        builder.HasKey(pn => pn.Number);
        builder.HasOne(pn => pn.NaturalPerson).WithMany(pn => pn.PhoneNumbers).HasForeignKey(pn => pn.NaturalPersonId);
    }
}