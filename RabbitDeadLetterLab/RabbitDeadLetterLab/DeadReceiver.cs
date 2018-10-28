using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitDeadLetterLab
{
	internal class DeadReceiver
	{
		private readonly IModel mModel;


		public DeadReceiver(IModel model, string queueName)
		{
			this.mModel = model;

			var consumer = new EventingBasicConsumer(model);
			consumer.Received += this.MessageReceived;
			model.BasicConsume(queueName, false, consumer);
		}


		private void MessageReceived(object sender, BasicDeliverEventArgs args)
		{
			var msg = Encoding.UTF8.GetString(args.Body);

			// Fix the message :-)
			msg = "*" + msg;

			var deathCollection = args.BasicProperties.Headers["x-death"] as List<object>;
			var deathDict = deathCollection[0] as Dictionary<string, object>;

			var reason = Encoding.Default.GetString(deathDict["reason"] as byte[]);
			var origExch= Encoding.Default.GetString(deathDict["exchange"] as byte[]);
			var origQueue = Encoding.Default.GetString(deathDict["queue"] as byte[]);

			var body = Encoding.UTF8.GetBytes(msg);
			mModel.BasicPublish(origExch, origQueue, false, args.BasicProperties, body);
			mModel.BasicAck(args.DeliveryTag, false);
		}
	}
}