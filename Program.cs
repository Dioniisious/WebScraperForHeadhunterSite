using HtmlAgilityPack;
using System.Text.RegularExpressions;

// Вводим название интересующей должности:
Console.WriteLine("Введи название интересующей должности (обязатяльный параметр):");
string? positionTitle = Console.ReadLine();

Console.WriteLine("Введите номер варианта с интересующим опытом работы:");
Console.WriteLine("\t1 - Не имеет значения ");
Console.WriteLine("\t2 - Нет опыта");
Console.WriteLine("\t3 - 1-3 года");
Console.WriteLine("\t4 - 3-6 лет");
Console.WriteLine("\t5 - 6+ лет");
string? gradeLevel = Console.ReadLine();

Console.WriteLine("Введи интересующий уровень ЗП:");
Console.WriteLine("Не пишите ничего, если не имеет значения");
string? salaryValue = Console.ReadLine();

// Пустая строка недопустима - иначе как понять, какую должность анализировать:
// Про опыт добавил проверку, чтобы компилятор не истерил
if (string.IsNullOrEmpty(positionTitle) || string.IsNullOrEmpty(gradeLevel))
{
    throw new Exception("Пустой ввод не обрабатывается: напишите пожалуйста название интересующей должности");
}

// Переводим строку из ввода в url-кодированную строку:
string convertedToUriCode = Uri.EscapeDataString(positionTitle);

// Формируем url-ку в зависимости от выбранных параметров:
string pageUrl;
if (gradeLevel == "1" && string.IsNullOrEmpty(salaryValue))
{
    pageUrl = $@"https://hh.ru/search/vacancy?text={convertedToUriCode}&area=1&hhtmFrom=main&hhtmFromLabel=vacancy_search_line";
}
else 
{
    string experienceData;
    switch(gradeLevel)
    {
        case "3":
            experienceData = "between1And3";
            break;
        case "4":
            experienceData = "between3And6";
            break;
        case "5":
            experienceData = "moreThan6";
            break;
        default:
            experienceData = "noExperience";
            break;
    }
    pageUrl = $@"https://hh.ru/search/vacancy?ored_clusters=true&enable_snippets=true&area=1&hhtmFrom=vacancy_search_list&hhtmFromLabel=vacancy_search_line&search_field=name&search_field=company_name&search_field=description&text={convertedToUriCode}&salary={salaryValue}&experience={experienceData}";
}

// А здесь создаем объект веб-страницы:
HtmlWeb webPage = new();
var document = webPage.Load(pageUrl);

// Найдем, сколько всего страниц будет по данному запросу:
var numberOfPagesContainer = document.DocumentNode.SelectNodes("//a[@data-qa='pager-page']");
int pagesAmount;

// Если пустое значение текста последней страницы - по дефолту ставим 1, иначе переводим в int контент span'а последнего элемента:
if (numberOfPagesContainer is null)
{
    pagesAmount = 1;
}
else
{
    string numberOfPagesString = numberOfPagesContainer[numberOfPagesContainer.Count - 1].FirstChild.InnerText;
    pagesAmount = Convert.ToInt32(numberOfPagesString);
}

// Строка с результатами:
string? resultContent = default;

// А здесь начнем серфинг по всем страницам через параметр &page:
for (int i = 0; i <= pagesAmount; i++)
{
    // URL с учетом страницы:
    string currentUrl = (i == 0) ? pageUrl : (pageUrl + "&page=" + i);

    // Создаем новую веб-страницу:
    HtmlWeb currentWebPage = new();
    var currentDocument = currentWebPage.Load(currentUrl);

    Console.WriteLine(currentDocument.DocumentNode.OuterHtml);

    // Все ссылки с содержимым вакансий:
    // var currentDocumentContent = currentDocument.DocumentNode.SelectNodes("//a[@class='bloko-link']");
    var currentDocumentContent = currentDocument.DocumentNode.SelectNodes("//span[@class='serp-item__title-link-wrapper']/a");



    // Записываем итеративно все элементы массива:
    foreach (var element in currentDocumentContent)
    {
        /*
        // Ссылки разные и среди них лишние есть, не о вакансиях. Спасла от них регулярка:
        Regex pattern = new Regex(@"https://hh.ru/vacancy/[0-9]{8}\?query=(\D)");
        var href = element.Attributes["href"].Value;
        if (pattern.IsMatch(element.OuterHtml))
        {
            resultContent += href;
            resultContent += "\n";
        }
        */
        Console.WriteLine(element.OuterHtml);
    }
}

// Создаем произвольный идентификатор файлу:
Random fileId = new();
string fileName = fileId.Next().ToString();

// Это сделаю потом, но важно сделать папку, которая сама определит, где находится решение.
// И уже в нем сделает путь с файлом: executionResults\Result_number_{fileName}.txt
// Пы сы - а что если попробовать сделать the same для файла формата xml?
// СТОООП... Более умное решение - сделать путь относительно решения, и уже по нему сохранять инфу.

// string resultFilePath = $@"\work_hard_live_better\C#\WebScraperForHeadhunterSite\executionResults\Result_number_{fileName}.txt";
string resultFilePath = $@"\IT_analytics\C#\WebScraperForHeadhunterSite_16MarchRawVersion\executionResults\Result_number_{fileName}.txt";

using (StreamWriter answer = File.CreateText(resultFilePath))
{
    answer.WriteLine(resultContent);
}

/*
// Здесь уже идет логика итерации по каждой из вакансий:
string[] allFoundedUrls = resultContent.Split("\n");

foreach (string urlUnit in allFoundedUrls)
{
    // Загружаем новую страницу - страницу конкретной вакансии
    document = webPage.Load(urlUnit);
    // И здесь уже что-нибудь интересное натворим
}
*/

// Div класса "g-user-content"
// Ух... А как нам придумать, чтобы переписывались блоки "Обязанности", "Требования"?
// Е-мое, а ведь иногда эти блоки называются по-другому! Весело, весело будет...
// Если что, записываем их в разные файлы. ID файлов одинаковый, просто разные сущности: ID_requirements и ID_responsibilities



/*
Важный нюанс - уже сейчас столкнулся с интересным вопросом производительности.
Опытом проверено - для 27 страниц с вакансиями процесс сбора URL-ок для каждой из вакансий занял полминуты, не хило так!!!
*/