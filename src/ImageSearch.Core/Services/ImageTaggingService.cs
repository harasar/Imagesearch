using ImageSearch.Core.Contracts;
using ImageSearch.Core.Storage;

namespace ImageSearch.Core.Services;

public sealed class ImageTaggingService : IImageTaggingService
{
    private readonly IImageTagger _imageTagger;
    private readonly JsonIndexStore _store;

    public ImageTaggingService(IImageTagger imageTagger, JsonIndexStore store)
    {
        _imageTagger = imageTagger;
        _store = store;
    }

    public async Task<ImageRecord> TagImageAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        var resolvedPath = ImageInputResolver.ResolveImages(imagePath).Single();
        var taggingResult = await _imageTagger.TagAsync(resolvedPath, cancellationToken);
        return await SaveAsync(taggingResult, cancellationToken);
    }

    public async Task<IReadOnlyList<ImageRecord>> TagDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        var files = ImageInputResolver.ResolveImages(directoryPath);
        var indexed = new List<ImageRecord>(files.Count);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var taggingResult = await _imageTagger.TagAsync(file, cancellationToken);
            indexed.Add(await SaveAsync(taggingResult, cancellationToken));
        }

        return indexed;
    }

    public async Task<IReadOnlyList<ImageRecord>> IndexPathAsync(string inputPath, CancellationToken cancellationToken = default)
    {
        var resolvedItems = ImageInputResolver.ResolveImages(inputPath);
        if (resolvedItems.Count == 1 && File.Exists(resolvedItems[0]))
        {
            return [await TagImageAsync(resolvedItems[0], cancellationToken)];
        }

        return await TagDirectoryAsync(inputPath, cancellationToken);
    }

    public async Task<IReadOnlyList<ImageRecord>> SearchAsync(string tag, CancellationToken cancellationToken = default)
    {
        return await _store.SearchAsync(tag, cancellationToken);
    }

    public async Task<ImageRecord> SaveAsync(TaggingResult result, CancellationToken cancellationToken = default)
    {
        return await _store.UpsertAsync(result, cancellationToken)
            ?? throw new InvalidOperationException("Image tagging result could not be saved.");
    }
}
