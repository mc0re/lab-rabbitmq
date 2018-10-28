using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RabbitRequeueLab
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var conn = CreateConnection())
			{
				using (var model = conn.CreateModel())
				{
					var exch = CreateExchange(model);
					CreateReceiver(model, exch);

					Console.WriteLine("Enter comma-separated message list. Empty = exit.");
					Console.WriteLine("'-' prefix - requeue. '=' prefix - republish.");

					for(; ; )
					{
						var inp = Console.ReadLine();
						if (string.IsNullOrWhiteSpace(inp)) break;

						foreach (var msg in inp.Split(','))
						{
							SendAsKey(model, exch, msg);
						}
					}
				}
			}
		}


		private static IConnection CreateConnection()
		{
			var factory = new ConnectionFactory
			{
				HostName = "localhost",
				UserName = "guest",
				Password = "guest"
			};

			return factory.CreateConnection();
		}


		private static string CreateExchange(IModel model)
		{
			const string name = "FanExchange";

			model.ExchangeDeclare(name, ExchangeType.Fanout, false, false, null);

			return name;
		}


		private static void CreateReceiver(IModel model, string exch)
		{
			var queue = model.QueueDeclare().QueueName;
			model.QueueBind(queue, exch, "", null);

			var recv = new RequeueReceiver(model, queue);
		}


		private static void SendAsKey(IModel model, string exch, string msg)
		{
			model.BasicPublish(exch, msg, null, null);
		}
	}
}
