using System.Text;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Это для кодировки windows-1251

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

            // Получаем все теги <a>
            var linkElements = document.QuerySelectorAll("a");

            if (linkElements.Length > 0)
            {
                foreach (var linkElement in linkElements)
                {
                    // Получаем аттрибуты href у каждой ссылки
                    string? hrefValue = linkElement.GetAttribute("href");
                    Console.WriteLine(hrefValue);
                }
            }
            else
            {
                Console.WriteLine("Ссылки не найдены.");
            }
        }
        else
        {
            Console.WriteLine($"Ошибка: {response.StatusCode} - {response.ReasonPhrase}");
        }
    }
}
catch (HttpRequestException e)
{
    Console.WriteLine($"Ошибка при выполнении запроса: {e.Message}");
}
