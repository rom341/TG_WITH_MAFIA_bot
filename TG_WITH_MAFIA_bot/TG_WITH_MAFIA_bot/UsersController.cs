﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_WITH_MAFIA_bot
{
    public class UsersController
    {
        public List<User> users { get; private set; }
        public UsersController() { users = new List<User>(); }
        public UsersController(List<User> users) { this.users = users; }
        public int FindUserIndex(long ChatID)
        {
            return users.FindIndex(user => user.ChatID == ChatID);
        }
        public int FindUserIndex(Func<User, bool> predicate)
        {
            return users.FindIndex(user => predicate(user));
        }
        public bool ChangeUserState(int idInList, UserStates state)
        {
            if (idInList < 0 || idInList >= users.Count) { Console.WriteLine($"User with id '{idInList}' is not found"); return false; }
            users[idInList].UserState = state;
            return true;
        }
        public UserStates GetUserState(int idInList)
        {
            if (idInList < 0 || idInList >= users.Count) { Console.WriteLine($"User with id '{idInList}' is not found"); return UserStates.NONE; }
            return users[idInList].UserState;
        }
        public void AddUser(User user)
        {
            users.Add(user);
        }
        public bool Contains(User user)
        {
            if (FindUserIndex(user.ChatID) != -1) return true;
            return false;
        }
    }
}
