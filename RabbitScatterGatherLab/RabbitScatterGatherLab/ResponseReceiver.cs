using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace RabbitScatterGatherLab
{
	internal class ResponseReceiver
	{
		public string Queue { get; private set; }


		private Subscription mSubscription;


		public ResponseReceiver(IModel model, string queueName)
		{
			Queue = queueName;
			mSubscription = new Subscription(model, queueName);
		}


		public string[] Receive(int timeout, int maxResponses)
		{
			var responses = new List<string>();

			bool ReceiveOne()
			{
				if (!mSubscription.Next(timeout, out var args))
					return false;

				var body = args.Body;
				var message = Encoding.UTF8.GetString(body);
				lock(responses) responses.Add(message);

				return true;
			}

			for (; ; )
			{
				var task = Task.Run(() => ReceiveOne());

				if (!task.Result) break;
				if (responses.Count >= maxResponses) break;
			}

			return responses.ToArray();
		}
	}
}
