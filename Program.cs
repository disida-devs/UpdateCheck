using System.Text;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

async Task<List<string?>> getLinksAsync(string site)
{
    // Это для кодировки windows-1251
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    try
    {
        using (HttpClient httpClient = new HttpClient())
        {
            HttpResponseMessage response = await httpClient.GetAsync("http://www.ettu.ru/news/");

            if (response.IsSuccessStatusCode)
            {
                string htmlContent = await response.Content.ReadAsStringAsync();

                HtmlParser parser = new HtmlParser();
                IHtmlDocument document = await parser.ParseDocumentAsync(htmlContent);

                List<string?> links = new List<string?>();

                // Получаем все теги <a>
                var linkElements = document.QuerySelectorAll("a");

                foreach (var linkElement in linkElements)
                {
                    // Получаем аттрибуты href у каждой ссылки
                    string? hrefValue = linkElement.GetAttribute("href");
                    links.Add(hrefValue);
                }

                return links;
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

// Проверяем работу модели
using (UpdateCheckContext db = new UpdateCheckContext())
{
    // Сайты
    Site site0 = new Site { Name = "8642", Url = "https://8642.ru/news" };
    Site site1 = new Site { Name = "Сом", Url = "https://som.ru/wtf" };

    db.Sites.AddRange(site0, site1);

    // Ссылки
    Link link0 = new Link { Site = site0, Url = "https://8642.ru/news/8642", Posted = true };
    Link link1 = new Link { Site = site0, Url = "https://8642.ru/news/1", Posted = false };
    Link link2 = new Link { Site = site1, Url = "https://som.ru/wtf/1", Posted = false };

    db.Links.AddRange(link0, link1, link2);

    // Параметры
    Param param = new Param { Parameter = "Хуй будешь?", Value = "Буду" };

    db.Params.Add(param);

    // Сохраняем
    db.SaveChanges();
}

class GetLinksException : Exception
{
    public GetLinksException(string message)
        : base(message) {}
}
