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
        private static string botToken = "6984398334:AAFamXOnYYfZ9dkxFw0D6Zu1aqdq0QW23Ro";
        private static RoomsController roomsController;
        private static UsersController usersController = new UsersController();
        private static GameController gameController;
        private TelegramBotClient botClient = new TelegramBotClient(botToken);

        public void StartBot()
        {
            Console.WriteLine("Start working");
            botClient.StartReceiving(Update, Error);
            roomsController = new RoomsController(this);
        }

        private async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;

            var chatId = message?.Chat.Id ?? callbackQuery?.Message.Chat.Id ?? 0;

            if (message != null && message.Text.StartsWith("/"))
            {
                await HandleCommands(botClient, message);
            }

            if (callbackQuery != null)
            {
                await HandleCallbackQuery(botClient, callbackQuery);
            }
        }

        private async Task HandleCommands(ITelegramBotClient botClient, Message message)
        {
            var chatId = message.Chat.Id;

            if (message.Text.ToLower().Equals("/start") || message.Text.ToLower().Equals("/menu"))
            {
                HandleStartMenuCommand(chatId);
            }
            else if (message.Text?.StartsWith("/connect") == true)
            {
                await HandleConnectCommand(chatId, message.Text);
            }
            else if (message.Text.ToLower() == "/room list")
            {
                await HandleRoomListCommand(chatId);
            }
            else if(message.Text.ToLower() == "/room start")
            {
                await HendleRoomStartComamnd(chatId);
            }
            else if(message.Text.ToLower() == "/leave")
            {
                await HendleLeaveCommand(new User(chatId));
            }
            else
            {
                await SendMessage(chatId, "Неизвестная команда");
            }
        }

        private async Task HendleRoomStartComamnd(long chatId)
        {
            gamec
        }

        private async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id;

            if (callbackQuery?.Data == "/BTN_CREATE")
            {
                await HandleCreateRoom(chatId);
            }
            else if (callbackQuery?.Data == "/BTN_CONNECT")
            {
                await SendMessage(chatId, "Введите команду '/connect' и добавьте ID комнаты");
            }
        }

        private async void HandleStartMenuCommand(long chatId)
        {
            User user = new User(chatId);

            if (usersController.Contains(user))
            {
                usersController.ChangeUserState(usersController.FindUserIndex(user.ChatID), UserStates.INMENU);
                await HendleLeaveCommand(user);
            }
            else
                usersController.AddUser(user);

            await SendMainMenu(chatId);
        }

        private async Task HandleConnectCommand(long chatId, string text)
        {
            UserStates userState = usersController.GetUserState(usersController.FindUserIndex(chatId));
            if(userState==UserStates.NONE)
            {
                Console.WriteLine($"{chatId} tried to connect by {text}, but get the 'none' userState");
                return;
            }
            
            if (userState == UserStates.INLOBBY || userState == UserStates.INGAME) 
            {
                await SendMessage(chatId, "Вы уже состоите в комнате. Используйте '/room list' чтобы узнать подробнее");
                return;
            }

            var roomIdText = text.Split(' ')[1];
            if (long.TryParse(roomIdText, out long roomId))
            {
                int idOfFoundRoom = roomsController.FindRoomIndex(room => room.Id == roomId);
                if (idOfFoundRoom != -1)
                {
                    usersController.ChangeUserState(usersController.FindUserIndex(chatId), UserStates.INLOBBY);
                    await HandleConnect(chatId, roomId);
                }
                else
                {
                    await SendMessage(chatId, "Комнаты с введенным ID не существует");
                }
            }
            else
            {
                await SendMessage(chatId, "Некорректный формат ID комнаты");
            }
        }

        private async Task HendleLeaveCommand(User user)
        {
            int roomId = roomsController.FindRoomIndex(room => room.Owner.ChatID == user.ChatID);
            //destroy a room, where user was an owner
            if (roomId != -1)
            {
                roomsController.GetRoom(roomId, out Room roomToDestroy);
                roomsController.DestroyRoom(roomToDestroy);
            }
            else
            {
                //leave from room, where user is just a member
                roomId = roomsController.FindRoomIndex(r => r.ContainsUser(u => u.ChatID == user.ChatID));
                if (roomId != -1)
                {
                    roomsController.GetRoom(roomId, out Room room);
                    await room.RemoveUser(user);
                    await SendMessage(user.ChatID, $"Комната покинута");
                }
                //if no room with user
                else
                {
                    return;
                }
            }
        }

        private async Task HandleRoomListCommand(long chatId)
        {
            if (usersController.GetUserState(usersController.FindUserIndex(chatId)) == UserStates.INLOBBY)
            {
                roomsController.GetRoom(roomsController.FindRoomIndexWithUser(new User(chatId)), out Room resultRoom);
                await SendMessage(chatId, $"{resultRoom.ToString()}");
            }
            else
            {
                await SendMessage(chatId, "Вы не состоите в комнате!");
            }
        }

        private async Task HandleCreateRoom(long chatId)
        {
            User roomOwner = new User(chatId);
            Room newRoom = new Room(roomOwner, this);
            roomsController.CreateNewRoom(newRoom);
            await SendMessage(chatId, $"ID вашей комнаты: {newRoom.Id}");
            usersController.ChangeUserState(usersController.FindUserIndex(roomOwner.ChatID), UserStates.INLOBBY);
        }

        private async Task HandleConnect(long chatId, long roomId)
        {
            User newPlayer = new User(chatId);

            if (roomsController.AddPlayerTo(roomsController.FindRoomIndex(room=>room.Id == roomId), newPlayer))
            {
                await SendMessage(chatId, "Подключено успешно");
            }
            else
            {
                await SendMessage(chatId, "Комнаты с введенным ID не существует");
            }
        }

        private Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            // Log the error (replace this with your actual logging mechanism)
            Console.WriteLine($"Error: {arg2.Message}");

            // You can return a completed task to suppress the exception
            return Task.CompletedTask;
        }

        private async Task SendMainMenu(long chatId)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Создать комнату", "/BTN_CREATE"),
                    InlineKeyboardButton.WithCallbackData("Присоединиться", "/BTN_CONNECT")
                }
            });

            await SendMessage(chatId, "Выберите действие:", replyMarkup: keyboard);
        }

        public async Task SendMessage(long chatId, string message, IReplyMarkup replyMarkup = null)
        {
            await botClient.SendTextMessageAsync(chatId, message, replyMarkup: replyMarkup);
        }
    }
}
