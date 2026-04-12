using ImageSearch.Core.Contracts;
using ImageSearch.Core.Models;
using ImageSearch.Core.Storage;

namespace ImageSearch.Core.Services;

public static class OnnxImageTaggingServiceFactory
{
    public static IImageTaggingService Create(string modelPath, string labelsPath, string indexPath)
    {
        var store = new JsonIndexStore(indexPath);
        var tagger = new OnnxImageTagger(new OnnxTaggerOptions
        {
            ModelPath = modelPath,
            LabelsPath = labelsPath
        });

        return new ImageTaggingService(tagger, store);
    }
}
