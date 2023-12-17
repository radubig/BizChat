using BizChat.Data;
using BizChat.Models;

namespace BizChat.Repositories
{
	public class MessageRepository
	{
		string connectionString;
		private readonly ApplicationDbContext dbContext;

		public MessageRepository(string connectionString, ApplicationDbContext _dbContext)
		{
			this.connectionString = connectionString;
			this.dbContext = _dbContext;
		}

		public object GetMessages(int id)
		{
			var lst = from m in dbContext.Messages
					  where m.ChannelId == id
					  select new
					  {
						  m.User!.UserName,
						  m.Content,
						  m.Date,
						  m.ChannelId
					  };

			var msgList = dbContext.Messages.ToList();
			foreach (var emp in msgList)
			{
				dbContext.Entry(emp).Reload();
			}

			//var f = dbContext.Product.ToList();
			return lst.ToList();
		}
	}
}
