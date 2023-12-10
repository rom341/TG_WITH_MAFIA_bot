using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace TG_WITH_MAFIA_bot
{
    public class BotController
    {
        private string botToken = "6984398334:AAFamXOnYYfZ9dkxFw0D6Zu1aqdq0QW23Ro";

        public void StartBot()
        {
            Console.WriteLine("Start working");
            var client = new TelegramBotClient(botToken);
            client.StartReceiving(Update, Error);
        }

        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;
            if (message.Text != null)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Bot is working!");               
            }
        }
        private static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }
    }
}
