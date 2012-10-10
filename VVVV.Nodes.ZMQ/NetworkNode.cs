using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;
using ZMQ;

namespace VVVV.Nodes.ZMQ
{
	public abstract class NetworkNode : IPluginEvaluate
	{
		[Input("Transport", DefaultEnumEntry = "TCP")]
		protected IDiffSpread<Transport> FTransportIn;
		
		[Input("Port", DefaultValue = 5555)]
		protected IDiffSpread<uint> FPortIn;

		[Input("Enabled")]
		protected ISpread<bool> FEnabledIn;

		[Input("Threads Size", DefaultValue = 1, IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
		protected IDiffSpread<int> FThreadsSizeIn;

		protected readonly Context FZmqContext = new Context(1);
		
		protected readonly List<Socket> FSockets = new List<Socket>();
		
		public abstract void Evaluate(int spreadmax);
		
		protected void RemoveSockets(int spreadMax)
		{
			for (int i = FSockets.Count; i <= spreadMax; i--)
			{
				int index = i - 1;

				FSockets[index].Dispose();
				FSockets.RemoveAt(index);
			}
		}
		
		protected abstract void AddSockets(int spreadMax);
	}
}