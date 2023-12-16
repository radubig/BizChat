using BizChat.Hubs;
using BizChat.Models;
using TableDependency.SqlClient;

namespace BizChat.SubscribeTableDependencies
{
	public class SubscribeMessageTableDependency : ISubscribeTableDependency
	{
		SqlTableDependency<Message> tableDependency;
		SignalRHub hub;

		public SubscribeMessageTableDependency(SignalRHub _hub)
		{
			hub = _hub;
		}

		public void SubscribeTableDependency(string connectionString)
		{
			tableDependency = new SqlTableDependency<Message>(connectionString, "Messages");
			tableDependency.OnChanged += TableDependency_OnChanged;
			tableDependency.OnError += TableDependency_OnError;
			tableDependency.Start();
		}

		private async void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<Message> e)
		{
			if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
			{
				await hub.SendMessages();
			}
		}

		private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
		{
			Console.WriteLine($"{nameof(Message)} SqlTableDependency error: {e.Error.Message}");
		}
	}
}
