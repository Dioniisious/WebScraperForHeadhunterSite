using HtmlAgilityPack;
using System.Text.RegularExpressions;
string pageUrl = @"https://hh.ru/search/vacancy?text=c%23&area=1&hhtmFrom=main&hhtmFromLabel=vacancy_search_line";
// string pageUrl = @"https://hh.ru/search/vacancy?text=python+разработчик&salary=&ored_clusters=true&enable_snippets=true&area=1&hhtmFrom=vacancy_search_list&hhtmFromLabel=vacancy_search_line";
HtmlWeb webPage = new();

// Получаею объект HtmlAgilityPack.HtmlDocument
var document = webPage.Load(pageUrl);

// Здесь извлекаю содержимое html-файла (вся веб-страница):
// var documentContent = document.DocumentNode.OuterHtml;

// Извлеку все данные из интересующего меня блока div:
// var documentContent = document.DocumentNode.SelectSingleNode("//div[@id='a11y-main-content']").OuterHtml;

// Вытащим все URL-ссылки с помощью тега А, имеющего ссылку на вакансию:
var documentContent = document.DocumentNode.SelectNodes("//a[@class='bloko-link']");

string? finalContent = default;

// В результате цикла ниже получаем html-разметку блока 1й вакансии:
foreach (var element in documentContent)
{
    Regex pattern = new Regex(@"https://hh.ru/vacancy/[0-9]{8}\?query=(\D)");
    var href = element.Attributes["href"].Value;
    if (pattern.IsMatch(element.OuterHtml))
    {
        finalContent += href;
        finalContent += "\n";
    }
    // finalContent += element.OuterHtml;
    // finalContent += href;
    // finalContent += "\n";
}



/*
Пояснялка:
Для основной страницы списки ваканский расположены в id класса "a11y-main-content"
Внутри этого div'а в каждом из следующих элементов div класса "serp-item serp-item_link serp-item-redesign" представлена инфа о вакансии
Далее - в каждой из вакансий в блоке div класса "g-user-content" расположена инфа, причем в тегах P и UL.

Думаю, нам не нужна html-разметка блоков div вакансий. Нужно чтобы произошел клик по этому блоку.
Тогда произойдет переход на страницу вакансии, а оттуда уже извлекаем теги P и UL.

Но вопрос скорее в другом: ниже url-адреса ленты и конкретной страницы про "C# разработчик" соответственно:
1) https://hh.ru/search/vacancy?text=c%23+разработчик&salary=&ored_clusters=true&experience=between1And3&enable_snippets=true&area=1&hhtmFrom=vacancy_search_list&hhtmFromLabel=vacancy_search_line
2) https://hh.ru/vacancy/92825871?query=c%23+разработчик&hhtmFrom=vacancy_search_list

Теперь же про "python разработчик" соответственно:
1) https://hh.ru/search/vacancy?text=python+разработчик&salary=&ored_clusters=true&experience=between1And3&enable_snippets=true&area=1&hhtmFrom=vacancy_search_list&hhtmFromLabel=vacancy_search_line
2) https://hh.ru/vacancy/88580480?query=python+разработчик&hhtmFrom=vacancy_search_list

Найдем закономерности: text=c%23+разработчик или text=python+разработчик
Получаем шаблон для каждой профессии с уникальным JOB_ID:
$"https://hh.ru/vacancy/88580480?query={JOB_ID}&hhtmFrom=vacancy_search_list"

Получаем шаблон для каждой вакансии с уникальным VACANCY_ID:
$"https://hh.ru/vacancy/{VACANCY_ID}?query=c%23+разработчик&hhtmFrom=vacancy_search_list"

Тогда остается просто найти все эти VACANCY_ID по искомым параметрам и открывать все страницы и уже потом качать список компетенций
Как их найти - все просто. Нужно в теге span (название позиции вакансии) найти тег А, у которых class="bloko-link"
А вообще, зачем, когда у нас уже есть список ссылок? Фильтруем нужные нам и по очереди их открываем.

Так, предлагаю сначала через регулярки разделить 2 разнящиеся ссылки (одна из них нам нужна, другая - нет):
1) https://hh.ru/vacancy/94133163?query=c%23&amp;hhtmFrom=vacancy_search_list
2) https://feedback.hh.ru/article/details/id/5951

Значит, нужен шаблон:
    @"https://hh.ru/vacancy/[0-9]{8}?query=(\D);hhtmFrom=vacancy_search_list"
Но здесь не понятно как быть с сиволом ; т.е, он усложняет, но From после него - это уже не URL.
Какая разница, с какого элемента был выполнен перехлд по данному URL? Нам это не нужно.

А на деле сработает такой шаблон:
    @"https://hh.ru/vacancy/[0-9]{8}\?query=(\D)"


А еще снизу после листов с вакансиями есть предложение "Попробуйте другие варианты поискового запроса"
Как вариант, можно устроить серфинг по смежным запросам. Но уже не так интенсивно, иначе этот парсер точно забанят к чертям)

*/



// Для каждого нового результата буду создавать отдельный текстовый файл:
Random fileId = new();
string fileName = fileId.Next().ToString();

// Открываю файл и записываю в него данные, для теста запишу идентификатор:
using (StreamWriter answer = File.CreateText($@"\work_hard_live_better\C#\WebScraperForHeadhunterSite\executionResults\Result_number_{fileName}.txt"))
{
    // answer.WriteLine(documentContent);
    answer.WriteLine(finalContent);
}



// А здесь уже созданный файл будем читать...
// Стоп! А что если не читать, а просто извлекать конкретные ссылки, которые нам нужны?
// И уже через них будем читать URL-ки

/*
У каждой из вакансий есть огромное количество страниц. На то робот и нужен - прошерстеть все, что только есть!
Ниже в URL-ках мы можем увидеть, что у нас имеется небольшое отличие в последнем параметре - page=0, page=1 и т.д.
    1) https://hh.ru/search/vacancy?area=1&search_field=name&search_field=company_name&search_field=description&enable_snippets=true&text=c%23+%D1%80%D0%B0%D0%B7%D1%80%D0%B0%D0%B1%D0%BE%D1%82%D1%87%D0%B8%D0%BA&page=0
    2) https://hh.ru/search/vacancy?area=1&search_field=name&search_field=company_name&search_field=description&enable_snippets=true&text=c%23+%D1%80%D0%B0%D0%B7%D1%80%D0%B0%D0%B1%D0%BE%D1%82%D1%87%D0%B8%D0%BA&page=1
В таком случае нам стоит запустить цикл, в котором счетчик страниц каждый раз будет расти: i++
И как только страницы кончатся - break (у мня просто нет пока идей, как сделать так, чтобы не доводить до ошибки 404)

Технически это реализуется несложно: начиная со 2й страницы, добавляем к URL: $@"{pageUrl}&page={pageNumber}"


Blocked: после решения текущих вопросов... надо придумать, как учесть параметры (всякие опыт, оплата труда и т.п. - тоже надо будет подумать).
Blocked: после решения текущих вопросов... уже поработаем с каждой из страниц вакансий по отдельности, было бы интересно почитать контенты "Требования" и "Обязанности".
"Условия" мне пока не так интересны.
*/