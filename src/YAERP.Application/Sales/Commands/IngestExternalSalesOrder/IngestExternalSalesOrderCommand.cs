using System;
using YAERP.Application.Common.Messaging;
using YAERP.Application.Sales.DTOs;

namespace YAERP.Application.Sales.Commands.IngestExternalSalesOrder;

public record IngestExternalSalesOrderCommand(
    ExternalOrderWebhookDto WebhookPayload,
    string RawPayloadJson,
    string SignatureHeader,
    string SecretKey) : ICommand<Guid>;
