using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_WITH_MAFIA_bot
{
    public class RoomsController
    {
        public List<Room> rooms { get; private set; }
        public RoomsController() { rooms = new List<Room>(); }
        public RoomsController(List<Room> rooms) { this.rooms = rooms; }
        public void CreateNewRoom(Room newRoom) { rooms.Add(newRoom); }
        public int FindRoomIndexById(long roomId)
        {
            return rooms.FindIndex(currentRoom => currentRoom.Id == roomId);
        }
        public bool AddPlayerTo(int idInList, User newPlayer)
        {
            if (idInList < 0 || idInList >= rooms.Count) { Console.WriteLine($"Room with id '{idInList}' is not found"); return false; }

            rooms[idInList].AddPlayer(newPlayer);
            return true;
        }
        public int FindRoomIndexWithPlayer(User player)
        {
            return rooms.FindIndex(room => room.FindPlayerIndex(player) != -1);
        }
        public bool GetRoom(int idInList, out Room resultRoom)
        {
            if (idInList < 0 || idInList >= rooms.Count)
            {
                Console.WriteLine($"Room with id '{idInList}' is not found");
                resultRoom = null;
                return false;
            }

            resultRoom = rooms[idInList];
            return true;
        }
    }
}
