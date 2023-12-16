using BizChat.Data;
using BizChat.Models;
using Microsoft.AspNetCore.Authorization;
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
		private readonly SignInManager<ApplicationUser> _signInManager;

		public ServersController(
			ApplicationDbContext context,
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			SignInManager<ApplicationUser> signInManager)
		{
			db = context;
			_userManager = userManager;
			_roleManager = roleManager;
			_signInManager = signInManager;
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
			if (serverId != null && _signInManager.IsSignedIn(User))
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
				// cam asa ar trebui sa facem verificarile
				ViewBag.IsModerator = db.ServerUsers.Where(su => su.ServerId == serverId && su.UserId == _userManager.GetUserId(User)).First().IsModerator;
				// in view verificam ViewBag.IsModerator == true
			}
			return View();
		}

		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult NewServer()
		{
			Server server = new Server();
			return View(server);
		}

		[HttpPost]
		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult NewServer(Server server)
		{

			if (ModelState.IsValid)
			{
				db.Servers.Add(server);
				db.SaveChanges();
				ServerUser serverUser = new ServerUser();
				serverUser.UserId = _userManager.GetUserId(User);
				serverUser.ServerId = server.Id;
				serverUser.IsModerator = true;
				serverUser.IsOwner = true;
				db.ServerUsers.Add(serverUser);
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			else
			{
				return View(server);
			}
		}

		[Authorize(Roles ="RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult NewCategory(int serverId)
		{
			Console.WriteLine(serverId);
			Console.WriteLine(_userManager.GetUserId(User));
			var IsModerator = db.ServerUsers.Where(su => su.ServerId == serverId && su.UserId == _userManager.GetUserId(User)).First().IsModerator;
			if (IsModerator == true)
			{
				ViewBag.ServerId = serverId;
				Category category = new Category();
				category.ServerId = serverId;
				return View(category);
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult NewCategory(Category category)
		{
			if (ModelState.IsValid)
			{
				db.Categories.Add(category);
				db.SaveChanges();
				return RedirectToAction(actionName: "Index", routeValues: new { serverId = category.ServerId});
			}
			else
			{
				return View(category);
			}
		}

		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult NewChannel(int categoryId)
		{
			Channel channel = new Channel();
			Console.WriteLine(categoryId);
			channel.ServerId = db.Categories.Find(categoryId).ServerId;
			channel.CategoryId = categoryId;
			return View(channel);
		}
		[HttpPost]
		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult NewChannel(Channel channel)
		{
			if(ModelState.IsValid)
			{
				db.Add(channel);
				db.SaveChanges();
				return RedirectToAction(actionName: "Index", routeValues: new { serverId = channel.ServerId });
			}
			else
			{
				return View(channel);
			}
		}
	}
}
