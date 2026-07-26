using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Identity;
using YAERP.Domain.Finance;

namespace YAERP.Infrastructure.Persistence.Configurations.Finance;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasConversion(id => id.Value, value => new AccountId(value));
        builder.Property(a => a.TenantId).HasConversion(id => id.Value, value => new TenantId(value)).IsRequired();
        builder.Property(a => a.AccountNumber).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(250);
        builder.Property(a => a.Type).HasConversion<string>().IsRequired();
        builder.Property(a => a.Balance).HasPrecision(18, 4);

        builder.HasIndex(a => new { a.TenantId, a.AccountNumber }).IsUnique();
    }
}
