using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using StockWise.Models;

namespace StockWise.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blob;
        private readonly AzureStorageOptions _opts;

        public BlobStorageService(BlobServiceClient blob, IOptions<AzureStorageOptions> opts)
        {
            _blob = blob;
            _opts = opts.Value;
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string? contentType = null, CancellationToken ct = default)
        {
            var container = _blob.GetBlobContainerClient(_opts.ContainerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);
            var blob = container.GetBlobClient(fileName);

            var headers = new BlobHttpHeaders
            {
                ContentType = string.IsNullOrEmpty(contentType) ? "Application/octet-stream" : contentType,
                CacheControl = "public, max-age=31536000, immutable"
            };

            await blob.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = headers }, ct);

            return blob.Uri.ToString();
        }

        public async Task DeleteAsync(string image)
        {
            var oldBlobName = Path.GetFileName(new Uri(image).AbsolutePath);
            var container = _blob.GetBlobContainerClient(_opts.ContainerName);
            var blob = container.GetBlobClient(oldBlobName);
            await blob.DeleteIfExistsAsync();
        }
        
    }
}
