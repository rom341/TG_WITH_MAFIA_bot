using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_WITH_MAFIA_bot
{
    public class RoomsController
    {
        private BotController botController;
        public List<Room> rooms { get; private set; }
        public RoomsController(BotController botController) { rooms = new List<Room>(); this.botController = botController; }
        public void CreateNewRoom(Room newRoom) { rooms.Add(newRoom); }
        public void DestroyRoom(Room room) 
        { 
            room.users.ForEach(user => 
            { 
                botController.SendMessage(user.ChatID, $"Комната, в которой вы находились ({room.Id}) удалена");
                user.UserState = UserStates.INMENU;
            });
            rooms.Remove(room);
        }
        //public int FindRoomIndexById(long roomId)
        //{
        //    return rooms.FindIndex(currentRoom => currentRoom.Id == roomId);
        //}
        public bool AddPlayerTo(int idInList, User newPlayer)
        {
            if (idInList < 0 || idInList >= rooms.Count) { Console.WriteLine($"Room with id '{idInList}' is not found"); return false; }

            rooms[idInList].AddUser(newPlayer);
            return true;
        }
        public int FindRoomIndexWithUser(User user)
        {
            return rooms.FindIndex(room => room.FindUserIndex(user) != -1);
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
        public int FindRoomIndex(Func<Room, bool> predicate)
        {
            return rooms.FindIndex(room => predicate(room));
        }
    }
}
