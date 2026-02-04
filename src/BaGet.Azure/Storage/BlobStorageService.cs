using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using BaGet.Core;

namespace BaGet.Azure
{
    // See: https://github.com/NuGet/NuGetGallery/blob/master/src/NuGetGallery.Core/Services/CloudBlobCoreFileStorageService.cs
    public class BlobStorageService : IStorageService
    {
        private readonly BlobContainerClient _container;

        public BlobStorageService(BlobContainerClient container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public async Task<Stream?> GetAsync(string path, CancellationToken cancellationToken)
        {
            var blob = _container.GetBlobClient(path);

            try
            {
                var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
                return response.Value.Content;
            }
            catch (RequestFailedException e) when (e.Status == (int)HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public Task<Uri> GetDownloadUriAsync(string path, CancellationToken cancellationToken)
        {
            var blob = _container.GetBlobClient(path);

            // TODO: Make expiry time configurable.
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _container.Name,
                BlobName = path,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(10))
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blob.GenerateSasUri(sasBuilder);
            return Task.FromResult(sasUri);
        }

        public async Task<StoragePutResult> PutAsync(
            string path,
            Stream content,
            string contentType,
            CancellationToken cancellationToken)
        {
            var blob = _container.GetBlobClient(path);
            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }
            };

            try
            {
                await blob.UploadAsync(content, options, cancellationToken);
                return StoragePutResult.Success;
            }
            catch (RequestFailedException e) when (e.Status == (int)HttpStatusCode.Conflict ||
                                                    e.Status == (int)HttpStatusCode.PreconditionFailed)
            {
                using var targetStream = await blob.OpenReadAsync(cancellationToken: cancellationToken);
                content.Position = 0;
                return content.Matches(targetStream)
                    ? StoragePutResult.AlreadyExists
                    : StoragePutResult.Conflict;
            }
        }

        public async Task DeleteAsync(string path, CancellationToken cancellationToken)
        {
            var blob = _container.GetBlobClient(path);
            await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
    }
}
