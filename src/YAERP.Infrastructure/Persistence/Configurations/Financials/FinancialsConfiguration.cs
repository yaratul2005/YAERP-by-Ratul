using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Entities.Financials;

namespace YAERP.Infrastructure.Persistence.Configurations.Financials;

public class FixedAssetConfiguration : IEntityTypeConfiguration<FixedAsset>
{
    public void Configure(EntityTypeBuilder<FixedAsset> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.AssetTag).IsUnique();
        builder.HasIndex(x => x.Status);

        builder.Property(x => x.AcquisitionCost).HasPrecision(18, 4);
        builder.Property(x => x.SalvageValue).HasPrecision(18, 4);
        builder.Property(x => x.AccumulatedDepreciation).HasPrecision(18, 4);
        builder.Property(x => x.BookValue).HasPrecision(18, 4);
    }
}

public class DepreciationScheduleEntryConfiguration : IEntityTypeConfiguration<DepreciationScheduleEntry>
{
    public void Configure(EntityTypeBuilder<DepreciationScheduleEntry> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.FixedAssetId);
        builder.HasIndex(x => new { x.PeriodYear, x.PeriodMonth, x.IsPosted });

        builder.Property(x => x.DepreciationAmount).HasPrecision(18, 4);
        builder.Property(x => x.AccumulatedDepreciationAfter).HasPrecision(18, 4);
        builder.Property(x => x.BookValueAfter).HasPrecision(18, 4);
    }
}

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.HasKey(x => x.Code);
    }
}

public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.ForeignCurrencyCode, x.EffectiveDateUtc });
        builder.Property(x => x.RateToBase).HasPrecision(18, 6);
    }
}

public class TaxRuleConfiguration : IEntityTypeConfiguration<TaxRule>
{
    public void Configure(EntityTypeBuilder<TaxRule> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TaxCode).IsUnique();
        builder.Property(x => x.RatePercent).HasPrecision(5, 2);
    }
}
