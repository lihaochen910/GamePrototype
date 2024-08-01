using System;

namespace Pixpil.AI.HFSM.Exceptions
{
	[Serializable]
	public class StateMachineException : Exception
	{
		public StateMachineException(string message) : base(message) { }
	}
}
