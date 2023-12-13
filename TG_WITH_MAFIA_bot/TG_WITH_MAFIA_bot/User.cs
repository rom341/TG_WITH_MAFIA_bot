using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TG_WITH_MAFIA_bot
{
    public enum UserStates
    {
        INMENU,
        INLOBBY,
        INGAME
    }
    public class User
    {
        public long ChatId { get; set; }
        public UserStates UserState { get; set; }
        public User(long chatId, UserStates userState)
        {
            ChatId = chatId;
            UserState = userState;
        }
    }
}
