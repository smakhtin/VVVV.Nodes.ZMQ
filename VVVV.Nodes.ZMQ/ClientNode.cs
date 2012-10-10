using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V2;
using ZMQ;

namespace VVVV.Nodes.ZMQ
{
	[PluginInfo(Name="Client", Category = "ZMQ", Help = "Send and receive data via ZMQ", Tags = "network", AutoEvaluate = true)]
	public class ClientNode : NetworkNode
	{
		[Input("Server IP", DefaultString = "localhost")]
		private IDiffSpread<string> FServerIpIn;
		
		[Input("Message")]
		protected ISpread<string> FMessageIn;

		[Input("Send", IsBang = true)]
		protected ISpread<bool> FSendIn;
	
		override public void Evaluate(int spreadMax)
		{
			if (spreadMax > FSockets.Count) AddSockets(spreadMax);
			if (spreadMax < FSockets.Count) RemoveSockets(spreadMax);
			
			for (int i = 0; i < spreadMax; i++)
			{
				if (!FEnabledIn[i]) continue;

				Socket socket = FSockets[i];

				if(FTransportIn.IsChanged || FServerIpIn.IsChanged || FPortIn.IsChanged)
				{
					socket.Bind(FTransportIn[i], FServerIpIn[i], FPortIn[i]);
				}
				
				if(FSendIn[i])
				{
					socket.Send(FMessageIn[i], Encoding.UTF8);
				}
			}
		}
		
		override protected void AddSockets(int spreadMax)
		{
			int count = spreadMax - FSockets.Count;

			for (int i = 0; i < count; i++)
			{
				FSockets.Add(FZmqContext.Socket(SocketType.PUB));
			}
		}
	}
}
