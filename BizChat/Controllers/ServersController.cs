using BizChat.Data;
using BizChat.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace BizChat.Controllers
{
    public class ServersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ServersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Un user poate nu vrea sa aiba propriul server, zic sa renuntam la asta
        private void AddDefaultServer()
        {
            string UserId = _userManager.GetUserId(User);
            ApplicationUser currentUser = db.Users.Find(UserId)!;

            Server server = new Server();
            server.Name = currentUser.UserName + " Server";
            db.Servers.Add(server);

            ServerUser serverUser = new ServerUser();
            serverUser.User = currentUser;
            serverUser.UserId = UserId;
            serverUser.Server = server;
            serverUser.ServerId = server.Id;
            db.ServerUsers.Add(serverUser);

            db.SaveChanges();
        }

        public IActionResult Index(int? serverId, int? channelId)
        {
			string UserId = _userManager.GetUserId(User); 
			var servers = db.Servers.Where(s => s.Users.Where(su => su.UserId == UserId).Count() > 0);
			ViewBag.Servers = servers;
            if (serverId != null)
            {
				ViewBag.ServerId = serverId;

                // retrieve all channels
				var channels = db.Channels.Where(channel => channel.ServerId == serverId);
				ViewBag.Channels = channels;

                // retrieve all server members
                var servermembers = db.ServerUsers.Where(su => su.ServerId == serverId);
				ViewBag.ServerMembers = db.ApplicationUsers.
										Where(user => servermembers.Where(sm => sm.UserId == user.Id).First() != null);

                // retrieve all categories of the server
				IQueryable<Category>? categories = db.Categories.Where(c => c.ServerId == serverId);
				ViewBag.Categories = categories;

                if (channelId != null)
                {
                    Channel? selectedChannel = db.Channels.Find(channelId);
                    ViewBag.selectedChannel = selectedChannel;
                }
			}
            return View();
        }

    }
}
