using RabbitMQ.Client;
using System;

namespace RabbitTopicLab
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (var conn = CreateRabbitConnection())
			{
				using (var model = conn.CreateModel())
				{
					string exch = CreateExchange(model);

					var topicList = CreateReceivers(model, exch, "a", "a.*", "a.*.*", "a.#", "*.a.*", "#.a.#", "#.a", "a *");

					Console.WriteLine("Enter topic. Empty string = exit. ? = help.");
					ShowTopics(topicList);

					for (; ; )
					{
						var inp = Console.ReadLine();
						if (string.IsNullOrWhiteSpace(inp)) break;

						if (inp == "?")
						{
							ShowTopics(topicList);
							continue;
						}

						Send(model, exch, inp);
					}
				}
			}
		}


		private static void ShowTopics(string[] topicList)
		{
			Console.WriteLine("Registered topics:");
			foreach (var t in topicList) Console.WriteLine("- {0}", t);
		}


		private static IConnection CreateRabbitConnection()
		{
			var factory = new ConnectionFactory()
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


		private static string[] CreateReceivers(IModel model, string exch, params string[] topics)
		{
			foreach (var topic in topics)
			{
				var queueName = model.QueueDeclare().QueueName;
				model.QueueBind(queueName, exch, topic);
				var recv = new TopicReceiver(model, queueName, topic);
			}

			return topics;
		}


		private static void Send(IModel model, string exch, string inp)
		{
			model.BasicPublish(exch, inp, null, null);
		}
	}
}
