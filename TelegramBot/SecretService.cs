using System.Collections;
using System.Text;

namespace TelegramBot
{
    public static class SecretService
    {
        public const string PathApiTelegram = @"C:\temp\TelegaApi";
        public const string PathApiMail = @"C:\temp\ApiMail";

        public static void InitEnviroment()
        {
            string path = File.ReadAllText(PathApiTelegram);
            Environment.SetEnvironmentVariable("TELEGRAM_BOT_API_TOKEN", path);

        }


        public static string PrintEnviromentSystem()
        {
            var keyValuePairs = new SortedDictionary<string, string>();
            var sb = new StringBuilder();

            foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
                keyValuePairs.Add((string)item.Key, (string)item.Value);


            foreach (var item in keyValuePairs)
                sb.AppendLine(item.Key.ToString() + " - " + item.Value.ToString() + "\n");


            return sb.ToString();
        }



    }
}
