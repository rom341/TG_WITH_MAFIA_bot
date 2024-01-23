using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TG_WITH_MAFIA_bot
{
    public class GameController
    {
        private BotController botController;
        public List<Room> roomsWithStartedGame { get; private set; }
        private Timer phaseTimer;
        //private RoleBase[] PossibleRoles = new RoleBase[] { new CivilianRole(), new MafiaRole() };

        public GameController(BotController botController)
        {
            this.botController = botController;
            phaseTimer = new Timer(120000);//2 минуты
            this.phaseTimer.Elapsed += OnPhaseTimerElapsed;
        }

        private void OnPhaseTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine($"PhaseTimer tick");
            foreach (var room in roomsWithStartedGame)
            {
                room.SetNextPhase();
                if (room.gamePhase == GamePhases.DAY)
                    ProcessDayPhase(room);
                else if (room.gamePhase == GamePhases.NIGHT)
                    ProcessNightPhase(room);
            }
        }

        public void StartGame(List<Room> rooms)
        {
            List<Room> roomWhereGameWillStart = rooms.Where(room => room.roomState == RoomStates.ISREADYTOSTART).ToList();
            //Роздаем роли
            if (!GiveRoles(roomWhereGameWillStart))
                Console.WriteLine($"StartGame error");

            foreach (Room room in roomsWithStartedGame)
            {
                Console.WriteLine($"Игра '{room.Id}' началась!");
                room.SendMEssageToAllUsers("Игра в вашей комнате началась!");
            }
            if (!phaseTimer.Enabled) phaseTimer.Start();
        }
        private bool GiveRoles(List<Room> roomsWhereGameWillBegin)
        {
            this.roomsWithStartedGame = roomsWhereGameWillBegin;
            Random random = new Random();
            foreach (Room room in this.roomsWithStartedGame)
            {
                if (room.roomState != RoomStates.ISREADYTOSTART)
                {
                    botController.SendMessage(room.Owner.ChatID, $"Для начала игры измените статус комнаты при помощи команды '/room ready'");
                    return false;
                }
                if (room.users.Count < 3)
                {
                    botController.SendMessage(room.Owner.ChatID, $"Для начала игры необходимо, как минимум 3 игрока");
                    roomsWithStartedGame.Remove(room);
                    return false;
                }
                //Заполняем 1/3 мафией, остальных гражданскими
                else
                {
                    room.roomState = RoomStates.INGAME;
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
                        room.users[i].player = new Player(new MafiaRole());
                    }
                    //Остальные - гражданские
                    for (int i = room.users.Count / 3; i < room.users.Count; i++)
                    {
                        room.users[i].player = new Player(new CivilianRole());
                    }
                    foreach (var user in room.users)
                    {
                        botController.SendMessage(user.ChatID, $"Ваша роль: {user.player.role.Name}\nОписание: {user.player.role.Description}");
                    }
                }
            }
            return true;
        }
        private void ProcessNightPhase(Room room)
        {
            // Логика для ночной фазы

            room.SendMEssageToAllUsers("Наступила ночь. Игроки делают свои действия.");
            foreach (var user in room.users)
            {
                if (user.player.role is MafiaRole)
                {
                    botController.SendMessage(user.ChatID, $"Выберите игрока, которого хотите убить");
                }
            }
        }

        public void ProcessDayPhase(Room room)
        {
            // Логика для дневной фазы

            room.SendMEssageToAllUsers("Наступил День. Время начать обсуждение");

        }
    }
}
