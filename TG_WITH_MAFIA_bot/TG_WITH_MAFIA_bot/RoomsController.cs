using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_WITH_MAFIA_bot
{
    public class RoomsController
    {
        private List<Room> rooms;
        public RoomsController() { rooms = new List<Room>(); }
        public RoomsController(List<Room> rooms) { this.rooms = rooms; }

        public void CreateNewRoom(Room newRoom) { rooms.Add(newRoom); }
        public int GetRoomById(long roomId)
        {
            return rooms.FindIndex(currentRoom => currentRoom.Id == roomId);
        }
        public bool AddPlayerTo(int idInList, Player newPlayer)
        {
            if (idInList < 0 || idInList >= rooms.Count) { Console.WriteLine($"Room with id '{idInList}' is not found"); return false; }

            rooms[idInList].AddPlayer(newPlayer);
            return true;
        }
    }
}
