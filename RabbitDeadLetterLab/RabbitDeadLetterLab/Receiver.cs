using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitDeadLetterLab
{
	internal class Receiver
	{
		private readonly IModel mModel;


		public Receiver(IModel model, string queueName)
		{
			this.mModel = model;

			var consumer = new EventingBasicConsumer(model);
			consumer.Received += this.MessageReceived;
			model.BasicConsume(queueName, false, consumer);
		}


		private void MessageReceived(object sender, BasicDeliverEventArgs args)
		{
			var msg = Encoding.UTF8.GetString(args.Body);

			if (msg[0] == 'r')
			{
				System.Console.WriteLine($"{msg} rejected");
				mModel.BasicReject(args.DeliveryTag, false);
			}
			else
			{
				System.Console.WriteLine($"{msg} accepted");
				mModel.BasicAck(args.DeliveryTag, false);
			}
		}
	}
}