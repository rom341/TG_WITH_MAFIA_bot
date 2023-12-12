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
    enum BotStates
    {
        DEFAULT,
        WAITING_CONNECTION
    }
    public class BotController
    {
        private string botToken = "6984398334:AAFamXOnYYfZ9dkxFw0D6Zu1aqdq0QW23Ro";
        private static RoomsController roomsController = new RoomsController();
        private static BotStates currentBotState;

        public void StartBot()
        {
            Console.WriteLine("Start working");
            currentBotState = BotStates.DEFAULT;
            var botClient = new TelegramBotClient(botToken);
            botClient.StartReceiving(Update, Error);
        }

        async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;

            // Get a chatId by any cost
            var chatId = message?.Chat.Id ?? callbackQuery?.Message.Chat.Id ?? 0;

            // Command controller
            if (message != null && currentBotState != BotStates.WAITING_CONNECTION)
            {
                if (message.Text[0] == '/')
                {
                    if (message.Text == "/start")
                    {
                        await SendMainMenu(botClient, message.Chat.Id);
                    }
                    else if (message.Text?.Contains("/connect") == true)
                    {
                        currentBotState = BotStates.WAITING_CONNECTION;
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, $"Неизвестная команда");
                    }
                }
            }

            // Menu controller
            if (callbackQuery != null || currentBotState == BotStates.WAITING_CONNECTION)
            {
                if (callbackQuery?.Data == "/BTN_CREATE" && currentBotState != BotStates.WAITING_CONNECTION)
                {
                    await HandleCreateRoom(botClient, chatId);
                }
                else if (callbackQuery?.Data == "/BTN_CONNECT" || currentBotState == BotStates.WAITING_CONNECTION)
                {
                    if (message?.Text != null && message.Text.StartsWith("/connect"))
                    {
                        // Extracting the room ID from the command "/connect [ID]"
                        var roomIdText = message.Text.Split(' ')[1];
                        if (long.TryParse(roomIdText, out long roomId))
                        {
                            await HandleConnect(botClient, chatId, roomId);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(chatId, $"Некорректный формат ID комнаты");
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, $"Неизвестная команда");
                    }
                }
            }
        }


        private async static Task HandleCreateRoom(ITelegramBotClient botClient, long chatId)
        {
            await botClient.SendTextMessageAsync(chatId, $"Вы выбрали 'Создать комнату'");
            Player roomOwner = new Player(chatId);
            Room room = new Room(roomOwner);
            roomsController.CreateNewRoom(room);
            await botClient.SendTextMessageAsync(chatId, $"ID вашей комнаты: {room.Id}");
        }

        private async static Task HandleConnect(ITelegramBotClient botClient, long chatId, long roomId)
        {
            await botClient.SendTextMessageAsync(chatId, $"Вы выбрали 'Присоединиться'");
            if (currentBotState == BotStates.DEFAULT)
            {
                await botClient.SendTextMessageAsync(chatId, $"Введите команду '/connect ' и введите ID комнаты");
                return;
            }

            Player newPlayer = new Player(chatId);
            if (roomsController.AddPlayerTo(roomsController.GetRoomById(roomId), newPlayer))
            {
                await botClient.SendTextMessageAsync(chatId, $"Подключено успешно");
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, $"Комнаты с введенным ID не существует");
            }
            currentBotState = BotStates.DEFAULT;
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