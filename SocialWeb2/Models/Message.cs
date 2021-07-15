using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SocialWeb2
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int IdUser { get; set; }
        public DateTime Date { get; set; }
        public int ChatId { get; set; }
        public virtual Chat Chat { get; set; }
    }
}
