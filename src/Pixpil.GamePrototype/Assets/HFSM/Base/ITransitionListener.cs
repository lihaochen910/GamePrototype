
namespace Pixpil.AI.HFSM;

public interface ITransitionListener {
	void BeforeTransition();
	void AfterTransition();
}
