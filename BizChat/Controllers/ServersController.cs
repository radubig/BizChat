using BizChat.Data;
using BizChat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using NuGet.Common;

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
				ViewBag.IsOwner = db.ServerUsers.Where(su => su.ServerId == serverId && su.UserId == _userManager.GetUserId(User)).First().IsOwner;
				ViewBag.UserRole = db.Roles.Find(db.UserRoles.Where(u => u.UserId == _userManager.GetUserId(User)).First().RoleId).Name;
				// in view verificam ViewBag.IsModerator == true
			}
			return View();
		}

		[HttpPost]
		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult Index([FromForm] Category category, int? serverId, int? channelId, int? e_categoryId)
		{
			string UserId = _userManager.GetUserId(User);
			var servers = db.Servers.Where(s => s.Users.Where(su => su.UserId == UserId).Count() > 0);
			ViewBag.Servers = servers;
			if (serverId != null && _signInManager.IsSignedIn(User))
			{
				ViewBag.ServerId = serverId;

				var channels = db.Channels.Where(channel => channel.ServerId == serverId);
				ViewBag.Channels = channels;

				var servermembers = db.ServerUsers.Where(su => su.ServerId == serverId);
				ViewBag.ServerMembers = db.ApplicationUsers.
										Where(user => servermembers.Where(sm => sm.UserId == user.Id).First() != null);

				IQueryable<Category>? categories = db.Categories.Where(c => c.ServerId == serverId);
				ViewBag.Categories = categories;

				if (channelId != null)
				{
					Channel? selectedChannel = db.Channels.Find(channelId);
					ViewBag.selectedChannel = selectedChannel;
				}
				var is_user_mod = db.ServerUsers.Where(su => su.ServerId == serverId && su.UserId == _userManager.GetUserId(User)).First().IsModerator;
				ViewBag.IsModerator = is_user_mod;
				ViewBag.IsOwner = db.ServerUsers.Where(su => su.ServerId == serverId && su.UserId == _userManager.GetUserId(User)).First().IsOwner;
				ViewBag.UserRole = db.Roles.Find(db.UserRoles.Where(u => u.UserId == _userManager.GetUserId(User)).First().RoleId).Name;

				if (is_user_mod == true)
				{
					if (e_categoryId != null && ModelState.IsValid)
					{
						Category category_to_edit = db.Categories.Find(e_categoryId);
						category_to_edit.CategoryName = category.CategoryName;
						db.SaveChanges();
					}
				}
			}
			return RedirectToAction(actionName: "Index", controllerName: "Servers", routeValues: new { serverId, channelId });
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

		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult EditServer(int serverId)
		{
			if (db.ServerUsers.Where(su => su.UserId == _userManager.GetUserId(User) && su.ServerId == serverId).First().IsOwner == true)
			{
				Server server = db.Servers.Find(serverId);
				return View(server);
			}
			else
			{
				return RedirectToAction("Index");
			}
		}

		[HttpPost]
		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult EditServer(Server newserver)
		{
			Server server = db.Servers.Find(newserver.Id);
			if (ModelState.IsValid)
			{
				if (db.ServerUsers.Where(su => su.UserId == _userManager.GetUserId(User) && su.ServerId == server.Id).First().IsOwner == true)
				{
					server.Name = newserver.Name;
					server.Description = newserver.Description;
					db.SaveChanges();
				}
				return RedirectToAction(actionName: "Index", controllerName: "Servers", routeValues: new { serverId = server.Id });
			}
			else
			{
				return View(server);
			}
		}

		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
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
				return RedirectToAction(actionName: "Index", routeValues: new { serverId = category.ServerId });
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
			channel.ServerId = db.Categories.Find(categoryId)!.ServerId;
			channel.CategoryId = categoryId;
			return View(channel);
		}
		[HttpPost]
		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult NewChannel(Channel channel)
		{
			if (ModelState.IsValid)
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

		[HttpPost]
		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult EditChannel([FromForm]Channel channel)
		{
			if(ModelState.IsValid)
			{
				db.Channels.Find(channel.Id).Name = channel.Name;
				db.Channels.Find(channel.Id).Description= channel.Description;
				db.SaveChanges();
			}
			return RedirectToAction(actionName: "Index", routeValues: new { serverId = channel.ServerId, channelId = channel.Id });
		}

		[NonAction]
		public void DeleteChannelMessages(int channelId)
		{
			db.Messages.RemoveRange(db.Messages.Where(m => m.ChannelId == channelId));
		}

		[HttpPost]
		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult DeleteServer(int serverId)
		{
			List<string> high_role_ids = (from r in db.Roles where r.Name != "RegisteredUser" select r.Id).ToList();
			// Delete Server with the serverId
			var user_id = _userManager.GetUserId(User);
			var user_role_in_server = db.ServerUsers.Where(su => su.UserId == user_id && su.ServerId == serverId).First();
			if (user_role_in_server.IsOwner == true || db.UserRoles.Where(ur => ur.UserId == user_id && high_role_ids.Contains<string>(ur.RoleId)).First() != null)
			{
				Console.WriteLine("~~~~~~~~~ Delete server with ID " + serverId);
				db.Servers.Remove(db.Servers.Find(serverId)!);
				/* Partea asta ar trebui in mod normal sa se stearga in cascada
				var s_channels = db.Channels.Where(c => c.ServerId == serverId);
				foreach (var channel in s_channels)
				{
					DeleteChannelMessages(channel.Id);
				}
				*/
				db.SaveChanges();
			}
			return RedirectToAction(controllerName: "Servers", actionName: "Index");
		}

		[HttpPost]
		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult CreateMessage(string content, string channelId)
		{
			Message message = new Message();
			message.Content = content;
			message.ChannelId = Convert.ToInt32(channelId);
			message.Date = DateTime.Now;
			message.UserId = _userManager.GetUserId(User);
			db.Messages.Add(message);
			db.SaveChanges();
			return Json(new { Message = "Success" });
		}
	}
}
