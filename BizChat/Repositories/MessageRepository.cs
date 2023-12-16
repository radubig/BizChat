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

		public struct MessageComponents
		{
			string UserName;
			string Content;
			DateTime Date;
		}

		public object GetMessages()
		{
			var lst = from m in dbContext.Messages
					  select new
					  {
						  m.User!.UserName,
						  m.Content,
						  m.Date
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
