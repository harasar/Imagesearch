namespace ImageSearch.Core.Contracts;

public sealed record TaggingResult(
    string ImagePath,
    IReadOnlyList<string> Tags);
