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
        private List<User> players;

        public Room(User owner)
        {
            Owner = owner;
            Id = Owner.ChatID;
            players = new List<User> { Owner };
        }

        public void AddPlayer(User newPlayer)
        {
            players.Add(newPlayer);
        }
        public int FindPlayerIndex(User targetPlayer)
        {
            return players.FindIndex(player => player.ChatID == targetPlayer.ChatID);
        }
        public bool ContainsPlayer(User targetPlayer)
        {
            return players.Exists(player => player.ChatID == targetPlayer.ChatID);
        }
        public override string ToString()
        {
            StringBuilder playerIds = new StringBuilder();
            foreach (var player in players)
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
