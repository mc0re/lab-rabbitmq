using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Text;
using System.Threading;


namespace RabbitMessageQueueLab
{
	public class ReceiverEventBased
	{
		private readonly IModel mModel;

		private readonly string mQueueName;


		public ReceiverEventBased(IModel model, string queueName)
		{
			mModel = model;
			mQueueName = queueName;

			var consumer = new EventingBasicConsumer(model);
			consumer.Received += Receive;
			model.BasicConsume(queueName, true, consumer);
		}


		private void Receive(object sender, BasicDeliverEventArgs ea)
		{
			var body = ea.Body;
			var message = Encoding.UTF8.GetString(body);
			Console.WriteLine(" [x] Received {0} on event thread {1}", message, Thread.CurrentThread.ManagedThreadId);
		}
	}
}