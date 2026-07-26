using System;
using YAERP.Domain.Common.Primitives;
using YAERP.Domain.Identity;

namespace YAERP.Domain.Finance;

public class Account : AggregateRoot<AccountId>
{
    private Account(AccountId id, TenantId tenantId, string accountNumber, string name, AccountType type, bool isActive, decimal balance) : base(id)
    {
        TenantId = tenantId;
        AccountNumber = accountNumber;
        Name = name;
        Type = type;
        IsActive = isActive;
        Balance = balance;
    }

    private Account() { }

    public TenantId TenantId { get; private set; } = default!;
    public string AccountNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public AccountType Type { get; private set; }
    public bool IsActive { get; private set; }
    public decimal Balance { get; private set; }

    public static Account Create(TenantId tenantId, string accountNumber, string name, AccountType type)
    {
        return new Account(new AccountId(Guid.NewGuid()), tenantId, accountNumber, name, type, true, 0m);
    }

    public void UpdateBalance(decimal amount)
    {
        // Depending on account type, amount could naturally mean debit/credit increasing balance.
        // We assume 'amount' here is a direct delta applied algebraically, or logic would be more complex based on AccountType.
        Balance += amount;
    }
}
