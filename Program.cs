using System.Text;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

async Task<List<string>> getLinksAsync(Site site)
{
    // Это для кодировки windows-1251
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    try
    {
        using (HttpClient httpClient = new HttpClient())
        {
            HttpResponseMessage response = await httpClient.GetAsync(site.Url);

            if (response.IsSuccessStatusCode)
            {
                string htmlContent = await response.Content.ReadAsStringAsync();

                HtmlParser parser = new HtmlParser();
                IHtmlDocument document = await parser.ParseDocumentAsync(htmlContent);

                List<string> links = new List<string>();

                // Получаем все теги <a>
                var linkElements = document.QuerySelectorAll("a");

                foreach (var linkElement in linkElements)
                {
                    // Получаем аттрибуты href у каждой ссылки
                    string? hrefValue = linkElement.GetAttribute("href");

                    if (!string.IsNullOrEmpty(hrefValue))
                    {
                        links.Add(hrefValue);
                    }
                }

                return links.Distinct().ToList();
            }
            else
            {
                throw new GetLinksException($"Ошибка при выполнении запроса к {site}: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
    }
    catch (HttpRequestException e)
    {
        throw new GetLinksException($"Ошибка при выполнении запроса к {site}: {e.Message}");
    }
}

using (UpdateCheckContext db = new UpdateCheckContext())
{
    var sites = db.Sites.ToList();

    foreach (var site in sites)
    {
        List<string> links = await getLinksAsync(site);
        
        bool itFirstStart = db.Links.Any(links => links.Site == site);

        foreach (string url in links)
        {
            if (!db.Links.Any(link => link.Site == site && link.Url == url))
            {
                db.Add(new Link { Site = site, Url = url, Posted = !itFirstStart });
            }
        }

        db.SaveChanges();
    }
}

class GetLinksException : Exception
{
    public GetLinksException(string message)
        : base(message) {}
}
