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
        // Supabase-csharp Storage.Upload can take a Stream, but let's ensure it's at the beginning
        if (fileStream.CanSeek)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
        }

        await _supabase.Storage
            .From(bucketName)
            .Upload(fileStream, fileName, new Supabase.Storage.FileOptions { ContentType = contentType, Upsert = true });

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