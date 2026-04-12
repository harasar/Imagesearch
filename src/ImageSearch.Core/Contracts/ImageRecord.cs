namespace ImageSearch.Core.Contracts;

public sealed class ImageRecord
{
    public int PictureId { get; init; }

    public required string ImagePath { get; init; }

    public required List<string> Tags { get; init; }
}
