using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V2;
using ZMQ;

namespace VVVV.Nodes.ZMQ
{
	public class ServerNode : IPluginEvaluate
	{
		[Input("Message")] 
		private ISpread<string> FMessageIn;

		[Input("Send")]
		private ISpread<bool> FSendIn;

		[Input("Transport", DefaultEnumEntry = "TCP")]
		private IDiffSpread<Transport> FTransportIn;

		[Input("Client IP", DefaultString = "*")]
		private IDiffSpread<string> FClientIpIn;

		[Input("Port", DefaultValue = 5555)]
		private IDiffSpread<uint> FPortIn;

		[Input("Enabled")]
		private ISpread<bool> FEnabledIn;

		[Input("Threads Size", DefaultValue = 1, IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
		private IDiffSpread<int> FThreadsSizeIn;

		[Output("Message")] 
		private Spread<string> FMessageOut;

		readonly Context FZmqContext = new Context(1);
		
		private readonly List<Socket> FSockets = new List<Socket>(); 

		public void Evaluate(int spreadMax)
		{
			FMessageOut.SliceCount = spreadMax;

			if (spreadMax < FSockets.Count) AddSockets(spreadMax);
			if (spreadMax > FSockets.Count) RemoveSockets(spreadMax);

			for (int i = 0; i < spreadMax; i++)
			{
				if (!FEnabledIn[i]) continue;

				Socket socket = FSockets[i];

				if(FTransportIn.IsChanged || FClientIpIn.IsChanged || FPortIn.IsChanged)
				{
						socket.Bind(FTransportIn[i], FClientIpIn[i], FPortIn[i]);
				}

				FMessageOut[i] = socket.Recv(Encoding.UTF8);

				if(FSendIn[i])
				{
					socket.Send(FMessageIn[i], Encoding.UTF8);
				}
				
			}
		}

		private void RemoveSockets(int spreadMax)
		{
			for (int i = FSockets.Count; i <= spreadMax; i--)
			{
				int index = i - 1;

				FSockets[index].Dispose();
				FSockets.RemoveAt(index);
			}
		}

		private void AddSockets(int spreadMax)
		{
			int count = spreadMax - FSockets.Count;

			for (int i = 0; i < count; i++)
			{
				FSockets.Add(FZmqContext.Socket(SocketType.REP));
			}
		}
	}
}