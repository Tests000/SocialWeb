using System.Collections.Generic;

namespace SocialWeb2
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Message> Messages { get; set; }

        public Chat()
        {
            Messages = new List<Message>();
        }

    }
}