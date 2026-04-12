using ImageSearch.Core.Contracts;
using ImageSearch.Core.Models;
using ImageSearch.Core.Services;
using ImageSearch.Core.Storage;

const string DefaultIndexPath = "data/image-index.json";

try
{
    return await RunAsync(args);
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Error: {exception.Message}");
    return 1;
}

static async Task<int> RunAsync(string[] args)
{
    if (args.Length == 0)
    {
        PrintHelp();
        return 0;
    }

    var command = args[0].ToLowerInvariant();
    return command switch
    {
        "index" => await RunIndexAsync(args.Skip(1).ToArray()),
        "search" => await RunSearchAsync(args.Skip(1).ToArray()),
        "help" or "--help" or "-h" => PrintHelpAndReturn(),
        _ => throw new InvalidOperationException($"Unknown command: {command}")
    };
}

static async Task<int> RunIndexAsync(string[] args)
{
    if (args.Length == 0)
    {
        throw new InvalidOperationException("Usage: index <image-or-folder> [--index <path>] [--model mock|onnx] [--model-path <path>] [--labels-path <path>] [--input-name <name>] [--output-name <name>]");
    }

    var sourcePath = args[0];
    var options = ParseOptions(args.Skip(1).ToArray());
    var indexPath = GetOption(options, "--index") ?? DefaultIndexPath;
    var modelMode = (GetOption(options, "--model") ?? "mock").ToLowerInvariant();

    var store = new JsonIndexStore(indexPath);
    using var disposableTagger = CreateDisposableTagger(modelMode, options, out var tagger);
    var taggingService = new ImageTaggingService(tagger, store);

    var records = await taggingService.IndexPathAsync(sourcePath);
    foreach (var record in records)
    {
        Console.WriteLine($"[{record.PictureId}] {record.ImagePath}");
        Console.WriteLine($"Tags: {string.Join(", ", record.Tags)}");
    }

    Console.WriteLine($"Indexed {records.Count} image(s) into {Path.GetFullPath(indexPath)}");
    return 0;
}

static async Task<int> RunSearchAsync(string[] args)
{
    if (args.Length == 0)
    {
        throw new InvalidOperationException("Usage: search <tag> [--index <path>]");
    }

    var tag = args[0];
    var options = ParseOptions(args.Skip(1).ToArray());
    var indexPath = GetOption(options, "--index") ?? DefaultIndexPath;
    var store = new JsonIndexStore(indexPath);
    var taggingService = new ImageTaggingService(new MockImageTagger(), store);

    var matches = await taggingService.SearchAsync(tag);
    if (matches.Count == 0)
    {
        Console.WriteLine($"No images found for tag '{tag}'.");
        return 0;
    }

    foreach (var match in matches)
    {
        Console.WriteLine($"[{match.PictureId}] {match.ImagePath}");
        Console.WriteLine($"Tags: {string.Join(", ", match.Tags)}");
    }

    return 0;
}

static IDisposable? CreateDisposableTagger(string modelMode, Dictionary<string, string> options, out IImageTagger tagger)
{
    if (modelMode == "onnx")
    {
        var modelPath = RequireOption(options, "--model-path");
        var labelsPath = RequireOption(options, "--labels-path");
        var onnxTagger = new OnnxImageTagger(new OnnxTaggerOptions
        {
            ModelPath = modelPath,
            LabelsPath = labelsPath,
            InputName = GetOption(options, "--input-name"),
            OutputName = GetOption(options, "--output-name")
        });

        tagger = onnxTagger;
        return onnxTagger;
    }

    tagger = new MockImageTagger();
    return null;
}

static Dictionary<string, string> ParseOptions(string[] args)
{
    var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    for (var i = 0; i < args.Length; i += 2)
    {
        if (i + 1 >= args.Length || !args[i].StartsWith("--", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Invalid option syntax near '{args[i]}'.");
        }

        options[args[i]] = args[i + 1];
    }

    return options;
}

static string? GetOption(IReadOnlyDictionary<string, string> options, string name)
{
    return options.TryGetValue(name, out var value) ? value : null;
}

static string RequireOption(IReadOnlyDictionary<string, string> options, string name)
{
    return GetOption(options, name) ?? throw new InvalidOperationException($"Missing required option: {name}");
}

static void PrintHelp()
{
    Console.WriteLine("ImageSearch CLI");
    Console.WriteLine("Commands:");
    Console.WriteLine("  index <image-or-folder> [--index <path>] [--model mock|onnx]");
    Console.WriteLine("  search <tag> [--index <path>]");
}

static int PrintHelpAndReturn()
{
    PrintHelp();
    return 0;
}
