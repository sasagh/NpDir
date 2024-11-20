using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpDirectory.Domain.Models;

namespace NpDirectory.Infrastructure.ModelConfigurations;

public class RelationConfiguration : IEntityTypeConfiguration<Relation>
{
    public void Configure(EntityTypeBuilder<Relation> builder)
    {
        builder.HasKey(relation => relation.Id);

        builder
            .HasOne(relation => relation.NaturalPerson)
            .WithMany()
            .HasForeignKey(relation => relation.NaturalPersonId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(relation => relation.RelatedPerson)
            .WithMany()
            .HasForeignKey(relation => relation.RelatedPersonId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}