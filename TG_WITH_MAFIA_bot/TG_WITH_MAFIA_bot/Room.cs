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
        public long Id { get; set; }
        private Player Owner;
        private List<Player> players;

        public Room(Player owner)
        {
            Owner = owner;
            Id = Owner.Id;
            players = new List<Player> { Owner };
        }

        public void AddPlayer(Player newPlayer)
        {
            players.Add(newPlayer);
        }
        public int GetPlayerListId(Player targetPlayer)
        {
            return players.FindIndex(player => player.Id == targetPlayer.Id);
        }
        public override string ToString()
        {
            StringBuilder playerIds = new StringBuilder();
            foreach (var player in players)
            {
                playerIds.Append(player.Id).Append(", ");
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
