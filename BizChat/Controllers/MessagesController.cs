﻿using BizChat.Data;
using BizChat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace BizChat.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public MessagesController(
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

        [HttpPost]
        [Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
        public IActionResult CreateMessage(string content, string channelId)
        {
            if (string.IsNullOrEmpty(content))
                return Json(new { Message = "Meh" });
			Message message = new Message();
            message.Content = content;
            message.ChannelId = Convert.ToInt32(channelId);
            message.Date = DateTime.Now;
            message.UserId = _userManager.GetUserId(User);
			db.Messages.Add(message);
            db.SaveChanges();
            return Json(new { Message = "Success" });
        }

        [HttpPost]
        [Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
        public IActionResult EditMessage(string content, string messageId)
        {
			if (string.IsNullOrEmpty(content))
                return Json(new { Message = "Meh" });
            Message message = db.Messages.Find(Convert.ToInt32(messageId))!;
            if (message.UserId == _userManager.GetUserId(User))
            {
                message.Content = content;
                message.Date = DateTime.Now;
				db.SaveChanges();
			}
            return Json(new { Message = "Success" });
        }

        [HttpPost]
        [Authorize(Roles = "RegisteredUser, AppModerator, AppAdmin")]
        public IActionResult DeleteMessage(string messageId)
        {
            int id = Convert.ToInt32(messageId);
            Console.WriteLine("Am sters mesajul " + messageId);
            Message message = db.Messages.Find(id)!;
			var IsModerator = db.ServerUsers.Where(su => su.ServerId == db.Channels.Find(message.ChannelId).ServerId 
                                && su.UserId == _userManager.GetUserId(User)).First().IsModerator;
			if (IsModerator == true || message.UserId == _userManager.GetUserId(User))
            {
                db.Messages.Remove(message);
                db.SaveChanges();
            }
            return Json(new { Message = "Success" });
        }
    }
}
