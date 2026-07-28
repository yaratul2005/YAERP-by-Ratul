using System;
using System.Collections.Generic;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Financials.Commands.PostJournalEntry;

public record JournalLineDto(string AccountCode, string AccountName, string Description, decimal Debit, decimal Credit);

public record PostJournalEntryCommand(
    string ReferenceNumber,
    string Description,
    List<JournalLineDto> Lines) : ICommand<Guid>;
