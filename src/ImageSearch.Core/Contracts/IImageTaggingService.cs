namespace ImageSearch.Core.Contracts;

public interface IImageTaggingService
{
    Task<ImageRecord> TagImageAsync(string imagePath, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImageRecord>> TagDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImageRecord>> IndexPathAsync(string inputPath, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImageRecord>> SearchAsync(string tag, CancellationToken cancellationToken = default);

    Task<ImageRecord> SaveAsync(TaggingResult result, CancellationToken cancellationToken = default);
}
