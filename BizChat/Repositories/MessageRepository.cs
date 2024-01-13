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
			var msgList = dbContext.Messages.Where(m => m.ChannelId == id).ToList();
			if (msgList.Count > 0)
			{
				foreach (var emp in msgList)
				{
					dbContext.Entry(emp).Reload();
				}
			}

			var lst = from m in msgList
					  where m.ChannelId == id
					  select new
					  {
						  m.User!.UserName,
						  m.Content,
						  m.Date,
						  m.ChannelId,
						  m.UserId,
						  m.Id
					  };

			return lst.ToList();
		}
	}
}
