namespace Dishwise.Application.Interfaces;

public interface IBlobServices
{
    Task<string> UploadBlobAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteBlobAsync(string blobUrl);
}