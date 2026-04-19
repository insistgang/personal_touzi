using PersonalTouzi.Core.Entities;

namespace PersonalTouzi.Infrastructure.Services;

public interface ITransactionSettlementService
{
    Task<Transaction> RecordTransactionAsync(
        RecordTransactionCommand command,
        CancellationToken cancellationToken = default);
}
