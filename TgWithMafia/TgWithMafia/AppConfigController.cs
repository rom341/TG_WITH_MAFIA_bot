using Newtonsoft.Json;
using System.IO;

namespace TgWithMafia
{
    internal class AppConfigController
    {
        private static AppConfigController instance;
        public static Config ConnectionSettings { get; private set; }

        // Приватный конструктор, чтобы запретить создание экземпляра через new
        public AppConfigController()
        {
            // Вызываем метод для чтения данных из файла
            ConnectionSettings = ReadConnectionSettingsFromFile();
        }

        public static AppConfigController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AppConfigController();
                }
                return instance;
            }
        }

        private Config ReadConnectionSettingsFromFile(string filePath = "appsettings.json")
        {
            try
            {
                // Пытаемся прочитать JSON из файла
                string json = File.ReadAllText(filePath);

                // Десериализуем JSON в объект ConnectionSettings
                Config connectionSettings = JsonConvert.DeserializeObject<Config>(json);

                // Возвращаем объект ConnectionSettings
                return connectionSettings ?? new Config();
            }
            catch (FileNotFoundException)
            {
                // Можно вывести сообщение или сделать что-то еще по вашему усмотрению
                return new Config { MySqlConnection = "Файл конфигурации не найден." };
            }
            catch (JsonException)
            {
                // Обработка исключения, если возникают проблемы с десериализацией JSON
                return new Config { MySqlConnection = "Ошибка при чтении данных из файла конфигурации." };
            }
        }
    }

    public class Config
    {
        public string MySqlConnection { get; set; }
        public string BotToken { get; set; }
    }
}
