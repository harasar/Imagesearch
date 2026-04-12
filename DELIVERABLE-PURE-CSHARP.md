# Pure C# Deliverable

This deliverable is the production-oriented shape for integration into an existing C# gallery application:

- `ImageSearch.Core` provides the reusable C# tagging service API.
- `ImageSearch.Cli` is only a test harness.
- The model runtime is pure C# via `Microsoft.ML.OnnxRuntime`.
- No Node.js or npm is required for this package.

## Main C# entry points

- `ImageSearch.Core.Services.ImageTaggingService`
- `ImageSearch.Core.Services.OnnxImageTaggingServiceFactory`
- `ImageSearch.Core.Models.OnnxImageTagger`

## Example integration

```csharp
using ImageSearch.Core.Services;

var service = OnnxImageTaggingServiceFactory.Create(
    modelPath: "./models/mobilenetv2-12-int8.onnx",
    labelsPath: "./models/imagenet_classes.txt",
    indexPath: "./data/image-index.json");

var record = await service.TagImageAsync("/path/to/image.jpg");
Console.WriteLine(string.Join(", ", record.Tags));
```

## Notes

- This is a pure C# integration shape, but the included model is still an ImageNet classifier, so tagging quality is limited.
- It matches the requirement of C# + model files only.
