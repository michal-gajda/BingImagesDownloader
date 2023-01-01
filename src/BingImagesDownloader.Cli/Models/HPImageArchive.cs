namespace BingImagesDownloader.Cli.Models;

using System.Text.Json.Serialization;

public sealed record HPImageArchive
{
    [JsonPropertyName("images")] public HPImage[] Images { get; init; } = default!;
}
