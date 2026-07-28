using System;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Sales.Commands.UpdateCustomerProfile;

public record UpdateCustomerProfileCommand(
    Guid CustomerId,
    string Name,
    string? Email,
    string? Phone,
    string? ShippingAddress,
    decimal CreditLimit) : ICommand<bool>;
