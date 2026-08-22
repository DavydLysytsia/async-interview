using AsyncInterview.Api.Data;
using AsyncInterview.Api.Models;
using Google.Apis.Json;
using Google.Apis.Util.Store;
using Microsoft.EntityFrameworkCore;

namespace AsyncInterview.Api.Services;

// IDataStore backed by the app database, so the Google auth library keeps each
// user's OAuth tokens in SQLite instead of files on disk.
public class EfDataStore : IDataStore
{
    private readonly AppDbContext _db;

    public EfDataStore(AppDbContext db)
    {
        _db = db;
    }

    private static string RowKey<T>(string key) => $"{key}-{typeof(T).FullName}";

    public async Task StoreAsync<T>(string key, T value)
    {
        var rowKey = RowKey<T>(key);
        var json = NewtonsoftJsonSerializer.Instance.Serialize(value);
        var existing = await _db.YouTubeTokens.FindAsync(rowKey);
        if (existing == null)
        {
            _db.YouTubeTokens.Add(new YouTubeToken { Key = rowKey, Json = json, UpdatedAt = DateTime.UtcNow });
        }
        else
        {
            existing.Json = json;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
    }

    public async Task<T> GetAsync<T>(string key)
    {
        var row = await _db.YouTubeTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Key == RowKey<T>(key));
        if (row == null) return default!;
        return NewtonsoftJsonSerializer.Instance.Deserialize<T>(row.Json);
    }

    public async Task DeleteAsync<T>(string key)
    {
        var row = await _db.YouTubeTokens.FindAsync(RowKey<T>(key));
        if (row != null)
        {
            _db.YouTubeTokens.Remove(row);
            await _db.SaveChangesAsync();
        }
    }

    public async Task ClearAsync()
    {
        _db.YouTubeTokens.RemoveRange(_db.YouTubeTokens);
        await _db.SaveChangesAsync();
    }
}
