using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TG_WITH_MAFIA_bot
{
    public enum RoomStates
    {
        NONE,//ERROR
        WAITING,
        ISREADYTOSTART,
        INGAME
    }
    public class Room
    {
        private BotController botController;
        public RoomStates roomState { get; set; }
        public long Id{ get; set; }
        public User Owner { get; private set; }
        public List<User> users { get; private set; }
        public Room(User owner, BotController botController)
        {
            Owner = owner;
            Id = Owner.ChatID;
            users = new List<User> { Owner };
            this.botController = botController;
            roomState = RoomStates.WAITING;
        }

        public async Task AddUser(User newUser)
        {
            await botController.SendMessage(Owner.ChatID, $"Пользователь ({newUser.ChatID}) Присоеденился");
            newUser.UserState = UserStates.INLOBBY;
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
        public void SendMEssageToAllUsers(string message)
        {
            foreach (User user in users)
            {
                botController.SendMessage(user.ChatID, message);
            }
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
