using ImageSearch.Core.Contracts;

namespace ImageSearch.Core.Models;

public sealed class MockImageTagger : IImageTagger
{
    public Task<TaggingResult> TagAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileNameWithoutExtension(imagePath);
        var splitTags = fileName
            .Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(static value => value.ToLowerInvariant())
            .Distinct()
            .ToList();

        if (splitTags.Count == 0)
        {
            splitTags.Add("unknown");
        }

        return Task.FromResult(new TaggingResult(imagePath, splitTags));
    }
}
