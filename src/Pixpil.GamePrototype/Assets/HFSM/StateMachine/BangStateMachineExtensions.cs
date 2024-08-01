using Bang.Entities;


namespace Pixpil.AI.HFSM;

public static class BangStateMachineExtensions {

	public static void PauseHFSMAgent( this global::Bang.Entities.Entity e ) {
		if ( e.HasHFSMAgent() ) {
			e.SetHFSMPaused();
		}
	}

	public static void ResumeHFSMAgent( this global::Bang.Entities.Entity e ) => e.RemoveHFSMPaused();

	public static bool IsHFSMAgentPaused( this global::Bang.Entities.Entity e ) {
		if ( e.HasHFSMAgent() ) {
			return e.HasHFSMPaused();
		}

		return false;
	}
}
