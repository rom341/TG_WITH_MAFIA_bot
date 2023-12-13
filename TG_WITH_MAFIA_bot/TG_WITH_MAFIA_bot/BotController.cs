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
        private static RoomsController roomsController = new RoomsController();
        private static UsersController usersController = new UsersController();

        public void StartBot()
        {
            Console.WriteLine("Start working");
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
            if (message != null)
            {
                if (message.Text.StartsWith("/"))
                {
                    if (message.Text.ToLower().StartsWith("/start") || message.Text.ToLower().StartsWith("/menu"))
                    {
                        User user = new User(message.Chat.Id, UserStates.INMENU);
                        if (usersController.Contains(user))
                            usersController.ChangeUserState(usersController.GetUserListId(user.ChatId), UserStates.INMENU);
                        else
                            usersController.AddUser(user);

                        await SendMainMenu(botClient, message.Chat.Id);
                    }
                    else if (message.Text?.StartsWith("/connect") == true)
                    {
                        // Extracting the room ID from the command "/connect [ID]"
                        var roomIdText = message.Text.Split(' ')[1];
                        if (long.TryParse(roomIdText, out long roomId))
                        {
                            if (roomsController.GetRoomListId(roomId) != -1)
                            {
                                usersController.ChangeUserState(usersController.GetUserListId(chatId), UserStates.INLOBBY);
                                await HandleConnect(botClient, chatId, roomId);
                            }
                            else
                                await botClient.SendTextMessageAsync(chatId, $"Комнаты с введенным ID не существует");
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(chatId, $"Некорректный формат ID комнаты");
                        }
                    }
                    else if (message.Text.ToLower() == "/room list")
                    {
                        if (usersController.GetUserState(usersController.GetUserListId(chatId)) == UserStates.INLOBBY)
                        {
                            roomsController.GetRoom(roomsController.FindRoomListIdContainsPlayer(new Player(chatId)), out Room resultRoom);
                            await botClient.SendTextMessageAsync(chatId, $"{resultRoom.ToString()}");
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(chatId, $"Вы не состоите в комнате!");
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, $"Неизвестная команда");
                    }
                }
            }

            // Menu controller
            if (callbackQuery != null)
            {
                if (callbackQuery?.Data == "/BTN_CREATE")
                {
                    if (usersController.GetUserState(usersController.GetUserListId(chatId)) == UserStates.INMENU)
                    {
                        usersController.ChangeUserState(usersController.GetUserListId(chatId), UserStates.INLOBBY);
                        await HandleCreateRoom(botClient, chatId);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, $"Вы уже в комнате!");
                    }
                }
                else if (callbackQuery?.Data == "/BTN_CONNECT")
                {
                    await botClient.SendTextMessageAsync(chatId, $"Введите команду '/connect' и добавьте ID комнаты");
                }
            }
        }


        private async static Task HandleCreateRoom(ITelegramBotClient botClient, long chatId)
        {
            Player roomOwner = new Player(chatId);
            Room room = new Room(roomOwner);
            roomsController.CreateNewRoom(room);
            await botClient.SendTextMessageAsync(chatId, $"ID вашей комнаты: {room.Id}");
        }

        private async static Task HandleConnect(ITelegramBotClient botClient, long chatId, long roomId)
        {
            Player newPlayer = new Player(chatId);
            if (roomsController.AddPlayerTo(roomsController.GetRoomListId(roomId), newPlayer))
            {
                await botClient.SendTextMessageAsync(chatId, $"Подключено успешно");
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, $"Комнаты с введенным ID не существует");
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