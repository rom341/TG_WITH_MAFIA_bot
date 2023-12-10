using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_WITH_MAFIA_bot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BotController controller = new BotController();
            controller.StartBot();
            Console.ReadLine();
        }
    }
}
