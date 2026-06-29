namespace CarDamageClaims.Api.Localization;

using CarDamageClaims.Api.Models;

public static class ExportLocalization
{
    public static string DocTitle(AppLanguage lang) =>
        lang == AppLanguage.En ? "Damage Report" : "Отчет о повреждениях";

    public static string ClientInfo(AppLanguage lang) =>
        lang == AppLanguage.En ? "Client" : "Клиент";

    public static string CarInfo(AppLanguage lang) =>
        lang == AppLanguage.En ? "Vehicle" : "Автомобиль";

    public static string EstimatedCost(AppLanguage lang) =>
        lang == AppLanguage.En ? "Estimated Cost" : "Стоимость ремонта";

    public static string AdminComment(AppLanguage lang) =>
        lang == AppLanguage.En ? "Admin Comment" : "Комментарий администратора";

    public static string AiSummary(AppLanguage lang) =>
        lang == AppLanguage.En ? "AI Summary" : "Сводка AI";

    public static string DamageItems(AppLanguage lang) =>
        lang == AppLanguage.En ? "Damage Items" : "Поврежденные элементы";

    public static string Status(AppLanguage lang) => lang == AppLanguage.En ? "Status" : "Статус";

    public static string Photos(AppLanguage lang) =>
        lang == AppLanguage.En ? "Photos" : "Фотографии";

    public static string Name(AppLanguage lang) => lang == AppLanguage.En ? "Name" : "ФИО";

    public static string Email(AppLanguage lang) => "Email";

    public static string Phone(AppLanguage lang) => lang == AppLanguage.En ? "Phone" : "Телефон";

    public static string CreatedAt(AppLanguage lang) =>
        lang == AppLanguage.En ? "Created At" : "Создано";

    public static string Brand(AppLanguage lang) => lang == AppLanguage.En ? "Brand" : "Марка";

    public static string Model(AppLanguage lang) => lang == AppLanguage.En ? "Model" : "Модель";

    public static string Year(AppLanguage lang) => lang == AppLanguage.En ? "Year" : "Год";

    public static string NoComment(AppLanguage lang) =>
        lang == AppLanguage.En ? "No comment" : "Без комментария";

    public static string RubLabel(AppLanguage lang) => lang == AppLanguage.En ? "RUB" : "руб.";

    public static string DamagePart(AppLanguage lang) => lang == AppLanguage.En ? "Part" : "Деталь";

    public static string DamageDescription(AppLanguage lang) =>
        lang == AppLanguage.En ? "Description" : "Описание";

    public static string DamageSeverity(AppLanguage lang) =>
        lang == AppLanguage.En ? "Severity" : "Серьезность";

    public static string DamageCost(AppLanguage lang) =>
        lang == AppLanguage.En ? "Estimated cost" : "Оценка стоимости";

    public static string NoDamageItems(AppLanguage lang) =>
        lang == AppLanguage.En ? "No damage items" : "Нет повреждений";

    public static string ExcelSheetName(AppLanguage lang) =>
        lang == AppLanguage.En ? "Requests" : "Заявки";

    public static string[] ExcelHeaders(AppLanguage lang)
    {
        return lang == AppLanguage.En
            ? new[]
            {
                "ID",
                "Created at",
                "Status",
                "Full Name",
                "Email",
                "Phone",
                "Car",
                "AI Cost",
                "Admin Name",
            }
            : new[]
            {
                "Номер заявки",
                "Создано",
                "Статус",
                "ФИО",
                "Email",
                "Телефон",
                "Автомобиль",
                "Стоимость AI",
                "Администратор",
            };
    }

    public static string StatusValue(DamageRequestStatus status, AppLanguage lang)
    {
        return status switch
        {
            DamageRequestStatus.New => lang == AppLanguage.En ? "New" : "Новый",
            DamageRequestStatus.AiProcessed =>
                lang == AppLanguage.En ? "AI processed" : "Обработано AI",
            DamageRequestStatus.Approved => lang == AppLanguage.En ? "Approved" : "Одобрено",
            DamageRequestStatus.Rejected => lang == AppLanguage.En ? "Rejected" : "Отклонено",
            DamageRequestStatus.Notified => lang == AppLanguage.En ? "Notified" : "Уведомлен",
            _ => status.ToString(),
        };
    }

    public static string SeverityValue(string severity, AppLanguage lang)
    {
        var normalized = severity.Trim().ToLowerInvariant();

        return normalized switch
        {
            "low" => lang == AppLanguage.En ? "low" : "низкая",
            "medium" => lang == AppLanguage.En ? "medium" : "средняя",
            "high" => lang == AppLanguage.En ? "high" : "высокая",
            _ => severity,
        };
    }
}
