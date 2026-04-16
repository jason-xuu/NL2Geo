namespace NL2Geo.Adapters;

public interface IRhinoDisplayAdapter
{
    void ShowPreview(string previewLabel);
}

public sealed class RhinoDisplayAdapter : IRhinoDisplayAdapter
{
    public List<string> PreviewItems { get; } = new();

    public void ShowPreview(string previewLabel)
    {
        PreviewItems.Add(previewLabel);
    }
}
