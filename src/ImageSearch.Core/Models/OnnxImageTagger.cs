using System.Collections.Immutable;
using ImageSearch.Core.Contracts;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageSearch.Core.Models;

public sealed class OnnxImageTagger : IImageTagger, IDisposable
{
    private readonly InferenceSession _session;
    private readonly OnnxTaggerOptions _options;
    private readonly ImmutableArray<string> _labels;
    private readonly string _inputName;
    private readonly string _outputName;

    public OnnxImageTagger(OnnxTaggerOptions options)
    {
        _options = options;
        _labels = File.ReadAllLines(options.LabelsPath)
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .Select(static line => line.Trim())
            .ToImmutableArray();

        _session = new InferenceSession(options.ModelPath);
        _inputName = !string.IsNullOrWhiteSpace(options.InputName)
            ? options.InputName
            : _session.InputMetadata.Keys.First();
        _outputName = !string.IsNullOrWhiteSpace(options.OutputName)
            ? options.OutputName
            : _session.OutputMetadata.Keys.First();
    }

    public Task<TaggingResult> TagAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var image = Image.Load<Rgb24>(imagePath);
        image.Mutate(context => context.Resize(new ResizeOptions
        {
            Size = new Size(_options.ImageSize, _options.ImageSize),
            Mode = ResizeMode.Crop
        }));

        var tensor = new DenseTensor<float>([1, _options.ChannelCount, _options.ImageSize, _options.ImageSize]);

        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < _options.ImageSize; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < _options.ImageSize; x++)
                {
                    var pixel = row[x];
                    tensor[0, 0, y, x] = Normalize(pixel.R / 255f, _options.Mean[0], _options.StdDev[0]);
                    tensor[0, 1, y, x] = Normalize(pixel.G / 255f, _options.Mean[1], _options.StdDev[1]);
                    tensor[0, 2, y, x] = Normalize(pixel.B / 255f, _options.Mean[2], _options.StdDev[2]);
                }
            }
        });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(_inputName, tensor)
        };

        using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _session.Run(inputs);
        var output = results.First(result => string.Equals(result.Name, _outputName, StringComparison.Ordinal));
        var scores = Softmax(output.AsEnumerable<float>().ToArray());
        var tags = SelectTopTags(scores);

        return Task.FromResult(new TaggingResult(imagePath, tags));
    }

    public void Dispose()
    {
        _session.Dispose();
    }

    private IReadOnlyList<string> SelectTopTags(float[] scores)
    {
        var tags = scores
            .Select((score, index) => new { Index = index, Score = score })
            .Where(item => item.Index < _labels.Length && item.Score >= _options.ScoreThreshold)
            .OrderByDescending(item => item.Score)
            .Take(_options.TopK)
            .Select(item => NormalizeTag(_labels[item.Index]))
            .Distinct()
            .ToList();

        if (tags.Count > 0)
        {
            return tags;
        }

        var bestIndex = Array.IndexOf(scores, scores.Max());
        if (bestIndex >= 0 && bestIndex < _labels.Length)
        {
            return [NormalizeTag(_labels[bestIndex])];
        }

        return ["unknown"];
    }

    private static float Normalize(float value, float mean, float stdDev)
    {
        return (value - mean) / stdDev;
    }

    private static float[] Softmax(float[] values)
    {
        if (values.Length == 0)
        {
            return values;
        }

        var max = values.Max();
        var exp = values.Select(value => MathF.Exp(value - max)).ToArray();
        var sum = exp.Sum();

        if (sum <= 0)
        {
            return values;
        }

        for (var i = 0; i < exp.Length; i++)
        {
            exp[i] /= sum;
        }

        return exp;
    }

    private static string NormalizeTag(string label)
    {
        return label.Trim().ToLowerInvariant();
    }
}
