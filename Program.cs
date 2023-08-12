using System.Text;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Configuration;

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

void admin()
{
    void editSites()
    {
        string getSiteName()
        {
            using UpdateCheckContext db = new UpdateCheckContext();

            string? siteName = null;
            while (siteName == null)
            {
                Console.Write("Введите название сайта: ");
                siteName = Console.ReadLine();

                siteName = siteName.Trim();

                if (string.IsNullOrEmpty(siteName))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("В названии должно быть хоть что-то помимо пробелов.");
                    Console.ResetColor();

                    siteName = null;
                }
                else if (db.Sites.Any(site => site.Name == siteName))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Название должно быть уникальным.");
                    Console.ResetColor();
                    
                    siteName = null;
                }

                Console.WriteLine();
            }

            return siteName;
        }

        string getSiteLink()
        {
            string? siteLink = null;
            while (siteLink == null)
            {
                Console.Write("Введите ссылку, которую нужно проверять: ");
                siteLink = Console.ReadLine();

                siteLink = siteLink.Trim();

                Uri uri;
                if (!(Uri.TryCreate(siteLink, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Это не похоже на ссылку.");
                    Console.ResetColor();

                    siteLink = null;
                }

                Console.WriteLine();
            }

            return siteLink;
        }

        void addSite()
        {
            using UpdateCheckContext db = new UpdateCheckContext();

            string siteName = getSiteName();
            string siteLink = getSiteLink();

            Site site = new Site { Name = siteName, Url = siteLink };
            db.Add(site);
            db.SaveChanges();

            Console.WriteLine("Был создан сайт со следующими параметрами");
            Console.WriteLine($"Название: {site.Name}");
            Console.WriteLine($"Ссылка: {site.Url}");
        }

        void editSite(int siteId)
        {
            void editSiteName(int siteId)
            {
                using UpdateCheckContext db = new UpdateCheckContext();

                Site site = db.Sites.Single(site => site.Id == siteId);

                string siteName = getSiteName();

                site.Name = siteName;
                db.SaveChanges();

                Console.WriteLine($"Новое название сайта: {site.Name}");
            }

            void editSiteLink(int siteId)
            {
                using UpdateCheckContext db = new UpdateCheckContext();

                Site site = db.Sites.Single(site => site.Id == siteId);

                string siteLink = getSiteLink();

                site.Url = siteLink;
                db.SaveChanges();

                Console.WriteLine($"Новая ссылка сайта: {site.Url}");
            }

            void deleteSite(int siteId)
            {
                int? choice = null;
                while (choice == null || choice < 1 || choice > 2)
                {
                    Console.WriteLine("Точно удалить?");
                    Console.WriteLine("1. Да");
                    Console.WriteLine("2. Нет");

                    Console.Write("?> ");
                    string input = Console.ReadLine();

                    if (int.TryParse(input, out int parsedChoice))
                    {
                        choice = parsedChoice;
                    }

                    Console.WriteLine();
                }

                if (choice == 1)
                {
                    using UpdateCheckContext db = new UpdateCheckContext();

                    Site site = db.Sites.Single(site => site.Id == siteId);

                    db.Sites.Remove(site);
                    db.SaveChanges();

                    Console.WriteLine($"Сайт {site.Name} удалён!");
                }
            }

            using UpdateCheckContext db = new UpdateCheckContext();

            Site site = db.Sites.Single(site => site.Id == siteId);

            Console.WriteLine("Текущие параметры сайта");
            Console.WriteLine($"Название: {site.Name}");
            Console.WriteLine($"Ссылка: {site.Url}");
            Console.WriteLine();

            int? choice = null;
            while (choice == null || choice < 1 || choice > 3)
            {
                Console.WriteLine("Что изменить?");
                Console.WriteLine("1. Название");
                Console.WriteLine("2. Ссылку");
                Console.Write("3. ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Удалить");
                Console.ResetColor();

                Console.Write("?> ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int parsedChoice))
                {
                    choice = parsedChoice;
                }

                Console.WriteLine();
            }

            switch (choice)
            {
                case 1:
                    editSiteName(site.Id);
                    break;
                case 2:
                    editSiteLink(site.Id);
                    break;
                case 3:
                    deleteSite(site.Id);
                    break;
            }
        }

        using UpdateCheckContext db = new UpdateCheckContext();
        var sites = db.Sites.ToList();

        int choice = -1;
        while (choice < 0 || choice > sites.Count)
        {
            Console.WriteLine("Какой сайт изменить?");
            Console.WriteLine("0. Создать новый");
            for (int index = 0; index < sites.Count; index++)
            {
                Console.WriteLine($"{index + 1}. Изменить {sites[index].Name}");
            }
            Console.Write("?> ");
            string input = Console.ReadLine();

            if (int.TryParse(input, out int parsedChoice))
            {
                choice = parsedChoice;
            }

            Console.WriteLine();
        }

        if (choice == 0)
        {
            addSite();
        }
        else
        {
            if (choice != null) editSite(sites[choice - 1].Id);
        }
    }

    void editParameters()
    {
        
    }

    int? choice = null;
    while (choice == null || choice < 1 || choice > 2)
    {
        Console.WriteLine("Что вы ходите сделать?");
        Console.WriteLine("1. Редактирование сайтов");
        Console.WriteLine("2. Редактирование параметров бота");

        Console.Write("?> ");
        string input = Console.ReadLine();

        if (int.TryParse(input, out int parsedChoice))
        {
            choice = parsedChoice;
        }

        Console.WriteLine();
    }

    switch (choice)
    {
        case 1:
            editSites();
            break;
        case 2:
            editParameters();
            break;
    }
}

if (args.Length > 0)
{
    switch (args[0])
    {
        case "edit":
            admin();
            break;
        default:
            Console.WriteLine("Я вас не понял. help и --help тоже нет.");
            break;
    }
    Environment.Exit(0);
}

using UpdateCheckContext db = new UpdateCheckContext();

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

class GetLinksException : Exception
{
    public GetLinksException(string message)
        : base(message) {}
}
