using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;


namespace RabbitHeadersLab
{
	internal class HeaderReceiver
	{
		private readonly IModel mModel;

		private readonly string mAbbr;


		public HeaderReceiver(IModel model, string queueName, HeaderSer headerSet)
		{
			this.mModel = model;
			mAbbr = headerSet.ToAbbreviation();

			var consumer = new EventingBasicConsumer(model);
			consumer.Received += Receive;
			model.BasicConsume(queueName, true, consumer);
		}


		private void Receive(object sender, BasicDeliverEventArgs ea)
		{
			var body = ea.Body;
			var message = Encoding.UTF8.GetString(body);
			var prop = ea.BasicProperties;

			Console.WriteLine(" [x] Received message '{0}' for headers {1}", message, mAbbr);
		}
	}
}
