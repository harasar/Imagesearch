namespace ImageSearch.Core.Models;

public sealed class OnnxTaggerOptions
{
    public required string ModelPath { get; init; }

    public required string LabelsPath { get; init; }

    public string? InputName { get; init; }

    public string? OutputName { get; init; }

    public int ImageSize { get; init; } = 224;

    public int ChannelCount { get; init; } = 3;

    public int TopK { get; init; } = 5;

    public float ScoreThreshold { get; init; } = 0.0f;

    public float[] Mean { get; init; } = [0.485f, 0.456f, 0.406f];

    public float[] StdDev { get; init; } = [0.229f, 0.224f, 0.225f];
}
