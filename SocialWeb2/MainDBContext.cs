using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SocialWeb2
{
    public class MainDBContext : DbContext
    {
        public MainDBContext(DbContextOptions<MainDBContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
