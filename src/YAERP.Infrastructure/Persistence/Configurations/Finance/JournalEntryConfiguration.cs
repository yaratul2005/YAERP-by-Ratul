using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YAERP.Domain.Identity;
using YAERP.Domain.Finance;

namespace YAERP.Infrastructure.Persistence.Configurations.Finance;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id).HasConversion(id => id.Value, value => new JournalEntryId(value));
        builder.Property(j => j.TenantId).HasConversion(id => id.Value, value => new TenantId(value)).IsRequired();
        builder.Property(j => j.EntryNumber).IsRequired().HasMaxLength(50);
        builder.Property(j => j.Description).HasMaxLength(500);

        builder.OwnsMany(j => j.Lines, jb =>
        {
            jb.ToTable("JournalLines");
            jb.WithOwner().HasForeignKey("JournalEntryId");
            jb.HasKey(l => l.Id);
            jb.Property(l => l.Id).HasConversion(id => id.Value, value => new JournalLineId(value));
            jb.Property(l => l.AccountId).HasConversion(id => id.Value, value => new AccountId(value)).IsRequired();
            jb.Property(l => l.Debit).HasPrecision(18, 4);
            jb.Property(l => l.Credit).HasPrecision(18, 4);
            jb.Property(l => l.Memo).HasMaxLength(250);
        });

        builder.HasIndex(j => new { j.TenantId, j.EntryNumber }).IsUnique();
    }
}
