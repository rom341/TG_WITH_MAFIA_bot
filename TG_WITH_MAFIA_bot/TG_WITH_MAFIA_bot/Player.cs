using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_WITH_MAFIA_bot
{
    public class Player
    {
        //public User user { get; set; }
        //public string name { get; set; }
        public RoleBase role { get; set; }
        Player(RoleBase role) { this.role = role; }
    }
}
