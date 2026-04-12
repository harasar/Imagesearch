namespace ImageSearch.Core.Contracts;

public interface IImageTagger
{
    Task<TaggingResult> TagAsync(string imagePath, CancellationToken cancellationToken = default);
}
