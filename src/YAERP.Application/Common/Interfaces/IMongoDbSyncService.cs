using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YAERP.Application.Common.Interfaces;

public record MongoDbSettingsDto(
    string ConnectionString,
    string DatabaseName,
    bool EnableEncryption,
    string EncryptionKey,
    bool AutoSyncDynamicChanges,
    bool IsConnected);

public record MongoDocumentRecordDto(
    string Id,
    string CollectionName,
    string RecordKey,
    string EncryptedJsonPayload,
    DateTime LastModifiedUtc,
    string ChangeType);

public interface IMongoDbSyncService
{
    Task<MongoDbSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task SaveSettingsAsync(MongoDbSettingsDto settings, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(string connectionString, string databaseName, CancellationToken cancellationToken = default);
    Task<bool> AutoSetupDatabaseAsync(string connectionString, string databaseName, CancellationToken cancellationToken = default);
    Task SaveDynamicRecordAsync(string collectionName, string recordKey, object payload, bool encrypt, CancellationToken cancellationToken = default);
    Task<IEnumerable<MongoDocumentRecordDto>> FetchStaticDatabaseSnapshotAsync(string collectionName, CancellationToken cancellationToken = default);
    Task StartChangeDetectionWatcherAsync(Func<MongoDocumentRecordDto, Task> onChangeDetected, CancellationToken cancellationToken = default);
}
