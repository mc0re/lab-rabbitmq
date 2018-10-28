using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;


namespace RabbitScatterGatherLab
{
	internal class TopicReceiver
	{
		private readonly string mTopic;

		private readonly IModel mModel;


		public TopicReceiver(IModel model, string queueName, string topic)
		{
			mTopic = topic;
			mModel = model;

			var consumer = new EventingBasicConsumer(model);
			consumer.Received += Receive;
			model.BasicConsume(queueName, false, consumer);
		}


		private void Receive(object sender, BasicDeliverEventArgs ea)
		{
			var body = ea.Body;
			var message = Encoding.UTF8.GetString(body);

			Console.WriteLine(" [x] Received message '{0}' on topics {1}", message, mTopic);

			var prop = mModel.CreateBasicProperties();
			//prop.CorrelationId = ea.BasicProperties.CorrelationId;

			var resp = $"Result of {message} processed by {mTopic}";
			mModel.BasicPublish("", ea.BasicProperties.ReplyTo, prop, Encoding.Default.GetBytes(resp));
			mModel.BasicAck(ea.DeliveryTag, false);
		}
	}
}
