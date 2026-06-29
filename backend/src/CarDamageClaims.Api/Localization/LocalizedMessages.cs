namespace CarDamageClaims.Api.Localization;

public static class LocalizedMessages
{
    public static string EmailAndPasswordRequired(AppLanguage lang) =>
        lang == AppLanguage.En ? "Email and password are required." : "Email и пароль обязательны.";

    public static string InvalidEmailOrPassword(AppLanguage lang) =>
        lang == AppLanguage.En ? "Invalid email or password." : "Неверный email или пароль.";

    public static string UserInactive(AppLanguage lang) =>
        lang == AppLanguage.En
            ? "User account is inactive."
            : "Учетная запись пользователя неактивна.";

    public static string JwtInvalid(AppLanguage lang) =>
        lang == AppLanguage.En ? "JWT configuration is invalid." : "Конфигурация JWT некорректна.";

    public static string AtLeastOneImage(AppLanguage lang) =>
        lang == AppLanguage.En
            ? "At least one image is required."
            : "Требуется минимум одно изображение.";

    public static string MaxThreeFiles(AppLanguage lang) =>
        lang == AppLanguage.En
            ? "You can upload up to 3 files."
            : "Можно загрузить не более 3 файлов.";

    public static string InvalidImageTypes(AppLanguage lang) =>
        lang == AppLanguage.En
            ? "Only JPEG, PNG and WEBP files are allowed. HEIC/HEIF must be converted to JPEG before upload."
            : "Допустимы только файлы JPEG, PNG и WEBP. HEIC/HEIF нужно преобразовать в JPEG перед загрузкой.";

    public static string InvalidImageExtension(AppLanguage lang) =>
        lang == AppLanguage.En
            ? "File extension must be one of: .jpg, .jpeg, .png, .webp."
            : "Расширение файла должно быть одним из: .jpg, .jpeg, .png, .webp.";

    public static string NotCarDetected(AppLanguage lang) =>
        lang == AppLanguage.En
            ? "The uploaded images do not contain a recognizable car. Please upload vehicle photos."
            : "На загруженных изображениях не распознан автомобиль. Пожалуйста, загрузите фото автомобиля.";

    public static string AiTemporarilyUnavailable(AppLanguage lang) =>
        lang == AppLanguage.En
            ? "AI analysis is temporarily unavailable. Please try again in a minute."
            : "AI-анализ временно недоступен. Пожалуйста, повторите попытку через минуту.";

    public static string AiInvalidResponse(AppLanguage lang) =>
        lang == AppLanguage.En
            ? "AI returned an invalid response format. Please try again."
            : "AI вернул некорректный формат ответа. Пожалуйста, повторите попытку.";

    public static string AiPricingUnavailable(AppLanguage lang) =>
        lang == AppLanguage.En
            ? "AI could not determine real part prices right now. Please try again shortly."
            : "AI не смог определить актуальные цены запчастей. Пожалуйста, повторите попытку немного позже.";

    public static string UserExists(AppLanguage lang) =>
        lang == AppLanguage.En
            ? "User with this email already exists."
            : "Пользователь с таким email уже существует.";

    public static string RoleInvalid(AppLanguage lang) =>
        lang == AppLanguage.En
            ? "Role must be Admin or Manager."
            : "Роль должна быть Admin или Manager.";

    public static string UserNotFound(AppLanguage lang) =>
        lang == AppLanguage.En ? "User not found." : "Пользователь не найден.";

    public static string DamageRequestNotFound(AppLanguage lang) =>
        lang == AppLanguage.En ? "Damage request not found." : "Заявка не найдена.";

    public static string OnlyAiProcessedApprove(AppLanguage lang) =>
        lang == AppLanguage.En
            ? "Only AI-processed requests can be approved."
            : "Одобрять можно только заявки со статусом AI-обработки.";

    public static string OnlyAiProcessedReject(AppLanguage lang) =>
        lang == AppLanguage.En
            ? "Only AI-processed requests can be rejected."
            : "Отклонять можно только заявки со статусом AI-обработки.";

    public static string ApproverMissing(AppLanguage lang) =>
        lang == AppLanguage.En ? "Unauthorized" : "Не авторизован";

    public static string NoValidImagesForReanalysis(AppLanguage lang) =>
        lang == AppLanguage.En
            ? "No valid images found for reanalysis."
            : "Для повторного анализа не найдены валидные изображения.";

    public static string StatusInvalid(AppLanguage lang) =>
        lang == AppLanguage.En ? "Status value is invalid." : "Некорректное значение статуса.";

    public static string DocxIntegrityFailed(AppLanguage lang) =>
        lang == AppLanguage.En
            ? "Failed to generate valid DOCX export."
            : "Не удалось сформировать корректный DOCX файл.";

    public static string Unauthorized(AppLanguage lang) =>
        lang == AppLanguage.En ? "Unauthorized" : "Не авторизован";
}
