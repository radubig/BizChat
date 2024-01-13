using BizChat.Data;
using BizChat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using NuGet.Common;
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

		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult SearchServers()
		{
			var search = "";
			List<int> serverIds = new List<int>();
			if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
			{
				search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();
				serverIds = (from s in db.Servers where (s.Name.Contains(search) || s.Description.Contains(search)) select s.Id).ToList(); 
			}
			var UserId = _userManager.GetUserId(User);
			List<int?> JoinedServerIds = (from su in db.ServerUsers where su.UserId == UserId select su.ServerId).ToList(); // id-urile serverelor in care ne aflam
			if(serverIds.Any())ViewBag.AllServers = db.Servers.Where(s => JoinedServerIds.Contains(s.Id) == false && serverIds.Contains(s.Id));
			else
			{
				ViewBag.AllServers = db.Servers.Where(s => JoinedServerIds.Contains(s.Id) == false);
			}
			ViewBag.UId = UserId;
			return View();
		}

		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult JoinRequest(string UId, int SId)
		{
			if (db.ServerUsers.Where(su => su.UserId == UId && su.ServerId == SId).Count() == 0) // ce cauti aici daca esti deja in server
			{
				ServerUser serverUser = new ServerUser();
				serverUser.UserId = UId;
				serverUser.ServerId = SId;
				List<string> modroles = (from r in db.Roles where r.Name == "AppModerator" || r.Name == "AppAdmin" select r.Id).ToList();
				if (modroles.Contains(db.UserRoles.Where(ur => ur.UserId == UId).First().RoleId) == false) serverUser.IsWaiting = true; // daca e mod/admin nu trebuie aprobat
				db.ServerUsers.Add(serverUser);
				db.SaveChanges();
			}
			return RedirectToAction("SearchServers");
		}

		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult ManageServerUsers(int serverId)
		{
			if (db.ServerUsers.Where(su => su.ServerId == serverId && su.UserId == _userManager.GetUserId(User)).Count() > 0)
			{
				ViewBag.IsModerator = db.ServerUsers.Where(su => su.ServerId == serverId && su.UserId == _userManager.GetUserId(User)).First().IsModerator;
				ViewBag.IsOwner = db.ServerUsers.Where(su => su.ServerId == serverId && su.UserId == _userManager.GetUserId(User)).First().IsOwner;
				ViewBag.serverId = serverId;
				ViewBag.Users = db.ServerUsers.Where(su => su.ServerId == serverId).Join(db.ApplicationUsers, su => su.UserId, au => au.Id,
												(su, au) => new { su.UserId, su.IsWaiting, su.IsModerator, su.IsOwner, au.UserName });
				return View();
			}
			return RedirectToAction("Index");
		}

		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult RequestResponse(int serverId, string userId, bool response)
		{
			ServerUser req_su = db.ServerUsers.Where(su => su.ServerId == serverId && su.UserId == userId).First();
			if (response == true)
			{
				req_su.IsWaiting = false;
			}
			else
			{
				db.Remove(req_su);
			}
			db.SaveChanges();
			return RedirectToAction(actionName: "ManageServerUsers", routeValues: new { serverId });
		}

		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult KickFromServer(int serverId, string userId)
		{
			db.Remove(db.ServerUsers.Where(su => su.ServerId == serverId && su.UserId == userId).First());
			db.SaveChanges();
			return RedirectToAction(actionName: "ManageServerUsers", routeValues: new { serverId });
		}

		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult ChangeMod(int serverId, string userId)
		{
			ServerUser su = db.ServerUsers.Where(su => su.ServerId == serverId && su.UserId == userId).First();
			su.IsModerator = su.IsModerator == true ? false : true;
			db.SaveChanges();
			return RedirectToAction(actionName: "ManageServerUsers", routeValues: new { serverId });
		}

		[Authorize(Roles = "AppAdmin")]
		public IActionResult ManageAllUsers()
		{
			ViewBag.AllUsers = db.Users.Join(db.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u.Id, u.UserName, ur.RoleId })
										.Join(db.Roles, uur => uur.RoleId, r => r.Id, (uur, r) => new { uur.Id, uur.UserName, RoleName = r.Name });
			return View();
		}

		[Authorize(Roles = "AppAdmin")]
		public IActionResult ChangeAppMod(string userId)
		{
			var Role = db.Roles.Where(r => r.Id == (db.UserRoles.Where(ur => ur.UserId == userId).First().RoleId)).First().Name;
			if (Role == "AppModerator")
			{
				db.UserRoles.Remove(db.UserRoles.Where(ur => ur.UserId == userId).First());
				db.UserRoles.Add(new IdentityUserRole<string> { UserId = userId, RoleId = db.Roles.Where(r => r.Name == "RegisteredUser").First().Id });
			}
			else
			{
				if (Role == "RegisteredUser")
				{
					db.UserRoles.Remove(db.UserRoles.Where(ur => ur.UserId == userId).First());
					db.UserRoles.Add(new IdentityUserRole<string> { UserId = userId, RoleId = db.Roles.Where(r => r.Name == "AppModerator").First().Id });
				}
			}
			db.SaveChanges();
			return RedirectToAction("ManageAllUsers");
		}

		public IActionResult Index(int? serverId, int? channelId)
		{
			string UserId = _userManager.GetUserId(User);
			var servers = db.Servers.Where(s => s.Users.Where(su => su.UserId == UserId && su.IsWaiting == false).Count() > 0);
			ViewBag.Servers = servers;
			if (db.ServerUsers.Where(su => su.UserId == UserId && su.ServerId == serverId).Count() > 0)
			{
				if (serverId != null && _signInManager.IsSignedIn(User) && db.ServerUsers.Where(su => su.UserId == UserId && su.ServerId == serverId).First().IsWaiting == false)
				{
					ViewBag.ServerId = serverId;

					// retrieve all channels
					var channels = db.Channels.Where(channel => channel.ServerId == serverId);
					ViewBag.Channels = channels;

					// retrieve all server members
					var servermembers = db.ServerUsers.Where(su => su.ServerId == serverId && su.IsWaiting == false);
					var waitingservermembers = db.ServerUsers.Where(su => su.ServerId == serverId && su.IsWaiting == true);
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
			}
			return View();
		}

		[HttpPost]
		public IActionResult UserInformation(string channelId)
		{
			//Console.WriteLine(Convert.ToInt32(channelId) * 100);
			var serverId = db.Channels.Find(Convert.ToInt32(channelId)).ServerId;
			var isModerator = db.ServerUsers.Where(su => su.ServerId == serverId && su.UserId == _userManager.GetUserId(User)).First().IsModerator;
			return Json(new { isMod = isModerator, userId = _userManager.GetUserId(User) });
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
			if (IsModerator == true && db.Servers.Find(serverId) != null)
			{
				ViewBag.ServerId = serverId;
				Category category = new Category();
				category.ServerId = serverId;
				ViewBag.ServerName = db.Servers.Find(serverId).Name;
				return View(category);
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult NewCategory(Category category, int serverId)
		{
			if (ModelState.IsValid)
			{
				db.Categories.Add(category);
				db.SaveChanges();
				return RedirectToAction(actionName: "Index", routeValues: new { serverId = category.ServerId });
			}
			else
			{
				ViewBag.ServerName = db.Servers.Find(serverId).Name;
				return View(category);
			}
		}

		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult NewChannel(int categoryId, int serverId)
		{
			Channel channel = new Channel();
			var IsModerator = db.ServerUsers.Where(su => su.ServerId == serverId && su.UserId == _userManager.GetUserId(User)).First().IsModerator;
			if (IsModerator == true)
			{
				channel.ServerId = db.Categories.Find(categoryId)!.ServerId;
				channel.CategoryId = categoryId;
				ViewBag.ServerName = db.Servers.Find(serverId).Name;
				ViewBag.CategoryName = db.Categories.Find(categoryId).CategoryName;
				return View(channel);
			}
			return RedirectToAction("Index");
		}
		[HttpPost]
		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult NewChannel(Channel channel, int categoryId, int serverId)
		{
			if (ModelState.IsValid)
			{
				db.Add(channel);
				db.SaveChanges();
				return RedirectToAction(actionName: "Index", routeValues: new { serverId = channel.ServerId });
			}
			else
			{
				ViewBag.ServerName = db.Servers.Find(serverId).Name;
				ViewBag.CategoryName = db.Categories.Find(categoryId).CategoryName;
				return View(channel);
			}
		}

		[HttpPost]
		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult EditChannel([FromForm] Channel channel)
		{
			if (ModelState.IsValid)
			{
				db.Channels.Find(channel.Id)!.Name = channel.Name;
				db.Channels.Find(channel.Id)!.Description = channel.Description;
				db.SaveChanges();
			}
			return RedirectToAction(actionName: "Index", routeValues: new { serverId = channel.ServerId, channelId = channel.Id });
		}

		[HttpPost]
		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult DeleteServer(int serverId)
		{
			List<string> high_role_ids = (from r in db.Roles where r.Name != "RegisteredUser" select r.Id).ToList();
			var user_id = _userManager.GetUserId(User);
			var user_role_in_server = db.ServerUsers.Where(su => su.UserId == user_id && su.ServerId == serverId).First();
			if (user_role_in_server.IsOwner == true || db.UserRoles.Where(ur => ur.UserId == user_id && high_role_ids.Contains<string>(ur.RoleId)).First() != null)
			{
				db.Servers.Remove(db.Servers.Find(serverId)!);
				db.SaveChanges();
			}
			return RedirectToAction(controllerName: "Servers", actionName: "Index");
		}

		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult DeleteCategory(int categoryId)
		{
			Category category = db.Categories.Find(categoryId)!;
			int? _serverId = category.ServerId;

			// Validarea este preluata de la DeleteServer()
			List<string> high_role_ids = (from r in db.Roles where r.Name != "RegisteredUser" select r.Id).ToList();
			var user_id = _userManager.GetUserId(User);
			var user_role_in_server = db.ServerUsers.Where(su => su.UserId == user_id && su.ServerId == _serverId).First();
			if (user_role_in_server.IsOwner == true || db.UserRoles.Where(ur => ur.UserId == user_id && high_role_ids.Contains<string>(ur.RoleId)).First() != null)
			{
				db.Categories.Remove(category);
				db.SaveChanges();
			}

			return RedirectToAction(controllerName: "Servers", actionName: "Index", routeValues: new { serverId = _serverId });
		}

		[Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
		public IActionResult DeleteChannel(int channelId)
		{
			Channel channel = db.Channels.Find(channelId)!;
			int? _serverId = channel.ServerId;


			// Validarea este preluata de la DeleteServer()
			List<string> high_role_ids = (from r in db.Roles where r.Name != "RegisteredUser" select r.Id).ToList();
			var user_id = _userManager.GetUserId(User);
			var user_role_in_server = db.ServerUsers.Where(su => su.UserId == user_id && su.ServerId == _serverId).First();
			if (user_role_in_server.IsOwner == true || db.UserRoles.Where(ur => ur.UserId == user_id && high_role_ids.Contains<string>(ur.RoleId)).First() != null)
			{
				db.Channels.Remove(channel);
				db.SaveChanges();
			}

			return RedirectToAction(controllerName: "Servers", actionName: "Index", routeValues: new { serverId = _serverId });
		}
	}
}
