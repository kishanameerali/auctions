using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using auctions.Models;
using System.Linq;

namespace auctions.Controllers
{
    public class HomeController : Controller
    {
        private AuctionContext _context;

        public HomeController(AuctionContext context)
        {
            _context = context;
        }

        private bool UserExists(string username)
        {
            List<User> ReturnedUsers = _context.users.Where(user => user.username == username).ToList();
            return ReturnedUsers.Count > 0;
        }
        // GET: /Home/
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("create")]
        public IActionResult Create(RegisterViewModel userdata)
        {
            if(UserExists(userdata.username))
            {
                ModelState.AddModelError("username", "That Username has already been registered");
            }
            if(ModelState.IsValid)
            {
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                User NewUser = new User();
                {
                    NewUser.first_name = userdata.first_name;
                    NewUser.last_name = userdata.last_name;
                    NewUser.username = userdata.username;
                    NewUser.password = Hasher.HashPassword(NewUser, userdata.password);
                    NewUser.wallet = 1000;
                    _context.users.Add(NewUser);
                    _context.SaveChanges();
                    HttpContext.Session.SetInt32("user_id", NewUser.userid);
                    return RedirectToAction("Dashboard", "Auction");
                };
            }
            return View("Index");
        }

        [HttpPost]
        [Route("signin")]
        public IActionResult Signin(LoginViewModel info)
        {
            User LoggedUser = _context.users.SingleOrDefault(l => l.username == info.logusername);
            PasswordHasher<User> Hasher = new PasswordHasher<User>();

            if(!UserExists(info.logusername) || Hasher.VerifyHashedPassword(LoggedUser, LoggedUser.password, info.logpassword) == 0)
            {
                ModelState.AddModelError("logusername", "Username or Password was incorrect");
            }
            if(ModelState.IsValid)
            {
                HttpContext.Session.SetInt32("user_id", LoggedUser.userid);
                return RedirectToAction("Dashboard", "Auction");
            }
            return View("Index");
        }
        
        [Route("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

    }
}
