using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Dishwise.Application.Interfaces;

namespace Dishwise.Application.Services
{
    public class BlobServices : IBlobServices
    {
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient? _blobServiceClient;
        private readonly string _containerName = "recipeimages";

        public BlobServices(IConfiguration configuration)
        {
            _configuration = configuration;

            // A missing connection string only fails when blob storage is actually used,
            // so the API still starts (and recipes without uploaded images still work).
            var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
            if (!string.IsNullOrWhiteSpace(connectionString))
                _blobServiceClient = new BlobServiceClient(connectionString);
        }

        private BlobServiceClient RequireBlobServiceClient() =>
            _blobServiceClient ?? throw new InvalidOperationException("AzureBlobStorage connection string is not configured.");

        public async Task<string> UploadBlobAsync(Stream fileStream, string fileName, string contentType)
        {
            var containerClient = RequireBlobServiceClient().GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });

            return blobClient.Uri.ToString();
        }

        public async Task DeleteBlobAsync(string blobUrl)
        {
            var containerClient = RequireBlobServiceClient().GetBlobContainerClient(_containerName);
            var blobName = Path.GetFileName(blobUrl);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}