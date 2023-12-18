using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_WITH_MAFIA_bot
{
    public class GameController
    {
        private BotController botController;
        public List<Room> rooms { get; private set; }
        private RoleBase[] PossibleRoles = new RoleBase[] { new CivilianRole(), new MafiaRole() };

        public GameController(BotController botController)
        {
            this.botController = botController;
        }


        public void StartGame(List<Room> roomsWhereGameWillBegin)
        {
            // Инициализация игры, ролей, раздача ролей игрокам и т.д.
            GiveRoles(roomsWhereGameWillBegin);

            foreach(Room room in rooms)
            { 
                Console.WriteLine($"Игра '{room.Id}' началась!");
                room.SendMEssageToAllUsers("Игра в вашей комнате началась!");
            }
            
        }
        private void GiveRoles(List<Room> roomsWhereGameWillBegin)
        {
            rooms = roomsWhereGameWillBegin;
            Random random = new Random();
            foreach (Room room in rooms)
            {
                if (room.users.Count < 3)
                {
                    botController.SendMessage(room.Owner.ChatID, $"Для начала игры необходимо, как минимум 3 игрока");
                    rooms.Remove(room);
                }
                //Заполняем 1/3 мафией, остальных гражданскими
                else
                {
                    //Перемешаем масив
                    for (int i = room.users.Count - 1; i >= 1; i--)
                    {
                        int j = random.Next(i + 1);
                        var temp = room.users[j];
                        room.users[j] = room.users[i];
                        room.users[i] = temp;
                    }
                    //Мафией будеи 1/3 игроков
                    for (int i = 0; i < room.users.Count / 3; i++)
                    {
                        room.users[0].player.role = new MafiaRole();
                    }
                    //Остальные - гражданские
                    for (int i = room.users.Count / 3; i < room.users.Count; i++)
                    {
                        room.users[0].player.role = new CivilianRole();
                    }
                    foreach(var user in room.users)
                    {
                        botController.SendMessage(user.ChatID, $"Ваша роль: {user.player.role.Name}\nОписание: {user.player.role.Description}");
                    }
                }
            }
        }
        public void ProcessNightPhase()
        {
            // Логика для ночной фазы
            foreach (var room in rooms)
            {
                room.SendMEssageToAllUsers("Наступила ночь. Игроки делают свои действия.");
                foreach (var user in room.users)
                {
                    if (user.player.role is MafiaRole)
                    {
                        botController.SendMessage(user.ChatID, $"Выберите игрока, которого хотите убить");
                    }
                }
            }
        }

        public void ProcessDayPhase()
        {
            // Логика для дневной фазы
            foreach (var room in rooms)
            {
                room.SendMEssageToAllUsers("Наступил День. Время начать обсуждение");
            }
        }
    }
}
