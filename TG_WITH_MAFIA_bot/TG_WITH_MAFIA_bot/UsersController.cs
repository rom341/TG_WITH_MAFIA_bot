using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_WITH_MAFIA_bot
{
    public class UsersController
    {
        private List<User> users;
        public UsersController() { users = new List<User>(); }
        public UsersController(List<User> users) { this.users = users; }
        public int GetUserListId(long ChatID)
        {
            return users.FindIndex(us => us.ChatId == ChatID);
        }
        public bool ChangeUserState(int idInList, UserStates state)
        {
            if (idInList < 0 || idInList >= users.Count) { Console.WriteLine($"User with id '{idInList}' is not found"); return false; }
            users[idInList].UserState = state;
            return true;
        }
        public UserStates GetUserState(int idInList)
        {
            if (idInList < 0 || idInList >= users.Count) { Console.WriteLine($"User with id '{idInList}' is not found"); return UserStates.INMENU; }
            return users[idInList].UserState;
        }
        public void AddUser(User user)
        {
            users.Add(user);
        }
        public bool Contains(User user)
        {
            if (GetUserListId(user.ChatId) != -1) return true;
            return false;
        }
    }
}
