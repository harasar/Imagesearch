using ImageSearch.Core.Services;

// Example usage from another C# application such as a gallery app.
// Adjust the paths below to match your deployed model and data locations.

var service = OnnxImageTaggingServiceFactory.Create(
    modelPath: "./models/mobilenetv2-12-int8.onnx",
    labelsPath: "./models/imagenet_classes.txt",
    indexPath: "./data/image-index.json");

// Tag a single image and save/update it in the JSON index.
var singleImageRecord = await service.TagImageAsync("/path/to/image.jpg");
Console.WriteLine($"Single image: {singleImageRecord.ImagePath}");
Console.WriteLine($"Tags: {string.Join(", ", singleImageRecord.Tags)}");

// Index all supported images under a directory.
var folderRecords = await service.TagDirectoryAsync("/path/to/image-folder");
Console.WriteLine($"Indexed {folderRecords.Count} file(s) from folder.");

// Search previously indexed images by tag.
var matches = await service.SearchAsync("dog");
foreach (var match in matches)
{
    Console.WriteLine($"[{match.PictureId}] {match.ImagePath}");
    Console.WriteLine($"Tags: {string.Join(", ", match.Tags)}");
}
