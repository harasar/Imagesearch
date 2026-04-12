using System.Text.Json;
using ImageSearch.Core.Contracts;

namespace ImageSearch.Core.Storage;

public sealed class JsonIndexStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly string _indexPath;

    public JsonIndexStore(string indexPath)
    {
        _indexPath = Path.GetFullPath(indexPath);
    }

    public async Task<ImageIndex> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_indexPath))
        {
            return new ImageIndex();
        }

        await using var stream = File.OpenRead(_indexPath);
        return await JsonSerializer.DeserializeAsync<ImageIndex>(stream, SerializerOptions, cancellationToken)
            ?? new ImageIndex();
    }

    public async Task SaveAsync(ImageIndex index, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_indexPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_indexPath);
        await JsonSerializer.SerializeAsync(stream, index, SerializerOptions, cancellationToken);
    }

    public async Task<ImageRecord?> UpsertAsync(TaggingResult result, CancellationToken cancellationToken = default)
    {
        var index = await LoadAsync(cancellationToken);
        var normalizedPath = Path.GetFullPath(result.ImagePath);

        var existing = index.Images.FirstOrDefault(record =>
            string.Equals(Path.GetFullPath(record.ImagePath), normalizedPath, StringComparison.OrdinalIgnoreCase));

        var normalizedTags = result.Tags
            .Where(static tag => !string.IsNullOrWhiteSpace(tag))
            .Select(static tag => tag.Trim().ToLowerInvariant())
            .Distinct()
            .OrderBy(static tag => tag)
            .ToList();

        if (existing is not null)
        {
            var updated = new ImageRecord
            {
                PictureId = existing.PictureId,
                ImagePath = normalizedPath,
                Tags = normalizedTags
            };

            var existingIndex = index.Images.FindIndex(record => record.PictureId == existing.PictureId);
            index.Images[existingIndex] = updated;
            await SaveAsync(index, cancellationToken);
            return updated;
        }

        var nextId = index.Images.Count == 0 ? 1 : index.Images.Max(record => record.PictureId) + 1;
        var created = new ImageRecord
        {
            PictureId = nextId,
            ImagePath = normalizedPath,
            Tags = normalizedTags
        };

        index.Images.Add(created);
        await SaveAsync(index, cancellationToken);
        return created;
    }

    public async Task<IReadOnlyList<ImageRecord>> SearchAsync(string tag, CancellationToken cancellationToken = default)
    {
        var normalizedTag = tag.Trim().ToLowerInvariant();
        var index = await LoadAsync(cancellationToken);

        return index.Images
            .Where(record => record.Tags.Contains(normalizedTag, StringComparer.OrdinalIgnoreCase))
            .OrderBy(record => record.PictureId)
            .ToList();
    }
}
