using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TgWithMafia
{
    internal class BotCommandsHandler
    {
        private static TelegramBotClient botClient;
        private static BotCommandsHandler instance;

        public BotCommandsHandler(TelegramBotClient botClient)
        {
            BotCommandsHandler.botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
        }

        public static BotCommandsHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BotCommandsHandler(botClient);
                }
                return instance;
            }
        }

        public async Task HandleCommands(Message message)
        {
            var chatId = message.Chat.Id;
            var text = message.Text;

            if (text.StartsWith("/"))
            {
                if(text.Contains('"'.ToString()) || text.Contains("'") || text.Contains("`") || text.Contains("#"))
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"При вводе команд нельзя использовать символы ` ' {'"'} # ");
                }
                if (text == "/start")
                {
                    await HandleCommandStart(message);
                }
                else if(text=="/help")
                {
                    await HandleCommandHelp(message);
                }
                else if (text == "/roominfo")
                {
                    await HandleCommandRoominfo(message);
                }
                else if (text == "/roomcreate")
                {
                    await HandleCommandCreateroom(message);
                }
                else if (text == "/roomleave")
                {
                    await HandleCommandLeave(message);
                }
                else if (text.StartsWith("/roomconnect"))
                {
                    await HandleCommandRoomConnect(message);
                }
                else if (text == "/roomready")
                {
                    await HandleCommandRoomReady(message);
                }
                else if (text.StartsWith("/roomaddbots"))
                {
                    await HandleCommandRoomAddBots(message);
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Неизвестная команда");
                }
            }
        }

        private async Task HandleCommandRoomAddBots(Message message)
        {
            var chatId = message.Chat.Id;

            // Check if the user is in a room and is the host
            var hostCheckQuery = "SELECT Rooms.RoomID, Rooms.OwnerID FROM Rooms JOIN RoomUsers ON Rooms.RoomID = RoomUsers.RoomID WHERE RoomUsers.UserID = @UserId";
            var hostCheckResult = DatabaseController.Instance.ExecuteSqlQuery(hostCheckQuery, new MySqlParameter("@UserId", chatId));

            if (hostCheckResult.Rows.Count == 0 || chatId != Convert.ToInt64(hostCheckResult.Rows[0]["OwnerID"]))
            {
                await botClient.SendTextMessageAsync(chatId, $"Вы не являетесь хостом комнаты или не состоите в комнате.");
                return;
            }

            // Extract the specified number of bots from the command
            string[] commandParts = message.Text.Split(' ');
            if (commandParts.Length != 2 || !ushort.TryParse(commandParts[1], out ushort numberOfBots))
            {
                await botClient.SendTextMessageAsync(chatId, $"Неправильный формат команды. Используйте /roomaddbots (число).");
                return;
            }

            // Limit the number of bots to 10 if it exceeds the limit
            numberOfBots = Math.Min(numberOfBots, (ushort)10);

            // Get the room ID
            long roomId = Convert.ToInt64(hostCheckResult.Rows[0]["RoomID"]);

            // Check if there are existing bots not in any room
            var existingBotsQuery = "SELECT ChatID FROM Users WHERE PlayerID IS NULL AND ChatID NOT IN (SELECT UserID FROM RoomUsers) AND ChatID <= 9999 LIMIT @NumberOfBots";
            var existingBotsResult = DatabaseController.Instance.ExecuteSqlQuery(existingBotsQuery, new MySqlParameter("@NumberOfBots", numberOfBots));

            if (existingBotsResult.Rows.Count >= numberOfBots)
            {
                AddExistingBotsToRoom(existingBotsResult, roomId);
                await botClient.SendTextMessageAsync(chatId, $"Добавлено {numberOfBots} существующих ботов в комнату.");
            }
            else
            {
                AddNewBotsToRoom(existingBotsResult, numberOfBots, roomId);
                await botClient.SendTextMessageAsync(chatId, $"Создано {numberOfBots} новых ботов и добавлено в комнату.");
            }
        }

        private void AddExistingBotsToRoom(DataTable existingBotsResult, long roomId)
        {
            foreach (DataRow row in existingBotsResult.Rows)
            {
                long botChatID = Convert.ToInt64(row["ChatID"]);
                InsertBotIntoRoom(roomId, botChatID);
            }
        }

        private void AddNewBotsToRoom(DataTable existingBotsResult, ushort numberOfBots, long roomId)
        {
            for (int i = 0; i < numberOfBots; i++)
            {
                long botChatID = GenerateUniqueBotChatID(existingBotsResult);
                InsertNewBot(botChatID);
                InsertBotIntoRoom(roomId, botChatID);
            }
        }

        private long GenerateUniqueBotChatID(DataTable existingBotsResult)
        {
            long botChatID;
            do
            {
                botChatID = new Random().Next(10000);
            } while (existingBotsResult.AsEnumerable().Any(row => Convert.ToInt64(row["ChatID"]) == botChatID));

            return botChatID;
        }

        private void InsertNewBot(long botChatID)
        {
            var insertBotQuery = "INSERT INTO Users (ChatID, TelegramName, UserState) VALUES (@ChatID, @TelegramName, 'INLOBBY')";
            DatabaseController.Instance.ExecuteSqlQuery(insertBotQuery,
                new MySqlParameter("@ChatID", botChatID),
                new MySqlParameter("@TelegramName", $"bot_{botChatID}"));
        }

        private void InsertBotIntoRoom(long roomId, long botChatID)
        {
            var insertRoomUserQuery = "INSERT INTO RoomUsers (RoomID, UserID) VALUES (@RoomID, @UserID)";
            DatabaseController.Instance.ExecuteSqlQuery(insertRoomUserQuery,
                new MySqlParameter("@RoomID", roomId),
                new MySqlParameter("@UserID", botChatID));

            var updateBotUserStateQuery = "UPDATE Users SET UserState = 'INLOBBY' WHERE ChatID = @ChatID";
            DatabaseController.Instance.ExecuteSqlQuery(updateBotUserStateQuery, new MySqlParameter("@ChatID", botChatID));
        }


        private async Task HandleCommandRoomConnect(Message message)
        {
            var chatId = message.Chat.Id;
            var userId = chatId;

            // Extract the provided room creator's ID from the command
            string[] commandParts = message.Text.Split(' ');
            if (commandParts.Length != 2 || !long.TryParse(commandParts[1], out long roomCreatorId))
            {
                await botClient.SendTextMessageAsync(chatId, $"Неправильный формат команды. Используйте /roomconnect (ID создателя комнаты).");
                return;
            }

            // Check if the user is already in a room
            var roomCheckQuery = $"SELECT RoomID FROM RoomUsers WHERE UserID = {userId}";
            var roomCheckResult = DatabaseController.Instance.ExecuteSqlQuery(roomCheckQuery);

            if (roomCheckResult.Rows.Count > 0)
            {
                await botClient.SendTextMessageAsync(chatId, $"Вы уже находитесь в комнате. Используйте /roominfo для получения информации.");
                return;
            }

            // Check if the specified room creator exists
            var roomCreatorCheckQuery = $"SELECT * FROM Users WHERE ChatID = {roomCreatorId}";
            var roomCreatorCheckResult = DatabaseController.Instance.ExecuteSqlQuery(roomCreatorCheckQuery);

            if (roomCreatorCheckResult.Rows.Count == 0)
            {
                await botClient.SendTextMessageAsync(chatId, $"Пользователь с ID {roomCreatorId} не найден.");
                return;
            }

            // Try to connect the user to the room
            var roomConnectQuery = $"INSERT INTO RoomUsers (RoomID, UserID) SELECT RoomID, {userId} FROM Rooms WHERE OwnerID = {roomCreatorId}";
            int rowsAffected = DatabaseController.Instance.ExecuteSqlQuery(roomConnectQuery).Rows.Count;

            if (rowsAffected > 0)
            {
                await botClient.SendTextMessageAsync(chatId, $"Вы успешно подключены к комнате {roomCreatorId}");

                // Notify the room owner (host) about the new participant
                long hostChatId = Convert.ToInt64(roomCreatorCheckResult.Rows[0]["ChatID"]);
                await botClient.SendTextMessageAsync(hostChatId, $"Новый участник подключился к вашей комнате.\nID: {userId}, Login: {message.Chat.Username}");
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, $"Комната с токеном {roomCreatorId} не найдена.");
            }
        }

        private async Task HandleCommandRoomReady(Message message)
        {
            var chatId = message.Chat.Id;

            // Check if the user is in a room and is the host
            var hostCheckQuery = $"SELECT Rooms.RoomID, Rooms.OwnerID FROM Rooms JOIN RoomUsers ON Rooms.RoomID = RoomUsers.RoomID WHERE RoomUsers.UserID = {chatId}";
            var hostCheckResult = DatabaseController.Instance.ExecuteSqlQuery(hostCheckQuery);

            if (hostCheckResult.Rows.Count == 0 || chatId != Convert.ToInt64(hostCheckResult.Rows[0]["OwnerID"]))
            {
                await botClient.SendTextMessageAsync(chatId, $"Вы не являетесь хостом комнаты или не состоите в комнате.");
                return;
            }

            // Create a new record in the Games table with GameStage = null
            var createGameQuery = "INSERT INTO Games (GameStage) VALUES (NULL)";
            DatabaseController.Instance.ExecuteSqlQuery(createGameQuery);

            // Get the ID of the newly created game
            var gameIdQuery = "SELECT LAST_INSERT_ID() as GameID";
            var gameIdResult = DatabaseController.Instance.ExecuteSqlQuery(gameIdQuery);
            long gameId = Convert.ToInt64(gameIdResult.Rows[0]["GameID"]);

            // Update the Room entry to link it to the newly created game
            var updateRoomQuery = $"UPDATE Rooms SET GameID = {gameId} WHERE RoomID = {Convert.ToInt64(hostCheckResult.Rows[0]["RoomID"])}";
            DatabaseController.Instance.ExecuteSqlQuery(updateRoomQuery);

            await botClient.SendTextMessageAsync(chatId, $"Игра готова к запуску. Ожидайте начала");
        }

        private async Task HandleCommandLeave(Message message)
        {
            var chatId = message.Chat.Id;

            // Check if the user is in a room
            var roomCheckQuery = $"SELECT ru.RoomID, r.OwnerID, r.GameID FROM RoomUsers ru JOIN Rooms r ON ru.RoomID = r.RoomID WHERE ru.UserID = {chatId}";
            var roomCheckResult = DatabaseController.Instance.ExecuteSqlQuery(roomCheckQuery);

            if (roomCheckResult.Rows.Count == 0)
            {
                await botClient.SendTextMessageAsync(chatId, $"Вы не находитесь в комнате.");
                return;
            }

            // User is in a room, determine if they are the host or a participant
            long roomId = Convert.ToInt64(roomCheckResult.Rows[0]["RoomID"]);
            long gameId = Convert.ToInt64(roomCheckResult.Rows[0]["GameID"]);
            long ownerChatId = Convert.ToInt64(roomCheckResult.Rows[0]["OwnerID"]);

            // If the user is the host
            if (chatId == ownerChatId)
            {// Delete Roles for all users in this room
                var deleteRolesQuery = $"DELETE FROM Roles WHERE ID IN (SELECT RoleID FROM Players WHERE PlayerID IN (SELECT UserID FROM RoomUsers ru JOIN Users u ON ru.UserID = u.ChatID WHERE ru.RoomID = {roomId}))";
                DatabaseController.Instance.ExecuteSqlQuery(deleteRolesQuery);

                // Delete Players for all users in this room
                var deletePlayersQuery = $"DELETE FROM Players WHERE PlayerID IN (SELECT PlayerID FROM RoomUsers ru JOIN Users u ON ru.UserID = u.ChatID WHERE ru.RoomID = {roomId})";
                DatabaseController.Instance.ExecuteSqlQuery(deletePlayersQuery);

                // Delete Games for this room
                var deleteGamesQuery = $"DELETE FROM Games WHERE GameID = {gameId}";
                DatabaseController.Instance.ExecuteSqlQuery(deleteGamesQuery);

                // Delete Rooms for this room
                var deleteRoomsQuery = $"DELETE FROM Rooms WHERE RoomID = {roomId}";
                DatabaseController.Instance.ExecuteSqlQuery(deleteRoomsQuery);

                // Delete RoomUsers for this room
                var deleteRoomUsersQuery = $"DELETE FROM RoomUsers WHERE RoomID = {roomId}";
                DatabaseController.Instance.ExecuteSqlQuery(deleteRoomUsersQuery);

                // Set Users.UserState = "INMENU" for all participants
                var updateUsersStateQuery = $"UPDATE Users SET UserState = 'INMENU' WHERE ChatID IN (SELECT UserID FROM RoomUsers WHERE RoomID = {roomId})";
                DatabaseController.Instance.ExecuteSqlQuery(updateUsersStateQuery);

                // Notify all users about the exit
                await NotifyUsersInRoom(roomId, "Хост закрыл комнату. Вы были выведены в меню");

                await botClient.SendTextMessageAsync(chatId, $"Комната была закрыта. Вы были выведены в меню.");
            }
            else
            {
                // Delete Roles only for this user
                var deleteRolesQuery = $"DELETE FROM Roles WHERE ID IN (SELECT RoleID FROM Players WHERE PlayerID = {chatId})";
                DatabaseController.Instance.ExecuteSqlQuery(deleteRolesQuery);

                // Delete Players only for this user
                var deletePlayersQuery = $"DELETE FROM Players WHERE PlayerID = {chatId}";
                DatabaseController.Instance.ExecuteSqlQuery(deletePlayersQuery);

                // Delete RoomUsers for this user
                var deleteRoomUsersQuery = $"DELETE FROM RoomUsers WHERE UserID = {chatId} AND RoomID = {roomId}";
                DatabaseController.Instance.ExecuteSqlQuery(deleteRoomUsersQuery);

                // Set Users.UserState = "INMENU" for this user
                var updateUsersStateQuery = $"UPDATE Users SET UserState = 'INMENU' WHERE ChatID = {chatId}";
                DatabaseController.Instance.ExecuteSqlQuery(updateUsersStateQuery);

                // Notify the host about the exit
                await botClient.SendTextMessageAsync(ownerChatId, $"Пользователь @{message.Chat.Username} покинул комнату.");

                await botClient.SendTextMessageAsync(chatId, $"Вы успешно покинули комнату. Вы были выведены в меню.");
            }
        }
        public static async Task NotifyUser(long chatID, string message)
        {
            await botClient.SendTextMessageAsync(chatID, message);
        }
        public static async Task NotifyUsersInRoom(long roomId, string message)
        {
            // Fetch all participants in the room
            var fetchParticipantsQuery = $"SELECT UserID FROM RoomUsers WHERE RoomID = {roomId}";
            var participantsResult = DatabaseController.Instance.ExecuteSqlQuery(fetchParticipantsQuery);

            // Notify each participant about the exit
            foreach (DataRow row in participantsResult.Rows)
            {
                long participantChatId = Convert.ToInt64(row["UserID"]);
                await botClient.SendTextMessageAsync(participantChatId, message);
            }
        }
        public static void NotifyUsersInGame(long gameId, string gameStage)
        {
            // Retrieve all users in the current game
            string getUsersQuery = $"SELECT * FROM Users U JOIN RoomUsers RU ON U.ChatID = RU.UserID JOIN Rooms R ON RU.RoomID = R.RoomID WHERE R.GameID = {gameId}";

            DataTable usersTable = DatabaseController.Instance.ExecuteSqlQuery(getUsersQuery);

            foreach (DataRow userRow in usersTable.Rows)
            {
                long userId = Convert.ToInt64(userRow["ChatID"]);
                // Send a message to each user in the game with the new game stage
                BotCommandsHandler.NotifyUser(userId, $"The game stage is now: {gameStage}");
            }
        }
        public static async Task NotifyUserWithInlineKeyboard(long userId, string message, InlineKeyboardMarkup inlineKeyboard)
        {
            try
            {
                var chatId = new ChatId(userId);
                await botClient.SendTextMessageAsync(chatId, message, replyMarkup: inlineKeyboard);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error notifying user with inline keyboard: " + ex.Message);
            }
        }

        private async Task HandleCommandRoominfo(Message message)
        {
            string sql = $"SELECT DISTINCT U.*, R.OwnerID FROM Users U " +
                         $"JOIN RoomUsers RU1 ON U.ChatID = RU1.UserID " +
                         $"JOIN RoomUsers RU2 ON RU1.RoomID = RU2.RoomID " +
                         $"JOIN Rooms R ON RU1.RoomID = R.RoomID " +
                         $"WHERE RU2.UserID = {message.Chat.Id}";

            var dbResponce = DatabaseController.Instance.ExecuteSqlQuery(sql);

            if (dbResponce.Rows.Count == 0)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Вы не состоите в комнате. Используйте /roomcreate");
                return;
            }
            else
            {
                string host = "";
                string users = "Users:\n";
                long ownerId = Convert.ToInt64(dbResponce.Rows[0]["OwnerID"]);

                foreach (DataRow row in dbResponce.Rows)
                {
                    string name = (string)row.ItemArray[2];

                    if (host == "")
                    {
                        host = $"Host: @{name}";
                    }
                    else
                    {
                        users += $"@{name}\n";
                    }
                }
                users = users == "Users:\n" ? "Users: -" : users;
                string result = $"RoomToken: {ownerId}\n{host}\n{users}";

                await botClient.SendTextMessageAsync(message.Chat.Id, result);
            }
        }

        private async Task HandleCommandCreateroom(Message message)
        {
            var dbResponce = DatabaseController.Instance.ExecuteSqlQuery($"SELECT * FROM RoomUsers WHERE ({message.Chat.Id} = UserID)");
            if (dbResponce.Rows.Count != 0)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Вы уже состоите в комнате. Используйте /roominfo");
                return;
            }

            DatabaseController.Instance.ExecuteSqlQuery($"INSERT INTO Rooms (OwnerID) VALUES ({message.Chat.Id})");
            dbResponce = DatabaseController.Instance.ExecuteSqlQuery($"SELECT RoomID FROM Rooms WHERE ({message.Chat.Id} = OwnerID)");
            DatabaseController.Instance.ExecuteSqlQuery($"INSERT INTO RoomUsers (RoomID, UserID) VALUES ({dbResponce.Rows[0].ItemArray[0]}, {message.Chat.Id})");
            DatabaseController.Instance.ExecuteSqlQuery($"UPDATE Users SET UserState = 'INLOBBY' WHERE ChatID = {message.Chat.Id}");

            await botClient.SendTextMessageAsync(message.Chat.Id, $"Комната успешно создана. Токен комнаты: {message.Chat.Id}");
        }

        private async Task HandleCommandHelp(Message message)
        {
            string helpText = "Список команд:\n" +

                "\nОбщие команды:\n" +
                "/start - начать работу с ботом\n" +
                "/help - получить список команд\n" +

                "\nКоманды для работы внутри комнаты:\n" +
                "/roomcreate - создать новую комнату для игры\n" +
                "/roomleave - покинуть комнату (расформировать, если вы - хост)\n" +
                "/roominfo - получить информацию о вашей комнате\n" +
                "/roomready - запустите игру и ожидайте начала дня\n" +
                "/roomaddbots (число) - HOST ONLY. Добавляет ботов в комнату. Число вводить без скобок.";

            await botClient.SendTextMessageAsync(message.Chat.Id, helpText);
        }

        private async Task HandleCommandStart(Message message)
        {
            if(DatabaseController.Instance.ExecuteSqlQuery($"SELECT * FROM 'Users' WHERE ChatID = {message.Chat.Id}") == null)
            {
                DatabaseController.Instance.ExecuteSqlQuery($"INSERT INTO Users (ChatID) VALUES ({message.Chat.Id})");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Привет, я бот для игры в мафию. Для получения списка команд введи команду /help");
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Вы уже зарегистрированы. Если вы забыли какую либо команду, используйте /help");
            }
        }

        private async Task HandleCommandSay(Message message)
        {
            long chatId = message.Chat.Id;

            var roomDataQuery = $"SELECT R.RoomID, R.GameID, R.OwnerID, G.GameStage " +
                        $"FROM RoomUsers RU " +
                        $"JOIN Rooms R ON RU.RoomID = R.RoomID " +
                        $"JOIN Games G ON R.GameID = G.GameID " +
                        $"WHERE RU.UserID = {chatId}";

            var roomDataResult = DatabaseController.Instance.ExecuteSqlQuery(roomDataQuery);

            if (roomDataResult.Rows.Count == 0)
            {
                await botClient.SendTextMessageAsync(chatId, "Вы не находитесь в комнате. Используйте /roomcreate, чтобы создать комнату.");
                return;
            }

            var roomId = Convert.ToInt64(roomDataResult.Rows[0]["RoomID"]);
            var gameId = Convert.ToInt64(roomDataResult.Rows[0]["GameID"]);
            var ownerChatId = Convert.ToInt64(roomDataResult.Rows[0]["OwnerID"]);
            var gameStage = roomDataResult.Rows[0]["GameStage"].ToString();
            string saidMessage = message.Text.Substring(6);

            if (gameStage == "NIGHT")
            {
                string userRolesQuery = $"SELECT U.ChatID, R.RoleName FROM" +
    "Rooms Ro " +
    "left join RoomUsers Ru on Ru.RoomID = Ro.RoomID " +
    "left join Users U on U.ChatID = Ru.UserID " +
    "left join Players P on P.PlayerID = U.PlayerID " +
    "left join Roles R on R.ID = P.RoleID " +
    $"WHERE Ro.RoomID = {roomId}";

                var userRolesResult = DatabaseController.Instance.ExecuteSqlQuery(userRolesQuery);

                foreach (DataRow row in userRolesResult.Rows)
                {
                    long otherUserId = Convert.ToInt64(row["ChatID"]);
                    string roleName = row["RoleName"].ToString();

                    if (otherUserId != chatId && roleName == "MAFIA")
                    {
                        await botClient.SendTextMessageAsync(otherUserId, $"[{message.Chat.Username}] : {saidMessage}");
                    }
                }
            }
            else
            {
                
            }
        }
    }
}
