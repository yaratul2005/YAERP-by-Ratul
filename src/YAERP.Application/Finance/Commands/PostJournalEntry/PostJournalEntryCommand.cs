using System;
using System.Collections.Generic;
using YAERP.Application.Common.Messaging;

namespace YAERP.Application.Finance.Commands.PostJournalEntry;

public record JournalLineDto(Guid AccountId, decimal Debit, decimal Credit, string? Memo);

public record PostJournalEntryCommand(
    string EntryNumber,
    string? Description,
    List<JournalLineDto> Lines) : ICommand<Guid>;
