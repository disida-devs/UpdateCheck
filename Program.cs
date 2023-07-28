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

var links = await getLinksAsync("http://www.ettu.ru/news/");

links.ForEach(Console.WriteLine);

class GetLinksException : Exception
{
    public GetLinksException(string message)
        : base(message) {}
}
