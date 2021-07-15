using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialWeb2.Models
{
    public class ViewMessage
    {
        public string Text { get; set; }
        public string UserName { get; set; }
        public DateTime Date { get; set; }
        public bool My { get; set; }
        public string Image { get; set; }
    }
}
