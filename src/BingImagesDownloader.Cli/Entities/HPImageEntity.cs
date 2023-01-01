namespace BingImagesDownloader.Cli.Entities;

public sealed class HPImageEntity
{
    public Guid Id { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
