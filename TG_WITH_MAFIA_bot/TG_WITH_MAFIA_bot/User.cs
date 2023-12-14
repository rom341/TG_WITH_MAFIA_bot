using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_WITH_MAFIA_bot
{
    public enum UserStates
    {
        NONE,
        INMENU,
        INLOBBY,
        INGAME
    }
    public class User
    {
        public long ChatID { get; private set; }
        public UserStates UserState { get; set; }
        public User(long id)
        {
            ChatID = id;
            UserState = UserStates.INMENU;
        }
    }
}
