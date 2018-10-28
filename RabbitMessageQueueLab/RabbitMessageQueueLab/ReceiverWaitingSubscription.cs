using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Text;
using System.Threading;


namespace RabbitMessageQueueLab
{
	public class ReceiverWaitingSubscription
	{
		private readonly IModel mModel;

		private readonly string mQueueName;

		private readonly QueueingBasicConsumer mConsumer;

		private readonly Subscription mSubscription;

		public bool IsEnabled { get; set; }


		public ReceiverWaitingSubscription(IModel model, string queueName)
		{
			mModel = model;
			mQueueName = queueName;
			mConsumer = new QueueingBasicConsumer(model);
			model.BasicConsume(queueName, false, mConsumer);

			mSubscription = new Subscription(mModel, mQueueName);
		}


		public void Subscribe()
		{
			// ?
		}


		public void ReceiveLoop()
		{
			Console.WriteLine("Waiting receiver starts on thread {0}", Thread.CurrentThread.ManagedThreadId);
			IsEnabled = true;

			while (IsEnabled)
			{
				var args = mSubscription.Next();
				var message = Encoding.UTF8.GetString(args.Body);

				Console.WriteLine(" [x] Received {0} on subscription thread {1}", message, Thread.CurrentThread.ManagedThreadId);

				mSubscription.Ack(args);
			}

			Console.WriteLine("Waiting receiver exits.");
		}
	}
}
