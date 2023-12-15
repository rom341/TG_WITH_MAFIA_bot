using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TG_WITH_MAFIA_bot
{
    public class Room
    {
        private BotController botController;
        public long Id { get; private set; }
        public User Owner { get; private set; }
        public List<User> users { get; private set; }
        public Room(User owner, BotController botController)
        {
            Owner = owner;
            Id = Owner.ChatID;
            users = new List<User> { Owner };
            this.botController = botController;
        }

        public async Task AddUser(User newUser)
        {
            users.ForEach(async user =>
            {
                await botController.SendMessage(user.ChatID, $"Пользователь ({newUser.ChatID}) Присоеденился");
                user.UserState = UserStates.INMENU;
            });
            users.Add(newUser);
        }
        public async Task RemoveUser(User userToDelete) 
        {
            users.Remove(userToDelete);
            users.ForEach(async user =>
            {
                await botController.SendMessage(user.ChatID, $"Пользователь ({userToDelete.ChatID}) покинул комнату");
                user.UserState = UserStates.INMENU;
            });
        }
        public int FindUserIndex(User targetUser)
        {
            return users.FindIndex(player => player.ChatID == targetUser.ChatID);
        }
        public bool ContainsUser(Func<User, bool> predicate)
        {
            return users.Exists(user => predicate(user));
        }
        public int FindUserIndex(Func<User, bool> predicate)
        {
            return users.FindIndex(user => predicate(user));
        }
        public override string ToString()
        {
            StringBuilder playerIds = new StringBuilder();
            foreach (var player in users)
            {
                playerIds.Append(player.ChatID).Append(", ");
            }

            // Remove the trailing comma and space
            if (playerIds.Length > 0)
            {
                playerIds.Length -= 2;
            }

            return $"Room ID: {Id}\nPlayer IDs: {playerIds}";
        }
    }
}
