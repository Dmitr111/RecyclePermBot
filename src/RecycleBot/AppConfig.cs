using DotNetEnv;

namespace RecycleBot
{
    // Настройки приложения: берутся из переменных окружения, которые можно задать в файле .env
    public class AppConfig
    {
        public const string TelegramBotTokenVar = "TELEGRAM_BOT_TOKEN";
        public const string YandexStaticApiKeyVar = "YANDEX_STATIC_API_KEY";
        public const string YandexSearchApiKeyVar = "YANDEX_SEARCH_API_KEY";
        public const string AdminChatIdVar = "ADMIN_CHAT_ID";

        // Токен бота у @BotFather
        public string TelegramBotToken { get; }

        // Ключ Yandex Static API
        public string YandexStaticApiKey { get; }

        // Ключ API поиска по организациям Яндекса
        public string YandexSearchApiKey { get; }

        // Chat ID администратора, которому доступна рассылка (необязательно)
        public long? AdminChatId { get; }

        private AppConfig(string telegramBotToken, string yandexStaticApiKey, string yandexSearchApiKey, long? adminChatId)
        {
            TelegramBotToken = telegramBotToken;
            YandexStaticApiKey = yandexStaticApiKey;
            YandexSearchApiKey = yandexSearchApiKey;
            AdminChatId = adminChatId;
        }

        // Загрузка настроек. Если обязательное значение не задано, бросает InvalidOperationException
        public static AppConfig Load()
        {
            // Ищем .env в текущей папке и выше; уже заданные переменные окружения не перезаписываются
            Env.NoClobber().TraversePath().Load();

            var errors = new List<string>();

            string token = GetRequired(TelegramBotTokenVar, errors);
            string staticKey = GetRequired(YandexStaticApiKeyVar, errors);
            string searchKey = GetRequired(YandexSearchApiKeyVar, errors);

            long? adminChatId = null;
            string? adminChatIdValue = Environment.GetEnvironmentVariable(AdminChatIdVar);
            if (!string.IsNullOrWhiteSpace(adminChatIdValue))
            {
                if (long.TryParse(adminChatIdValue.Trim(), out long parsed))
                {
                    adminChatId = parsed;
                }
                else
                {
                    errors.Add($"{AdminChatIdVar} должен быть числом (Chat ID), сейчас: \"{adminChatIdValue}\"");
                }
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Ошибка конфигурации:\n  - " + string.Join("\n  - ", errors) +
                    "\nЗадайте переменные окружения или создайте файл .env по образцу .env.example.");
            }

            return new AppConfig(token, staticKey, searchKey, adminChatId);
        }

        private static string GetRequired(string name, List<string> errors)
        {
            string? value = Environment.GetEnvironmentVariable(name);

            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"не задана переменная {name}");
                return "";
            }

            return value.Trim();
        }
    }
}
