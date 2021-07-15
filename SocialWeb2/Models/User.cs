using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialWeb2
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime? Birthday { get; set; }
        public string Image { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Chats { get; set; }
        public string Friends { get; set; }
        public string Subscribers { get; set; }
        public string Subscriptions { get; set; }
    }
}
