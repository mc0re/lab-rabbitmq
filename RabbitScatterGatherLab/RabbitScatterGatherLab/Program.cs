using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;


namespace RabbitScatterGatherLab
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (var conn = Createconnection())
			{
				using (var model = conn.CreateModel())
				{
					string exch = CreateExchange(model);

					var topics = CreateReceivers(model, exch,
						new[] { "1", "3", "5", "7" }, new[] { "2", "3", "6", "7" }, new[] { "4", "5", "6", "7" });

					var respListener = CreateResponseListener(model);

					Console.WriteLine("Enter message topic (1-7). ? = help. Empty string = exit.");
					ShowTopics(topics);

					for (; ; )
					{
						var inp = Console.ReadLine();

						if (string.IsNullOrWhiteSpace(inp)) break;

						if (inp == "?")
						{
							ShowTopics(topics);
							continue;
						}

						Send(model, exch, inp, respListener.Queue);
						var responses = respListener.Receive(1000, 3);

						Console.WriteLine("Received {0} responses.", responses.Length);
						foreach (var resp in responses)
						{
							Console.WriteLine("  Response: {0}", resp);
						}
					}
				}
			}
		}


		private static IConnection Createconnection()
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
			const string name = "TopicExchange";

			model.ExchangeDeclare(name, ExchangeType.Topic, false, false, null);

			return name;
		}


		private static string[] CreateReceivers(IModel model, string exch, params string[][] topics)
		{
			var topicNames = new List<string>();

			foreach (var topicSet in topics)
			{
				var queueName = model.QueueDeclare().QueueName;

				foreach (var topic in topicSet)
				{
					model.QueueBind(queueName, exch, topic);
				}

				var topicName = string.Join(", ", topicSet);
				var recv = new TopicReceiver(model, queueName, topicName);
				topicNames.Add(topicName);
			}

			return topicNames.ToArray();
		}


		private static void ShowTopics(string[] topics)
		{
			Console.WriteLine("Registered receivers:");
			foreach (var topic in topics)
			{
				Console.WriteLine("  {0}", topic);
			}
		}


		private static ResponseReceiver CreateResponseListener(IModel model)
		{
			var name = model.QueueDeclare().QueueName;

			return new ResponseReceiver(model, name);
		}


		private static void Send(IModel model, string exch, string inp, string replyTo)
		{
			var prop = model.CreateBasicProperties();
			prop.CorrelationId = Guid.NewGuid().ToString();
			prop.ReplyTo = replyTo;

			var msg = string.Format("Selected {0}", inp);
			model.BasicPublish(exch, inp, prop, Encoding.Default.GetBytes(msg));
		}
	}
}
