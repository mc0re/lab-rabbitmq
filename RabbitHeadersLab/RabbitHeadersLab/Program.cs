using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitHeadersLab
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
					CreateReceivers(exch, model, new HeaderSetCollection
					{
						new AllHeaders("Letter", "a", "Number", "1"),
						new AnyHeader("Letter", "a", "Number", "1"),
						new AllHeaders("Letter", "z", "Number", "1"),
						new AnyHeader("Letter", "z", "Number", "1"),
						new AllHeaders("Letter", "z", "Number", "9"),
						new AnyHeader("Letter", "z", "Number", "9")
					});

					Console.WriteLine("Enter headers: letter, number. Empty string = exit.");

					for (; ; )
					{
						var inp = Console.ReadLine();
						if (string.IsNullOrWhiteSpace(inp)) break;

						if (inp.Length != 2)
						{
							Console.WriteLine("Expecting letter and number");
							continue;
						}

						var letter = inp[0];
						var number = inp[1];
						Send(model, exch, letter, number);
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
			const string name = "HeadersExchange";

			model.ExchangeDeclare(name, ExchangeType.Headers, false, false, null);

			return name;
		}


		private static void CreateReceivers(string exch, IModel model, HeaderSetCollection headerSetCollection)
		{
			foreach (var headerSet in headerSetCollection)
			{
				var queueName = model.QueueDeclare().QueueName;
				model.QueueBind(queueName, exch, "", headerSet.ToArgs());
				var recv = new HeaderReceiver(model, queueName, headerSet);
			}
		}


		private static void Send(IModel model, string exch, char letter, char number)
		{
			var msg = string.Format("Entered {0}-{1}", letter, number);
			var body = Encoding.Default.GetBytes(msg);

			var prop = model.CreateBasicProperties();
			prop.Headers = new Dictionary<string, object>
			{
				{ "Letter", letter.ToString() },
				{ "Number", number.ToString() }
			};

			model.BasicPublish(exch, "", prop, body);
		}
	}
}
