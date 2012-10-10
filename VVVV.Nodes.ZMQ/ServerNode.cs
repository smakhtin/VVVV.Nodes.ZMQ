using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V2;
using ZMQ;

namespace VVVV.Nodes.ZMQ
{
	[PluginInfo(Name="Server", Category = "ZMQ", Help = "Send and receive data via ZMQ", Tags = "network", AutoEvaluate = true)]
	public class ServerNode : NetworkNode
	{
		[Input("Client IP", DefaultString = "*")]
		private IDiffSpread<string> FClientIpIn;
		
		[Output("Message")] 
		protected ISpread<string> FMessageOut;

		override public void Evaluate(int spreadMax)
		{
			FMessageOut.SliceCount = spreadMax;

			if (spreadMax > FSockets.Count) AddSockets(spreadMax);
			if (spreadMax < FSockets.Count) RemoveSockets(spreadMax);

			for (int i = 0; i < spreadMax; i++)
			{
				if (!FEnabledIn[i]) continue;

				Socket socket = FSockets[i];

				if(FTransportIn.IsChanged || FClientIpIn.IsChanged || FPortIn.IsChanged)
				{
					socket.Connect(FTransportIn[i], FClientIpIn[i], FPortIn[i]);
					socket.Subscribe("", Encoding.UTF8);
				}
				
				FMessageOut[i] = socket.Recv(Encoding.UTF8, SendRecvOpt.NOBLOCK);
			}
		}
		
		override protected void AddSockets(int spreadMax)
		{
			int count = spreadMax - FSockets.Count;

			for (int i = 0; i < count; i++)
			{
				FSockets.Add(FZmqContext.Socket(SocketType.SUB));
			}
		}
	}
}