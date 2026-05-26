using Scobius.Core.Interfaces;
using Supabase;

namespace Scobius.Infrastructure.Services;

public class SupabaseFileService : IFileService
{
    private readonly Client _supabase;

    public SupabaseFileService(Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string bucketName)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();

        await _supabase.Storage
            .From(bucketName)
            .Upload(bytes, fileName, new Supabase.Storage.FileOptions { ContentType = contentType, Upsert = true });

        return _supabase.Storage
            .From(bucketName)
            .GetPublicUrl(fileName);
    }

    public async Task DeleteFileAsync(string fileName, string bucketName)
    {
        await _supabase.Storage
            .From(bucketName)
            .Remove(new List<string> { fileName });
    }
}