using System;

namespace YAERP.Application.Finance.DTOs;

public record InvoiceDto(Guid InvoiceId, string InvoiceNumber, decimal TotalAmount, DateTime DueDateUtc);
