using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace RabbitRequeueLab
{
	internal class RequeueReceiver
	{
		private const string RetryHeader = "RetryCount";

		private IModel mModel;


		public RequeueReceiver(IModel model, string queueName)
		{
			this.mModel = model;

			var consumer = new EventingBasicConsumer(model);
			consumer.Received += this.MessageReceived;
			model.BasicConsume(queueName, false, consumer);
		}


		private void MessageReceived(object sender, BasicDeliverEventArgs args)
		{
			var msg = args.RoutingKey;
			System.Console.WriteLine($"Received '{msg}'");

			if (msg[0] == '-' && !args.Redelivered)
			{
				mModel.BasicReject(args.DeliveryTag, true);
				System.Console.WriteLine("- Requeing");
			}
			else if (msg[0] == '=')
			{
				var retries = GetRetries(args.BasicProperties);
				if (retries < 2)
				{
					Console.WriteLine($"- {retries} retries, replublishing");
					SetRetries(args.BasicProperties, retries + 1);
					mModel.BasicPublish(args.Exchange, args.RoutingKey, args.BasicProperties, args.Body);
					mModel.BasicAck(args.DeliveryTag, false);
				}
				else
				{
					Console.WriteLine($"- {retries} retries, discarding");
					mModel.BasicReject(args.DeliveryTag, false);
				}
			}
			else
			{
				mModel.BasicAck(args.DeliveryTag, false);
			}
		}


		private int GetRetries(IBasicProperties prop)
		{
			if (prop?.Headers == null)
				return 0;

			if (prop.Headers.TryGetValue(RetryHeader, out var counter))
			{
				return (int) counter;
			}

			return 0;
		}


		private void SetRetries(IBasicProperties prop, int retries)
		{
			if (prop.Headers == null)
				prop.Headers = new Dictionary<string, object>();

			if (prop.Headers.ContainsKey(RetryHeader))
			{
				prop.Headers[RetryHeader] = retries;
			}
			else
			{
				prop.Headers.Add(RetryHeader, retries);
			}
		}
	}
}