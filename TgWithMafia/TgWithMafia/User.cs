using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgWithMafia
{
    public enum UserState
    {
        INMENU,
        INLOBBY,
        INGAME
    }
    internal class User
    {
        public ulong ID { get; set; }
        public ulong ChatID { get; set; }
        public string TelegramName { get; set; }
        public ulong PlayerID { get; set; }
        public UserState userState { get; set; }
    }
}
