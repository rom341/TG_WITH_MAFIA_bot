using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_WITH_MAFIA_bot
{
    public class GameController
    {
        private List<Player> players;
        private List<RoleBase> roles;
        RoleBase[] PossibleRoles = new RoleBase[] { new CivilianRole(), new MafiaRole() };

        public GameController(List<Player> players)
        {
            this.players = players;
            this.roles = new List<RoleBase>();
            Random random = new Random();

            foreach (var player in players)
            {
                IRole currentRole = PossibleRoles[random.Next(PossibleRoles.Length)].Clone();
                if (currentRole is MafiaRole && roles.Count(r => r.GetType() == typeof(MafiaRole)) < roles.Count / 4)
                {
                    roles.Add((RoleBase)currentRole);
                    player.role = (RoleBase)currentRole;
                }
            }
        }

        public void StartGame()
        {
            // Инициализация игры, ролей, раздача ролей игрокам и т.д.
            // ...

            Console.WriteLine("Игра началась!");
            foreach (var player in players)
            {
                Console.WriteLine($"{player.user.ChatID} получил роль {player.role.Name}");
            }
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
