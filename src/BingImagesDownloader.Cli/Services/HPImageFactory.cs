namespace BingImagesDownloader.Cli.Services;

using System.Globalization;

public static class HPImageFactory
{
    public static IEnumerable<string> GetUrls(uint index = 0)
    {
        const int count = 7;
        var cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures);

        foreach (var cultureInfo in cultureInfos)
        {
            yield return $"https://www.bing.com/HPImageArchive.aspx?format=js&idx={index * count}&n={count}&mkt={cultureInfo.Name}";
        }
    }

    public static IEnumerable<string> GetUrls(uint index, string name)
    {
        yield return $"https://www.bing.com/HPImageArchive.aspx?format=js&idx={index}&n=8&mkt={name}";
    }
}
