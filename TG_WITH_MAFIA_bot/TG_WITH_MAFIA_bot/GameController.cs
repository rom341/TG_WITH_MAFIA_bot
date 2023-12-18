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
                        // обменять значения data[j] и data[i]
                        var temp = room.users[j];
                        room.users[j] = room.users[i];
                        room.users[i] = temp;
                    }

                    for (int i = 0; i < room.users.Count / 3; i++)
                    {
                        room.users[0].player.role = new MafiaRole();
                    }
                    for (int i = room.users.Count / 3; i < room.users.Count; i++)
                    {
                        room.users[0].player.role = new CivilianRole();
                    }
                }
                ////Заготовка для большого количевста ролей
                //else if(room.users.Count < 5)
                //{
                //    for (int i = room.users.Count - 1; i >= 1; i--)
                //    {
                //        int j = random.Next(i + 1);
                //        // обменять значения data[j] и data[i]
                //        var temp = room.users[j];
                //        room.users[j] = room.users[i];
                //        room.users[i] = temp;
                //    }
                //    room.users[0].player.role = new MafiaRole();
                //    foreach(User user in room.users)
                //        user.player.role = new CivilianRole();
                //}
            }

            public void ProcessNightPhase()
            {
                // Логика для ночной фазы (если в игре присутствует ночь)
                // ...

                Console.WriteLine("Наступила ночь. Игроки делают свои действия.");

                foreach (var player in players)
                {
                    if (player.role is MafiaRole)
                    {
                        // Логика для действия мафии в ночи
                        // ...
                        Console.WriteLine($"Мафия игрока {player.user.ChatID} действует.");
                        //player.role.UseSkill(ref /* цель для мафии */);
                    }
                    // Другие проверки для других ролей, если необходимо
                }
            }

            public void ProcessDayPhase()
            {
                // Логика для дневной фазы (если в игре присутствует день)
                // ...

                Console.WriteLine("Наступил день. Игроки обсуждают события.");

                foreach (var player in players)
                {
                    // Логика для обсуждения и принятия решений
                    // ...
                }
            }
        }
    }
}
