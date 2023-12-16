﻿using BizChat.Data;
using BizChat.Models;
using BizChat.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace BizChat.Hubs
{
    public class SignalRHub : Hub
    {
        MessageRepository messageRepository;
        private readonly ApplicationDbContext dbContext;

        public SignalRHub(IConfiguration configuration, ApplicationDbContext _dbContext)
        {
            dbContext = _dbContext;
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            messageRepository = new MessageRepository(connectionString, dbContext);
        }

        public async Task SendMessages()
        {
            var messages = messageRepository.GetMessages();
            await Clients.All.SendAsync("ReceivedMessages", messages);
        }
    }
}
