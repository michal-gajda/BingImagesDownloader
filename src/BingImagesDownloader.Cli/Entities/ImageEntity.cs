namespace BingImagesDownloader.Cli.Entities;

public sealed class ImageEntity
{
    public Guid Id { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Urls { get; set; } = new();
}
