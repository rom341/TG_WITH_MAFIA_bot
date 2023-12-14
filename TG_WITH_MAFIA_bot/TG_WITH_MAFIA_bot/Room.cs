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
        public long Id { get; private set; }
        private User Owner;
        public List<User> users { get; private set; }

        //public Room()
        //{
        //    Owner = null;
        //    Id = -1;
        //    users = null;
        //}
        public Room(User owner)
        {
            Owner = owner;
            Id = Owner.ChatID;
            users = new List<User> { Owner };
        }

        public void AddUser(User newUser)
        {
            users.Add(newUser);
        }
        public void RemoveUser(User userToDelete) { users.Remove(userToDelete); }
        public int FindUserIndex(User targetUser)
        {
            return users.FindIndex(player => player.ChatID == targetUser.ChatID);
        }
        public bool ContainsUser(User targetUser)
        {
            return users.Exists(player => player.ChatID == targetUser.ChatID);
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
