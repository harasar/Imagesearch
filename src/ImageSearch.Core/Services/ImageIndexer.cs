using ImageSearch.Core.Contracts;
using ImageSearch.Core.Storage;

namespace ImageSearch.Core.Services;

public sealed class ImageIndexer
{
    private readonly ImageTaggingService _service;

    public ImageIndexer(IImageTagger imageTagger, JsonIndexStore store)
    {
        _service = new ImageTaggingService(imageTagger, store);
    }

    public async Task<IReadOnlyList<ImageRecord>> IndexPathAsync(string inputPath, CancellationToken cancellationToken = default)
    {
        return await _service.IndexPathAsync(inputPath, cancellationToken);
    }
}
