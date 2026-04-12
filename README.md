# Deliverable: Pure C# ONNX

This folder is the pure C# handoff package for integration into an existing C# gallery application.

## Contents

- `src/ImageSearch.Core`: reusable tagging library
- `src/ImageSearch.Cli`: test harness only
- `models/mobilenetv2-12-int8.onnx`: current ONNX model
- `models/imagenet_classes.txt`: labels used by the model

## Build server steps

From this folder:

```bash
dotnet restore src/ImageSearch.Cli/ImageSearch.Cli.csproj
dotnet build src/ImageSearch.Cli/ImageSearch.Cli.csproj -c Release
```

If your normal Tizen flow publishes for a specific runtime, do that in your existing build pipeline.

## Test run

```bash
dotnet run --project src/ImageSearch.Cli --configuration Release -- index /path/to/image.jpg \
  --model onnx \
  --model-path ./models/mobilenetv2-12-int8.onnx \
  --labels-path ./models/imagenet_classes.txt
```

## C# integration

Use `ImageSearch.Core` from your gallery application.

Example:

```csharp
using ImageSearch.Core.Services;

var service = OnnxImageTaggingServiceFactory.Create(
    modelPath: "./models/mobilenetv2-12-int8.onnx",
    labelsPath: "./models/imagenet_classes.txt",
    indexPath: "./data/image-index.json");

var record = await service.TagImageAsync("/path/to/image.jpg");
```

A ready-to-read sample is also included here:

- `GalleryIntegrationExample.cs`

## Target deployment

For your production Tizen flow:

1. Build on the build server.
2. Package the resulting application/library plus the model files into your RPM.
3. Install the RPM on the target.
4. Ensure the model and labels files are deployed to known paths accessible by the gallery app.

## Note

This package is pure C# plus model files only. It does not require Node.js.
The current model is a classifier, so tagging quality is limited.
