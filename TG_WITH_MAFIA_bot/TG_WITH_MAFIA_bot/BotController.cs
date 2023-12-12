using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TG_WITH_MAFIA_bot
{
    public class BotController
    {
        private string botToken = "6984398334:AAFamXOnYYfZ9dkxFw0D6Zu1aqdq0QW23Ro";

        public void StartBot()
        {
            Console.WriteLine("Start working");
            var botClient = new TelegramBotClient(botToken);
            botClient.StartReceiving(Update, Error);
        }

        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;

                if (message.Text != null)
                {
                    if (message.Text == "/start")
                    {
                        await SendMainMenu(botClient, message.Chat.Id);
                    }
                }
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;
                var chatId = callbackQuery.Message.Chat.Id;

                if (callbackQuery.Data == "/BTN_CREATE")
                {
                    await botClient.SendTextMessageAsync(chatId, $"Вы выбрали 'Создать комнату'");
                }
                else if (callbackQuery.Data == "/BTN_CONNECT")
                {
                    await botClient.SendTextMessageAsync(chatId, $"Вы выбрали 'Присоединиться'");
                }
            }
        }

        private static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }

        private static async Task SendMainMenu(ITelegramBotClient botClient, long chatId)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Создать комнату", "/BTN_CREATE"),
                    InlineKeyboardButton.WithCallbackData("Присоединиться", "/BTN_CONNECT")
                }
            });

            await botClient.SendTextMessageAsync(chatId, "Выберите действие:", replyMarkup: keyboard);
        }
    }
}