using System;
using System.Threading.Tasks;

namespace RabbitMessageQueueLab
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var res = QueueManager.RegisterFan(3);

			try
			{
				// Async
				var rcv1 = new ReceiverEventBased(res.Model, res.RecvQueue[0]);

				// Sync, needs another thread
				var rcv2 = new ReceiverWaiting(res.Model, res.RecvQueue[1]);
				Task.Run(() => rcv2.ReceiveLoop());

				var rcv3 = new ReceiverWaitingSubscription(res.Model, res.RecvQueue[2]);
				Task.Run(() => rcv3.ReceiveLoop());

				var sender = new Sender(res.Model, res.SendExchange, res.SendKey);

				Console.WriteLine("Press ESC to exit, S to toggle subscriptions, any other key to send a message.");
				var msgIdx = 1;
				var leave = false;

				while (! leave)
				{
					var key = Console.ReadKey().Key;
					switch (key)
					{
						case ConsoleKey.Escape:
							leave = true;
							break;

						case ConsoleKey.S:
							rcv3.Subscribe();
							break;

						default:
							sender.Send($"Message {msgIdx}: {key}");
							msgIdx += 1;
							break;
					}
				}

				rcv2.IsEnabled = false;
				rcv3.IsEnabled = false;
			}
			finally
			{
				res.Model.Dispose();
				res.Conn.Dispose();
			}
		}
	}
}
