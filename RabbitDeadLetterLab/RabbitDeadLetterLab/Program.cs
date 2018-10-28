using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace RabbitDeadLetterLab
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			try
			{
				using (var conn = CreateConnection())
				{
					using (var model = conn.CreateModel())
					{
						var processingExch = CreateFanoutExchange(model, "Processing");
						var deadExch = CreateFanoutExchange(model, "DeadLetter");
						var deadQueue = CreateQueue(model, deadExch);
						var processingQueue = CreateQueue(model, processingExch, deadExch);

						var procRecv = new Receiver(model, processingQueue);
						var deadRecv = new DeadReceiver(model, deadQueue);

						Console.WriteLine("Enter the message. Starts with 'r' - rejected. Empty = exit.");

						for (; ; )
						{
							var inp = Console.ReadLine();

							if (string.IsNullOrEmpty(inp)) break;

							Send(model, processingExch, inp);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception: " + ex.Message);
			}
		}


		private static IConnection CreateConnection()
		{
			var factory = new ConnectionFactory
			{
				HostName = "localhost",
				UserName = "hello",
				Password = "hello",
				VirtualHost = "lab"
			};

			return factory.CreateConnection();
		}


		private static string CreateFanoutExchange(IModel model, string name)
		{
			name = "Fanout" + name;

			model.ExchangeDeclare(name, ExchangeType.Fanout, false, false, null);

			return name;
		}


		private static string CreateQueue(IModel model, string exch, string deadExch = null)
		{
			var args =
				deadExch == null
				? null
				: new Dictionary<string, object> { { "x-dead-letter-exchange", deadExch } };

			var qname = model.QueueDeclare(durable: deadExch != null, arguments: args).QueueName;
			model.QueueBind(qname, exch, "", null);

			return qname;
		}


		private static void Send(IModel model, string exch, string inp)
		{
			model.BasicPublish(exch, "", null, Encoding.UTF8.GetBytes(inp));
		}
	}
}
