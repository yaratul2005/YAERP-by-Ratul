using System;
using YAERP.Application.Common.Messaging;
using YAERP.Domain.Finance;

namespace YAERP.Application.Finance.Commands.CreateAccount;

public record CreateAccountCommand(
    string AccountNumber,
    string Name,
    AccountType Type) : ICommand<Guid>;
