using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Text;
using System.Threading;


namespace RabbitMessageQueueLab
{
	public class ReceiverWaiting
	{
		private readonly IModel mModel;

		private readonly string mQueueName;

		private readonly QueueingBasicConsumer mConsumer;

		public bool IsEnabled { get; set; }


		public ReceiverWaiting(IModel model, string queueName)
		{
			mModel = model;
			mQueueName = queueName;
			mConsumer = new QueueingBasicConsumer(model);
			model.BasicConsume(queueName, false, mConsumer);
		}


		public void ReceiveLoop()
		{
			Console.WriteLine("Waiting receiver starts on thread {0}", Thread.CurrentThread.ManagedThreadId);
			IsEnabled = true;

			while (IsEnabled)
			{
				var args = (mConsumer.Queue.Dequeue() as BasicDeliverEventArgs);
				var message = Encoding.UTF8.GetString(args.Body);

				Console.WriteLine(" [x] Received {0} on dequeue thread {1}", message, Thread.CurrentThread.ManagedThreadId);

				mModel.BasicAck(args.DeliveryTag, false);
			}

			Console.WriteLine("Waiting receiver exits.");
		}
	}
}
