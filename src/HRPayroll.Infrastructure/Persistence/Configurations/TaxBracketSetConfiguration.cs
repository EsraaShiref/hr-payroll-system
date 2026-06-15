using HRPayroll.Domain.Entities;
using HRPayroll.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class TaxBracketSetConfiguration : IEntityTypeConfiguration<TaxBracketSet>
{
    public void Configure(EntityTypeBuilder<TaxBracketSet> builder)
    {
        builder.ToTable("TaxBracketSets");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.EffectiveFrom)
            .IsRequired();

        builder.Property(t => t.EffectiveTo);

        builder.OwnsMany(t => t.Brackets, b =>
        {
            b.ToTable("TaxBrackets");
            b.WithOwner().HasForeignKey("TaxBracketSetId");

            b.Property<Guid>("Id");
            b.HasKey("Id");

            b.Property(br => br.FromAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property(br => br.ToAmount)
                .HasColumnType("decimal(18,2)");

            b.Property(br => br.Rate)
                .IsRequired()
                .HasColumnType("decimal(5,2)");
        });

        builder.HasIndex(t => new { t.EffectiveFrom, t.EffectiveTo })
            .HasDatabaseName("IX_TaxBracketSets_EffectiveRange");

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
