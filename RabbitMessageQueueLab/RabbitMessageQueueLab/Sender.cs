using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;


namespace RabbitMessageQueueLab
{
	internal class Sender
	{
		private readonly IModel mModel;

		private readonly string mSendExchange;

		private readonly string mSendKey;


		public Sender(IModel model, string sendExchange, string sendKey)
		{
			mModel = model;
			mSendExchange = sendExchange;
			mSendKey = sendKey;
		}


		public void Send(string msg)
		{
			var prop = mModel.CreateBasicProperties();
			prop.Persistent = false;

			var body = Encoding.UTF8.GetBytes(msg);
			mModel.BasicPublish(mSendExchange, mSendKey, prop, body);
			Console.WriteLine(" [x] Sent {0} on thread {1}", msg, Thread.CurrentThread.ManagedThreadId);
		}
	}
}