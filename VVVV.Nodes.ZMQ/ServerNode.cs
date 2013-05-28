using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using ZMQ;

namespace VVVV.Nodes.ZMQ
{
	[PluginInfo(Name="Server", Category = "ZMQ", Help = "Send and receive data via ZMQ", Tags = "network", AutoEvaluate = true)]
	public class ServerNode : NetworkNode
	{
		[Input("Local IP", DefaultString = "*")]
		private IDiffSpread<string> FLocalIpIn;
		
		[Output("Message")] 
		protected ISpread<string> FMessageOut;

		override public void Evaluate(int spreadMax)
		{
			FMessageOut.SliceCount = spreadMax;
			FStatusOut.SliceCount = spreadMax;

			if (spreadMax > FSockets.Count) AddSockets(spreadMax);
			if (spreadMax < FSockets.Count) RemoveSockets(spreadMax);

			for (int i = 0; i < spreadMax; i++)
			{
				if (!FEnabledIn[i]) continue;

				var socket = FSockets[i];

				if(FTransportIn.IsChanged || FLocalIpIn.IsChanged || FPortIn.IsChanged)
				{
					try
					{
						socket.Unsubscribe("", Encoding.UTF8);
					}
					catch (Exception e)
					{
						FStatusOut[i] = e.Message;
					}
					
					try
					{
						socket.Bind(FTransportIn[i], FLocalIpIn[i], FPortIn[i]);
						FStatusOut[i] = "Binded";
					}
					catch (Exception e)
					{
						FStatusOut[i] = e.Message;
					}

					try
					{
						socket.Subscribe("", Encoding.UTF8);
					}
					catch (Exception e)
					{
						FStatusOut[i] = e.Message;
					}
					
				}

				try
				{
					var result = socket.RecvAll(Encoding.UTF8, SendRecvOpt.NOBLOCK);
					FMessageOut[i] = result.First();
				}
				catch (Exception e)
				{
					FStatusOut[i] = e.Message;
				}
			}
		}
		
		override protected void AddSockets(int spreadMax)
		{
			var count = spreadMax - FSockets.Count;

			for (var i = 0; i < count; i++)
			{
				FSockets.Add(FZmqContext.Socket(SocketType.SUB));
				FSockets[i].HWM = 2;
			}
		}
	}
}