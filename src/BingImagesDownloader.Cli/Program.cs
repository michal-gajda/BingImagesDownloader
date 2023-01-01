using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using BingImagesDownloader.Cli.Entities;
using BingImagesDownloader.Cli.Models;
using BingImagesDownloader.Cli.Services;

var stopwatch = new Stopwatch();
stopwatch.Start();

var regexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
var regex = new Regex(@"^/th\?id=OHR\.(?<ImageFileName>[0-9a-zA-Z_]+)_{1,}(([A-Z]{2}\-[A-Z]{2})|(ROW)){1}[0-9]{10}$", regexOptions, TimeSpan.FromMilliseconds(100));

var fileName = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Bing.db");

var parts = new[]
{
        new KeyValuePair<string, string>("Filename", fileName),
        new KeyValuePair<string, string>("Connection", nameof(LiteDB.ConnectionType.Shared)),
    };

var connectionString = string.Join(";", parts.Select(part => $"{part.Key}={part.Value}"));

using var db = new LiteDB.LiteDatabase(connectionString);

var hpImageEntities = db.GetCollection<HPImageEntity>(nameof(HPImageEntity));

var httpClient = new HttpClient();

uint startIndex = 0;
uint endIndex = 2;

for (uint index = startIndex; index < endIndex; index++)
{
    var json = string.Empty;

    foreach (var requestUrl in HPImageFactory.GetUrls(index/*, "en-US"*/))
    {
        try
        {
            json = await httpClient.GetStringAsync(requestUrl);

            var images = JsonSerializer.Deserialize<HPImageArchive>(json);

            if (images is null)
            {
                continue;
            }

            foreach (var image in images.Images)
            {
                if (hpImageEntities.FindOne(ie => ie.Hash == image.Hash) is null)
                {
                    hpImageEntities.Insert(new HPImageEntity
                    {
                        Hash = image.Hash,
                        Url = image.Url,
                    });
                }
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(requestUrl);
            Console.WriteLine(json);
            Console.WriteLine(exception);
            return;
        }
    }
}

hpImageEntities.EnsureIndex(index => index.Url);

var urls = hpImageEntities
    .Query()
    .Select(entity => entity.Url)
    .ToList();
urls.Sort();

var imageEntities = db.GetCollection<ImageEntity>(nameof(ImageEntity));

foreach (var url in urls)
{
    var match = regex.Match(url);

    if (match.Success)
    {
        var name = match.Groups["ImageFileName"].Value;

        var imageEntity = imageEntities.FindOne(e => e.Name == name);

        if (imageEntity is null)
        {
            imageEntities.Insert(new ImageEntity
            {
                Hash = string.Empty,
                Name = name,
                Urls = new()
                    {
                        url,
                    },
            });
        }
        else
        {
            if (!imageEntity.Urls.Contains(url))
            {
                var sourceUrls = new List<string>(imageEntity.Urls);
                sourceUrls.Add(url);
                imageEntity.Urls = sourceUrls;
                imageEntities.Update(imageEntity);
            }
        }
    }
    else
    {
        Console.WriteLine(url);
    }
}

var jobs = imageEntities.Query().Select(ef => new
{
    FileName = ef.Name,
    Url = ef.Urls.Last(),
}).ToList();

foreach (var job in jobs)
{
    var url = job.Url;
    var imageFileName = job.FileName;
    var link = $"https://www.bing.com{url}_UHD.jpg";
    var targetFile = $"{imageFileName}.jpg";
    var targetFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), targetFile);

    if (File.Exists(targetFilePath))
    {
        continue;
    }

    try
    {
        var imageBlob = await httpClient.GetByteArrayAsync(link);
        await File.WriteAllBytesAsync(targetFilePath, imageBlob);
        Console.WriteLine($"{link}");
    }
    catch (Exception exception)
    {
        Console.WriteLine(exception);
    }
}

stopwatch.Stop();

Console.WriteLine(string.Format("Done in {0}s at {1}", Convert.ToInt32(stopwatch.Elapsed.TotalSeconds + 0.5), DateTime.Now.ToString("d MMM HH:mm")));