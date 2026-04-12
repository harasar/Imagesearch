namespace ImageSearch.Core.Services;

public static class ImageInputResolver
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".bmp",
        ".webp"
    };

    public static IReadOnlyList<string> ResolveImages(string inputPath)
    {
        var fullPath = Path.GetFullPath(inputPath);

        if (File.Exists(fullPath))
        {
            ValidateExtension(fullPath);
            return [fullPath];
        }

        if (Directory.Exists(fullPath))
        {
            return Directory
                .EnumerateFiles(fullPath, "*.*", SearchOption.AllDirectories)
                .Where(file => SupportedExtensions.Contains(Path.GetExtension(file)))
                .OrderBy(static file => file, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        throw new FileNotFoundException($"Input path was not found: {fullPath}");
    }

    private static void ValidateExtension(string inputPath)
    {
        if (!SupportedExtensions.Contains(Path.GetExtension(inputPath)))
        {
            throw new InvalidOperationException($"Unsupported image format: {inputPath}");
        }
    }
}
