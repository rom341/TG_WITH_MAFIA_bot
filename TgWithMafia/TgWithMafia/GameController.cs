using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Telegram.Bot.Types;

namespace TgWithMafia
{
    internal class GameController
    {
        private System.Threading.Timer gameTimer;
        private Random random;

        public GameController()
        {
            int timerInterval = 30000;
            gameTimer = new System.Threading.Timer(TimerCallback, null, 0, timerInterval);
            random = new Random();
            BotCallBackQuery.Initialize();
        }

        private void TimerCallback(object state)
        {
            try
            {
                // Get all Games from the database
                DataTable gamesTable = DatabaseController.Instance.ExecuteSqlQuery("SELECT * FROM Games WHERE IsPaused = 0");

                foreach (DataRow gameRow in gamesTable.Rows)
                {
                    long gameId = Convert.ToInt64(gameRow["GameID"]);
                    string currentState = gameRow["GameStage"].ToString();

                    // Determine the next game state
                    string nextState = GetNextGameState(currentState);
                    if (currentState == string.Empty)
                    {
                        AssignRoles(gameId);
                    }
                    // If the game state has changed, update the database and notify users
                    if (currentState != nextState)
                    {
                        // Update the game state in the database
                        DatabaseController.Instance.ExecuteSqlQuery($"UPDATE Games SET GameStage = '{nextState}' WHERE GameID = {gameId}");

                        // Notify all users in the current game about the new game stage
                        string notifyMessage = $"Наступает {nextState}.";
                        BotCommandsHandler.NotifyUsersInGame(gameId, notifyMessage);

                        if (nextState == "MIDDAY" || nextState == "NIGHT")
                        {
                            DisplayPlayerMenuAsync(gameId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during timer callback: " + ex.Message);
            }
        }

        private string GetNextGameState(string currentState)
        {
            // Define the order of game states (you can modify this based on your specific states)
            string[] gameStates = { "MORNING", "MIDDAY", "EVENING", "NIGHT" };

            if (currentState == null)
            {
                return "MORNING"; // If the game hasn't started, set the initial state to MORNING
            }

            // Find the index of the current state and get the next one in the sequence
            int currentIndex = Array.IndexOf(gameStates, currentState);
            int nextIndex = (currentIndex + 1) % gameStates.Length;

            return gameStates[nextIndex];
        }

        private void AssignRoles(long gameId)
        {
            // Retrieve all users in the current game
            string getUsersQuery = $"SELECT * FROM Users U JOIN RoomUsers RU ON U.ChatID = RU.UserID JOIN Rooms R ON RU.RoomID = R.RoomID WHERE R.GameID = {gameId}";

            DataTable usersTable = DatabaseController.Instance.ExecuteSqlQuery(getUsersQuery);
            // Check if there are users to assign roles
            if (usersTable.Rows.Count == 0)
            {
                MessageBox.Show("No users found for the given game ID in 'assigmentRole' function.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int totalUsers = usersTable.Rows.Count;
            int mafiaCount = totalUsers / 3;

            Dictionary<int, string> roleIds = GenerateRoleIds(totalUsers, mafiaCount);

            // Shuffle the roles
            List<int> shuffledRoleIds = new List<int>(roleIds.Keys);
            shuffledRoleIds = shuffledRoleIds.OrderBy(x => random.Next()).ToList();

            for (int i = 0; i < totalUsers; i++)
            {
                int roleId = shuffledRoleIds[i];

                // Create a new player entry in the database
                DatabaseController.Instance.ExecuteSqlQuery($"INSERT INTO Players (InGameName, RoleID) VALUES ('Player{i + 1}', {roleId})");

                // Retrieve the last inserted PlayerID
                DataTable lastInsertIdTable = DatabaseController.Instance.ExecuteSqlQuery("SELECT LAST_INSERT_ID() AS ID");
                long playerId = Convert.ToInt64(lastInsertIdTable.Rows[0]["ID"]);

                // Retrieve the UserID for the current user
                long userId = Convert.ToInt64(usersTable.Rows[i]["ChatID"]);

                // Update the User record with the assigned PlayerID
                DatabaseController.Instance.ExecuteSqlQuery($"UPDATE Users SET PlayerID = {playerId} WHERE ChatID = {userId}");

                // Display role information to the user (you may implement this part)
                BotCommandsHandler.NotifyUser(userId, $"Your role is: {roleIds[roleId]}");
            }
        }

        private Dictionary<int, string> GenerateRoleIds(int totalUsers, int mafiaCount)
        {
            Dictionary<int, string> roleDictionary = new Dictionary<int, string>();

            // Assign MAFIA roles
            for (int i = 0; i < mafiaCount; i++)
            {
                int roleId = CreateRole("MAFIA");
                roleDictionary.Add(roleId, "MAFIA");
            }

            // Assign DOCTOR role if there are 5 or more players
            if (totalUsers >= 5)
            {
                int doctorRoleId = CreateRole("DOCTOR");
                roleDictionary.Add(doctorRoleId, "DOCTOR");
            }

            // Assign CIVILIAN roles to the remaining players
            while (roleDictionary.Count < totalUsers)
            {
                int civilianRoleId = CreateRole("CIVILIAN");
                roleDictionary.Add(civilianRoleId, "CIVILIAN");
            }

            return roleDictionary;
        }

        private int CreateRole(string roleName)
        {
            // Create a new entry in the Roles table
            DatabaseController.Instance.ExecuteSqlQuery($"INSERT INTO Roles (RoleName, Health) VALUES ('{roleName}', 100)");

            // Retrieve the RoleID of the newly created role
            DataTable newRoleTable = DatabaseController.Instance.ExecuteSqlQuery("SELECT LAST_INSERT_ID() AS ID");
            return Convert.ToInt32(newRoleTable.Rows[0]["ID"]);
        }

        private void DisplayPlayerMenuAsync(long gameId)
        {
            try
            {
                // Retrieve all users in the current game
                string getUsersQuery = $"SELECT * FROM Users U JOIN RoomUsers RU ON U.ChatID = RU.UserID JOIN Rooms R ON RU.RoomID = R.RoomID WHERE R.GameID = {gameId}";

                DataTable usersTable = DatabaseController.Instance.ExecuteSqlQuery(getUsersQuery);

                // Check if there are users in the current room
                if (usersTable.Rows.Count == 0)
                {
                    MessageBox.Show("No users found in the current room.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (DataRow userRow in usersTable.Rows)
                {
                    long userId = Convert.ToInt64(userRow["ChatID"]);

                    // Exclude the current player from the list
                    var otherPlayers = usersTable.AsEnumerable()
                        .Where(row => Convert.ToInt64(row["ChatID"]) != userId)
                        .ToDictionary(row => Convert.ToInt64(row["ChatID"]), row => row["TelegramName"].ToString());

                    // Display the menu to the current player
                    BotCallBackQuery.Instance.DisplayMenuAsync(userId, otherPlayers);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during menu display: " + ex.Message);
            }
        }
    }
}
