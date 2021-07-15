using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SocialWeb2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SocialWeb2.Controllers
{
    public class Account : Controller
    {
        public MainDBContext db;

        public IWebHostEnvironment env;

        public Account(MainDBContext context, IWebHostEnvironment env)
        {
            this.env = env;
            db = context;
            db.Messages.Load();
        }

        [Authorize]
        public async Task<IActionResult> Prof(int? UserId)
        {
            List<User> user = new List<User>();
            if (UserId == null)
            {
                User user1 = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
                if (user1 == null)
                {
                    await Logout();
                    return RedirectToAction("Login", "Account");
                }
                user.Add(user1);
                return View(user);
            }
            else
            {
                User us = await db.Users.FirstOrDefaultAsync(u => u.Id == UserId);
                if (us != null)
                {
                    user.Add(us);
                    User user1 = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
                    if (user1.Subscriptions != null && user1.Subscriptions != "")
                    {
                        var userSub = user1.Subscriptions.Split(' ');
                        foreach (var i in userSub)
                        {
                            int id = Int32.Parse(i);
                            if (id == UserId)
                            {
                                ViewData["Message"] = "Unsub";
                                return View(user);
                            }
                        }
                    }
                    if (user1.Friends != null && user1.Friends != "")
                    {
                        var userFrs = user1.Friends.Split(' ');
                        foreach (var i in userFrs)
                        {
                            int id = Int32.Parse(i);
                            if (id == UserId)
                            {
                                ViewData["Message"] = "Unsub";
                                return View(user);
                            }
                        }
                    }
                    return View(user);
                }
            }
            return RedirectToAction("Prof", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> Prof(string Name, int? mes, int? fr, string frList, int? unfr)
        {
            if (Name != null)
                return RedirectToAction("People", "Account", new { users = Name });
            User user1 = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (mes != null)
            {

                User user2 = await db.Users.FirstOrDefaultAsync(u => u.Id == mes);
                string chatName;
                Chat chat;
                if (mes < user1.Id)
                    chatName = mes.ToString() + ' ' + user1.Id.ToString();
                else
                    chatName = user1.Id.ToString() + ' ' + mes.ToString();
                chat = await db.Chats.FirstOrDefaultAsync(u => u.Name == chatName);
                if (chat == null)
                {
                    chat = new Chat();
                    chat.Name = chatName;
                    db.Chats.Add(chat);
                    db.SaveChanges();
                    chat = await db.Chats.FirstOrDefaultAsync(u => u.Name == chatName);
                    user1.Chats = AddToList(user1.Chats, chat.Id.ToString());
                    user2.Chats = AddToList(user2.Chats, chat.Id.ToString());
                    db.Users.Update(user1);
                    db.Users.Update(user2);
                    db.SaveChanges();
                }
                return RedirectToAction("Messages", "Account", new { chatId = chat.Id });

            }
            if (fr != null)
            {
                User user2 = await db.Users.FirstOrDefaultAsync(u => u.Id == fr);
                if (user2.Subscriptions != null && user2.Subscriptions != "")
                {
                    var Ids = user2.Subscriptions.Split(' ');
                    foreach (var i in Ids)
                    {
                        int id = Int32.Parse(i);
                        if (id == user1.Id)
                        {
                            user1.Friends = AddToList(user1.Friends, fr.ToString());
                            user1.Subscribers = RemoveFromList(user1.Subscribers, fr.ToString());
                            user2.Subscriptions = RemoveFromList(user2.Subscriptions, user1.Id.ToString());
                            user2.Friends = AddToList(user2.Friends, user1.Id.ToString());
                            db.Users.Update(user1);
                            db.Users.Update(user2);
                            db.SaveChanges();
                            return RedirectToAction("Prof", "Account", new { UserId = fr });
                        }

                    }
                }
                user1.Subscriptions = AddToList(user1.Subscriptions, fr.ToString());
                user2.Subscribers = AddToList(user2.Subscribers, user1.Id.ToString());
                db.Users.Update(user1);
                db.Users.Update(user2);
                db.SaveChanges();
                return RedirectToAction("Prof", "Account", new { UserId = fr });
            }
            if (frList != null)
            {
                var data = frList.Split(' ');
                return RedirectToAction("Friends", "Account", new { id = Int32.Parse(data[0]), ListType = Int32.Parse(data[1]) });
            }
            if (unfr != null)
            {
                User user2 = await db.Users.FirstOrDefaultAsync(u => u.Id == unfr);
                if (user1.Subscriptions != null && user1.Subscriptions != "")
                {
                    var userSub = user1.Subscriptions.Split(' ');
                    foreach (var i in userSub)
                    {
                        int id = Int32.Parse(i);
                        if (id == unfr)
                        {
                            user1.Subscriptions = RemoveFromList(user1.Subscriptions, unfr.ToString());
                            user2.Subscribers = RemoveFromList(user2.Subscribers, user1.Id.ToString());
                            db.Users.Update(user1);
                            db.Users.Update(user2);
                            db.SaveChanges();
                            return RedirectToAction("Prof", "Account", new { UserId = unfr });
                        }
                    }
                }
                if (user1.Friends != null && user1.Friends != "")
                {
                    var userFrs = user1.Friends.Split(' ');
                    foreach (var i in userFrs)
                    {
                        int id = Int32.Parse(i);
                        if (id == unfr)
                        {
                            user1.Friends = RemoveFromList(user1.Friends, unfr.ToString());
                            user2.Friends = RemoveFromList(user2.Friends, user1.Id.ToString());
                            user2.Subscriptions = AddToList(user2.Subscriptions, user1.Id.ToString());
                            user1.Subscribers = AddToList(user1.Subscribers, unfr.ToString());
                            db.Users.Update(user1);
                            db.Users.Update(user2);
                            db.SaveChanges();
                            return RedirectToAction("Prof", "Account", new { UserId = unfr });
                        }
                    }
                }
            }
            return RedirectToAction("Prof", "Account", new { UserId = fr });
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string login, string password)
        {
            if (ModelState.IsValid)
            {
                User user = await db.Users.FirstOrDefaultAsync(u => u.Login == login && u.Password == password);
                if (user != null)
                {
                    await Authenticate(login); // аутентификация
                    return RedirectToAction("Prof", "Account");
                }
                ModelState.AddModelError("", "Incorrect username or password");

            }
            ViewData["Message"] = "Incorrect username or password";
            return View();

        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string name, string surname, DateTime date, string login, string password)
        {
            User user = await db.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (user == null)
            {
                if (name != null && surname != null && password != null && login != null && DateTime.Now.Year - date.Year > 5 && DateTime.Now.Year - date.Year < 150)
                {
                    User NewUser = new User();
                    NewUser.Login = login;
                    NewUser.Password = password;
                    NewUser.Name = name;
                    NewUser.Surname = surname;
                    NewUser.Birthday = date;
                    db.Users.Add(NewUser);
                    db.SaveChanges();
                    return RedirectToAction("Login", "Account");
                }
                else
                    ViewData["Message"] = "Fill in all the fields correctly";
            }
            else
                ViewData["Message"] = "This login is used by another user";
            return View();
        }

        private async Task Authenticate(string userName)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Chats()
        {
            string login = User.Identity.Name;
            List<ViewChat> chats = new List<ViewChat>();
            string newListChats = "";

            User user = await db.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (user.Chats != null && user.Chats != "")
            {
                var ChatsId = user.Chats.Split(' ');
                foreach (var i in ChatsId)
                {
                    if (i != "")
                    {
                        int id = Int32.Parse(i);
                        Chat chat = await db.Chats.FirstOrDefaultAsync(c => c.Id == id);
                        if (chat != null && chat.Messages != null && chat.Messages.Count > 0)
                        {
                            ViewChat viewChat = new ViewChat() { Id = chat.Id };
                            viewChat.LastMessage = (chat.Messages as List<Message>).ElementAt(chat.Messages.Count-1);
                            var UserIds = chat.Name.Split(' ');
                            foreach (var UserId in UserIds)
                            {
                                int UsId = Int32.Parse(UserId);
                                if (UsId != user.Id)
                                {
                                    User UChat = await db.Users.FirstOrDefaultAsync(u => u.Id == UsId);
                                    viewChat.Name = UChat.Name + ' ' + UChat.Surname;
                                    if (UChat.Image != null)
                                        viewChat.Image = UChat.Image;
                                }
                            }
                            chats.Add(viewChat);
                            newListChats = newListChats + ' ' + i;
                        }
                        else if (chat != null)
                        {
                            db.Chats.Remove(chat);
                        }
                    }
                }
                user.Chats = newListChats;
                db.Users.Update(user);
                db.SaveChanges();
                chats.Sort((a, b) => { if (a.LastMessage.Date < b.LastMessage.Date) return 1; if (a.LastMessage.Date > b.LastMessage.Date) return -1; else return 0; } );

            }
            return View(chats);
        }

        [HttpPost]
        public IActionResult Chats(string ChatId, string Name)
        {
            if (Name != null)
                return RedirectToAction("People", "Account", new { users = Name });
            return RedirectToAction("Messages", "Account", new { chatId = ChatId });
        }

        public List<User> Header(string Name)
        {

            var Users = new List<User>();
            if (Name != null && Name != "")
            {
                var splName = Name.Split();
                foreach (var i in db.Users)
                {
                    if (i.Name.ToLower().IndexOf(splName[0].ToLower()) != -1 && (splName.Length < 2 || i.Surname.ToLower().IndexOf(splName[1].ToLower()) != -1))
                    {
                        Users.Add(i);
                    }
                }
            }
            return Users;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Messages(string chatId)
        {
            ViewData["Message"] = chatId;
            if (chatId != null)
            {
                int id = Int32.Parse(chatId);
                Chat chat = await db.Chats.FirstOrDefaultAsync(c => c.Id == id);
                var list = new List<ViewMessage>();
                var userList = new List<User>();
                var Users = chat.Name.Split(' ');
                foreach(var i in Users)
                {
                    if(i!="")
                    {
                        var Id = Int32.Parse(i);
                        userList.Add(await db.Users.FirstOrDefaultAsync(u => u.Id == Id));
                    }
                }
                if (chat.Messages != null && chat.Messages.Count > 0)
                {
                    foreach (var i in chat.Messages)
                    {
                        var mes = new ViewMessage() { Date = i.Date, Text = i.Text };
                        foreach(var user in userList)
                        {
                            if (user.Id==i.IdUser)
                            {
                                mes.Image = user.Image;
                                if (user.Login == User.Identity.Name)
                                    mes.My = true;
                                else
                                    mes.My = false;
                            mes.UserName = user.Name + ' ' + user.Surname;
                            }
                        }
                        list.Add(mes);
                    }
                }
                return View(list);
            }
            else
            {
                return RedirectToAction("Prof", "Account");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Messages(string message, string ChatId, string Name)
        {
            if (Name != null)
                return RedirectToAction("People", "Account", new { users = Name });
            if (message != null && message != "")
            {
                Message newMess = new Message();
                newMess.Text = message;
                Chat chat = await db.Chats.FirstOrDefaultAsync(u => u.Id == Int32.Parse(ChatId));
                newMess.Chat = chat;
                newMess.Date = DateTime.Now;
                User user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
                newMess.IdUser = user.Id;
                db.Messages.Add(newMess);
                db.SaveChanges();
            }
            return RedirectToAction("Messages", "Account", new { chatId = ChatId });
        }

        [HttpGet]
        public IActionResult People(string users)
        {
            var List = Header(users);
            return View(List);
        }

        [HttpPost]
        public IActionResult People(int chatId, string Name)
        {
            if (Name != null)
                return RedirectToAction("People", "Account", new { users = Name });
            return RedirectToAction("Prof", "Account", new { UserId = chatId });

        }

        string RemoveFromList(string List, string Id)
        {
            string newList = null; ;
            if (List != null)
            {
                var SplList = List.Split(' ');
                int ParseId = Int32.Parse(Id);
                foreach (var id in SplList)
                {
                    if (id != "")
                    {
                        int sId = Int32.Parse(id);
                        if (ParseId != sId)
                        {
                            if (newList == null)
                                newList = id;
                            else
                                newList += " " + id;
                        }
                    }
                }
            }
            return newList;
        }

        string AddToList(string List, string Id)
        {
            if (List == null)
                List = Id;
            else
                List += " " + Id;
            return List;
        }

        async Task<List<User>> GetListAsync(string List)
        {
            var stList = new List<SocialWeb2.User>();
            if (List != null)
            {
                var listId = List.Split(' ');
                foreach (var i in listId)
                {
                    if (i != "")
                    {
                        int id = Int32.Parse(i);
                        stList.Add(await db.Users.FirstOrDefaultAsync(u => u.Id == id));
                    }
                }
            }
            return stList;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Friends(int? id, int? ListType)
        {
            List<User> users = new List<User>();
            User user;
            if (id != null)
                user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            else
                user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (ListType == null || ListType == 1)
            {
                ViewData["Message"] = "Friends List";
                return View(GetListAsync(user.Friends).Result);
            }
            else if (ListType == 2)
            {
                ViewData["Message"] = "Subscribers List";
                return View(GetListAsync(user.Subscribers).Result);
            }
            else
            {
                ViewData["Message"] = "Subscriptions List";
                return View(GetListAsync(user.Subscriptions).Result);
            }
        }

        [HttpPost]
        public IActionResult Friends(string frId, string Name)
        {
            if (Name != null)
                return RedirectToAction("People", "Account", new { users = Name });
            return RedirectToAction("Prof", "Account", new { UserId = frId });
        }
        public static string cut(string str, int size)
        {
            if (size < str.Length)
            {
                string res = "";
                for (int i = 0; i < size; i++)
                {
                    res += str[i];
                }
                return res;
            }
            return str;
        }

        [HttpGet]
        public async Task<IActionResult> Setting()
        {
            User user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            var list = new List<User>() { user };
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Setting(string uname, string surname, DateTime date, string login, string password, IFormFile file,string Name)
        {
            if (Name != null)
                return RedirectToAction("People", "Account", new { users = Name });
            User user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (await db.Users.FirstOrDefaultAsync(u => u.Login == login) == null||login==User.Identity.Name)
            {
                var dir = env.ContentRootPath+@"\\wwwroot\\ProfImag";
                if (file != null)
                {
                    using (var fileStream = new FileStream(Path.Combine(dir, user.Id.ToString() + ".png"), FileMode.Create, FileAccess.Write))
                    {
                        file.CopyTo(fileStream);
                    }
                }
                if (uname != null && uname != "")
                    user.Name = uname;
                if (surname != null && surname != "")
                    user.Surname = surname;
                if (DateTime.Now.Year - date.Year > 5 && DateTime.Now.Year - date.Year < 150)
                    user.Birthday = date;
                if (password != null)
                    user.Password = password;
                if (file != null)
                    user.Image = @"/ProfImag/" + user.Id.ToString() + ".png";
                db.Update(user);
                db.SaveChanges();
            }
            else
            {
                ViewData["Message"] = "This login is used by another user";
                
            }
            return View(new List<User>() { user });

        }
    }

}
