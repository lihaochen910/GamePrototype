
namespace Pixpil.AI.HFSM
{
	public interface ITimer
	{
		float Elapsed
		{
			get;
		}

		void Reset();
	}
}
