using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;

namespace BaGet.Azure
{
    public class AzureSearchBatchIndexer
    {
        /// <summary>
        /// Azure Search accepts batches of up to 1000 documents.
        /// </summary>
        public const int MaxBatchSize = 1000;

        private readonly SearchClient _searchClient;
        private readonly ILogger<AzureSearchBatchIndexer> _logger;

        public AzureSearchBatchIndexer(
            SearchClient searchClient,
            ILogger<AzureSearchBatchIndexer> logger)
        {
            _searchClient = searchClient ?? throw new ArgumentNullException(nameof(searchClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task IndexAsync(
            IReadOnlyList<IndexDocumentsAction<PackageDocument>> batch,
            CancellationToken cancellationToken)
        {
            if (batch.Count > MaxBatchSize)
            {
                throw new ArgumentException(
                    $"Batch cannot have more than {MaxBatchSize} elements",
                    nameof(batch));
            }

            try
            {
                var indexBatch = IndexDocumentsBatch.Create(batch.ToArray());
                await _searchClient.IndexDocumentsAsync(indexBatch, cancellationToken: cancellationToken);

                _logger.LogInformation("Pushed batch of {DocumentCount} documents", batch.Count);
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.RequestEntityTooLarge && batch.Count > 1)
            {
                var halfCount = batch.Count / 2;
                var halfA = batch.Take(halfCount).ToList();
                var halfB = batch.Skip(halfCount).ToList();

                _logger.LogWarning(
                    ex,
                    "The request body for a batch of {BatchSize} was too large. Splitting into two batches of size " +
                    "{HalfA} and {HalfB}.",
                    batch.Count,
                    halfA.Count,
                    halfB.Count);

                await IndexAsync(halfA, cancellationToken);
                await IndexAsync(halfB, cancellationToken);
            }
        }
    }
}
