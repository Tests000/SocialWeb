using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialWeb2.Models
{
    public class ViewChat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public Message LastMessage { get; set; }
    }
}
