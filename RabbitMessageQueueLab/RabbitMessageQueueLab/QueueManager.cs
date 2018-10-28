using RabbitMQ.Client;
using System.Collections.Generic;
using System.Linq;

namespace RabbitMessageQueueLab
{
	public class RegistrationResult
	{
		public IModel Model;
		public IConnection Conn;
		public string SendExchange;
		public string SendKey;
		public string[] RecvQueue;
	}


	public class QueueManager
	{
		public static RegistrationResult RegisterDirect(int nofQueues)
		{
			const string queueName = "helloqueue";

			var factory = new ConnectionFactory()
			{
				HostName = "localhost",
				UserName = "guest",
				Password = "guest"
			};

			var connection = factory.CreateConnection();
			var model = connection.CreateModel();

			model.QueueDeclare(queueName, true, false, false, null);

			var queueArr = Enumerable.Repeat(queueName, nofQueues).ToArray();

			return new RegistrationResult
			{
				Model = model,
				Conn = connection,
				SendExchange = "",
				SendKey = queueName,
				RecvQueue = queueArr
			};
		}


		public static RegistrationResult RegisterFan(int nofQueues)
		{
			const string exch = "exchfan";

			var factory = new ConnectionFactory()
			{
				HostName = "localhost",
				UserName = "guest",
				Password = "guest"
			};

			var connection = factory.CreateConnection();
			var model = connection.CreateModel();

			model.ExchangeDeclare(exch, ExchangeType.Fanout, true, false, null);
			var queueList = new List<string>();

			for (var id = 1; id <= nofQueues; id++)
			{
				//var queueName = string.Format(queuePrefix, id);
				//model.QueueDeclare(queueName, true, false, false, null);
				var queueName = model.QueueDeclare().QueueName;
				model.QueueBind(queueName, exch, "");
				queueList.Add(queueName);
			}

			return new RegistrationResult
			{
				Model = model,
				Conn = connection,
				SendExchange = exch,
				SendKey = "",
				RecvQueue = queueList.ToArray()
			};
		}
	}
}
