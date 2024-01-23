using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TgWithMafia
{
    internal class BotController
    {
        TelegramBotClient botClient;
        public BotController()
        {
            botClient = new TelegramBotClient(AppConfigController.ConnectionSettings.BotToken);
            new BotCommandsHandler(botClient);
            botClient.StartReceiving(Update, Error);
        }

        private async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;

            var chatId = message?.Chat.Id ?? callbackQuery?.Message.Chat.Id ?? 0;

            if (message != null && message.Text.StartsWith("/"))
            {
                await BotCommandsHandler.Instance.HandleCommands(message);
            }

            if (callbackQuery != null)
            {
                await BotCallBackQuery.Instance.HandleCallbackQuery(callbackQuery);
            }
        }
        private Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            MessageBox.Show("Произошла ошибка: " + arg2.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return Task.CompletedTask;
        }
    }
}
