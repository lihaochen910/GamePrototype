using System.Collections.Immutable;
using DigitalRune.Mathematics;
using ImGuiNET;
using Murder;
using Murder.Editor;
using Murder.Editor.Attributes;
using Murder.Editor.CustomComponents;
using Murder.Editor.Utilities;
using Pixpil.AI;


namespace Pixpil.GamePrototype.Editor.CustomComponents;

[CustomComponentOf( typeof( UtilityAiAgentComponent ) )]
public class UtilityAiAgentComponentEditor : CustomComponent {

	protected override bool DrawAllMembersWithTable( ref object target ) {

		var playingInEditor = Architect.Instance != null && Architect.Instance.IsPlayingGame;
		if ( playingInEditor ) {
#if DEBUG
			var utilityAiAgentComponent = ( UtilityAiAgentComponent )target;

			var asset = utilityAiAgentComponent.TryGetUtilityAiAsset();
			if ( asset is null ) {
				return base.DrawAllMembersWithTable( ref target, sameLineFilter );
			}

			if ( asset.RootReasoner is null || utilityAiAgentComponent.Reasoner is null ) {
				return base.DrawAllMembersWithTable( ref target, sameLineFilter );
			}

			UtilityAiReasoner reasoner = utilityAiAgentComponent.Reasoner;
			ImmutableArray< UtilityAiConsideration > considerations = reasoner.Considerations;
			
			ImGui.TextColored( Game.Profile.Theme.HighAccent, Prettify.FormatName( reasoner.GetType().Name ) );
			
			ImGui.SeparatorText( "Default" );
			DrawDebugConsideration( reasoner.DefaultConsideration );
			// ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.BgFaded );
			ImGui.SeparatorText( "Considerations" );
			// ImGui.PopStyleColor();

			if ( reasoner is HighestScoreReasoner ) {
				considerations = reasoner.Considerations.Sort(
					( ca , cb ) => ca.TryGetCachedResultScore() > cb.TryGetCachedResultScore() ? -1 : 1 );
			}
			if ( reasoner is LowestScoreReasoner ) {
				considerations = reasoner.Considerations.Sort(
					( ca , cb ) => ca.TryGetCachedResultScore() > cb.TryGetCachedResultScore() ? 1 : -1 );
			}

			foreach ( var consideration in considerations ) {
				DrawDebugConsideration( consideration );
			}
			
			ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.BgFaded );
			ImGui.SeparatorText( "Finish Considerations" );
			ImGui.PopStyleColor();
			ImGui.Spacing();
#endif
		}

		return base.DrawAllMembersWithTable( ref target );
	}

	private void DrawDebugConsideration( UtilityAiConsideration consideration ) {

		if ( consideration is null ) {
			return;
		}
		
		var considerationDebugName = consideration.Name;
		if ( string.IsNullOrEmpty( consideration.Name ) ) {
			considerationDebugName = consideration.Action != null ? consideration.Action.Name : "unnamed";
		}

		var colorCyan = new System.Numerics.Vector4( 0.01171875f, 0.9375f, 0.984375f, 1 );
		var considerationScore = consideration.TryGetCachedResultScore();
		
		if ( consideration is AllOrNothingConsideration allOrNothingConsideration ) {
			ImGui.TextColored( colorCyan, $"{considerationDebugName} = " );
			ImGui.SameLine();
			ImGui.Text( $"{considerationScore:0.00}" );
			ImGui.SameLine();
			ImGui.TextColored( Game.Profile.Theme.HighAccent, $" {nameof(AllOrNothingConsideration)}" );
			ImGui.Indent( 16 );
			ImGui.TextColored( Game.Profile.Theme.Yellow, $"Threshold: {allOrNothingConsideration.Threshold}" );

			foreach ( var kv in consideration.GetDebugUnitScoresDictionary() ) {
				var score = kv.Value;
				var successful = score >= allOrNothingConsideration.Threshold;
				ImGui.TextColored( Game.Profile.Theme.Accent, $"{GetApprisalDebugName( kv.Key )} =" );
				ImGui.SameLine();
				if ( successful ) {
					ImGui.PushStyleColor( ImGuiCol.Text, new System.Numerics.Vector4( 0, 1, 0, 1 ) );
				}
				else {
					ImGui.PushStyleColor( ImGuiCol.Text, new System.Numerics.Vector4( 1, 0, 0, 1 ) );
				}
				ImGui.Text( $"{score} >= {allOrNothingConsideration.Threshold}" );
				ImGui.PopStyleColor();
			}

			ImGui.Unindent( 16 );
		}

		if ( consideration is FixedScoreConsideration fixedScoreConsideration ) {
			ImGui.TextColored( colorCyan, $"{considerationDebugName} = " );
			ImGui.SameLine();
			ImGui.Text( $"{considerationScore:0.00}" );
			ImGui.SameLine();
			ImGui.SameLine();
			ImGui.TextColored( Game.Profile.Theme.HighAccent, $" {nameof(FixedScoreConsideration)}" );
		}

		if ( consideration is SumOfChildrenConsideration sumOfChildrenConsideration ) {
			ImGui.TextColored( colorCyan, $"{considerationDebugName} = " );
			ImGui.SameLine();
			ImGui.Text( $"{considerationScore:0.00}" );
			ImGui.SameLine();
			ImGui.TextColored( Game.Profile.Theme.HighAccent, $" {nameof(SumOfChildrenConsideration)}" );
			ImGui.Indent( 16 );

			foreach ( var kv in consideration.GetDebugUnitScoresDictionary() ) {
				var score = kv.Value;
				ImGui.TextColored( Game.Profile.Theme.Accent, $"{GetApprisalDebugName( kv.Key )}: " );
				if ( score >= 0f ) {
					// ImGui.PushStyleColor( ImGuiCol.Text, new System.Numerics.Vector4( 0, 1, 0, 1 ) );
					ImGui.SameLine();
					ImGui.Text( $"+{score}" );
				}
				else {
					ImGui.SameLine();
					ImGui.Text( $"{score}" );
				}
					
				ImGui.PopStyleColor();
			}
			
			ImGui.Unindent( 16 );
		}
		
		if ( consideration is SumOfChildrenWithPreAppraisalsConsideration sumOfChildrenWithPreAppraisalsConsideration ) {
			ImGui.TextColored( colorCyan, $"{considerationDebugName} = " );
			ImGui.SameLine();
			ImGui.Text( $"{considerationScore:0.00}" );
			ImGui.SameLine();
			ImGui.TextColored( Game.Profile.Theme.HighAccent, $" {nameof(SumOfChildrenWithPreAppraisalsConsideration)}" );
			
			ImGui.Indent( 16 );
			ImGui.TextColored( Game.Profile.Theme.Yellow, $"Threshold: {sumOfChildrenWithPreAppraisalsConsideration.Threshold}" );

			ImGui.Text( "Pre:" );
			if ( sumOfChildrenWithPreAppraisalsConsideration.PreCheckMode is SumOfChildrenWithPreAppraisalsConsideration.PreAppraisalsCheckMode.AllRequired ) {
				ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Green, $"({nameof( SumOfChildrenWithPreAppraisalsConsideration.PreAppraisalsCheckMode.AllRequired )})" );
			}
			else {
				ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Green, $"({nameof( SumOfChildrenWithPreAppraisalsConsideration.PreAppraisalsCheckMode.AnySuffice )})" );
			}
			ImGui.Indent( 8 );
			foreach ( var appraisal in sumOfChildrenWithPreAppraisalsConsideration.Appraisals ) {

				foreach ( var kv in consideration.GetDebugUnitScoresDictionary() ) {
					if ( kv.Key == appraisal ) {
						ImGui.TextColored( Game.Profile.Theme.Accent, $"{GetApprisalDebugName( appraisal )}: " );
						ImGui.SameLine();
						ImGui.TextColored( kv.Value < sumOfChildrenWithPreAppraisalsConsideration.Threshold ? Game.Profile.Theme.Red : Game.Profile.Theme.Green, $"{kv.Value:0.00} >= {sumOfChildrenWithPreAppraisalsConsideration.Threshold}" );
						break;
					}
				}
				
			}
			ImGui.Unindent( 8 );
			
			ImGui.Text( "Sum:" );
			ImGui.Indent( 8 );
			foreach ( var appraisal in sumOfChildrenWithPreAppraisalsConsideration.OptionalAppraisals ) {
				foreach ( var kv in consideration.GetDebugUnitScoresDictionary() ) {
					if ( kv.Key == appraisal ) {
						ImGui.TextColored( Game.Profile.Theme.Accent, $"{GetApprisalDebugName( appraisal )}: " );
						ImGui.SameLine();
						ImGui.TextColored( Game.Profile.Theme.Yellow, $"+({kv.Value:0.00})" );
					}
				}
			}
			ImGui.Unindent( 8 );
			
			ImGui.Unindent( 16 );
			return;
		}

		if ( consideration is ThresholdConsideration thresholdConsideration ) {
			ImGui.TextColored( colorCyan, $"{considerationDebugName} = " );
			ImGui.SameLine();
			ImGui.Text( $"{considerationScore:0.00}" );
			ImGui.SameLine();
			ImGui.TextColored( Game.Profile.Theme.HighAccent, $" {nameof(ThresholdConsideration)}" );
			ImGui.Indent( 16 );
			
			foreach ( var kv in consideration.GetDebugUnitScoresDictionary() ) {
				var score = kv.Value;
				if ( score < thresholdConsideration.Threshold ) {
					ImGui.PushStyleColor( ImGuiCol.Text, new System.Numerics.Vector4( 1, 0, 0, 1 ) );
				}
				else {
					ImGui.PushStyleColor( ImGuiCol.Text, new System.Numerics.Vector4( 0, 1, 0, 1 ) );
				}

				ImGui.Text( $"{GetApprisalDebugName( kv.Key )}: {score} < {thresholdConsideration.Threshold}" );
				ImGui.PopStyleColor();
			}
			
			ImGui.Unindent( 16 );
		}
	}


	public static void DrawDebugConsiderationNonRuntime( UtilityAiConsideration consideration ) {
		if ( consideration is null ) {
			return;
		}
		
		var considerationDebugName = consideration.Name;
		if ( string.IsNullOrEmpty( consideration.Name ) ) {
			considerationDebugName = consideration.Action != null ? consideration.Action.Name : "unnamed";
		}

		string PrettyScores( (float, float) valueRange ) {
			if ( Numeric.AreEqual( valueRange.Item1, valueRange.Item2 ) ) {
				return $"{valueRange.Item1:0.00}";
			}
			else {
				if ( valueRange.Item1 < valueRange.Item2 ) {
					return $"{valueRange.Item1:0.00} ~ {valueRange.Item2:0.00}";
				}
				else {
					return $"{valueRange.Item2:0.00} ~ {valueRange.Item1:0.00}";
				}
			}
		}

		var colorCyan = new System.Numerics.Vector4( 0.01171875f, 0.9375f, 0.984375f, 1 );
		var considerationScores = consideration.GetPreictedScores();
		var considerationScoresDebugName = PrettyScores( considerationScores );
		
		if ( consideration is AllOrNothingConsideration allOrNothingConsideration ) {
			ImGui.TextColored( colorCyan, $"{considerationDebugName} = " );
			ImGui.SameLine();
			ImGui.Text( $"{considerationScoresDebugName}" );
			ImGui.SameLine();
			ImGui.TextColored( Game.Profile.Theme.HighAccent, $" {nameof(AllOrNothingConsideration)}" );
			ImGui.Indent( 16 );
			ImGui.TextColored( Game.Profile.Theme.Yellow, $"Threshold: {allOrNothingConsideration.Threshold}" );

			foreach ( var appraisal in allOrNothingConsideration.Appraisals ) {
				ImGui.TextColored( Game.Profile.Theme.Accent, $"{GetApprisalDebugName( appraisal )}: " );
				ImGui.SameLine();
				ImGui.Text( $"{PrettyScores( appraisal.GetPredictedScores() )}" );
				ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Yellow, $" < {allOrNothingConsideration.Threshold}" );
			}

			ImGui.Unindent( 16 );
			return;
		}

		if ( consideration is FixedScoreConsideration fixedScoreConsideration ) {
			ImGui.TextColored( colorCyan, $"{considerationDebugName} = " );
			ImGui.SameLine();
			ImGui.Text( $"{fixedScoreConsideration.Score:0.00}" );
			ImGui.SameLine();
			ImGui.SameLine();
			ImGui.TextColored( Game.Profile.Theme.HighAccent, $" {nameof(FixedScoreConsideration)}" );
			return;
		}

		if ( consideration is SumOfChildrenConsideration sumOfChildrenConsideration ) {
			ImGui.TextColored( colorCyan, $"{considerationDebugName} = " );
			ImGui.SameLine();
			ImGui.Text( $"{considerationScoresDebugName}" );
			ImGui.SameLine();
			ImGui.TextColored( Game.Profile.Theme.HighAccent, $" {nameof(SumOfChildrenConsideration)}" );
			ImGui.Indent( 16 );

			foreach ( var appraisal in sumOfChildrenConsideration.Appraisals ) {
				ImGui.TextColored( Game.Profile.Theme.Accent, $"{GetApprisalDebugName( appraisal )}: " );
				ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Yellow, $"+-({PrettyScores( appraisal.GetPredictedScores() )})" );
			}
			
			ImGui.Unindent( 16 );
			return;
		}

		if ( consideration is SumOfChildrenWithPreAppraisalsConsideration sumOfChildrenWithPreAppraisalsConsideration ) {
			ImGui.TextColored( colorCyan, $"{considerationDebugName} = " );
			ImGui.SameLine();
			ImGui.Text( $"{considerationScoresDebugName}" );
			ImGui.SameLine();
			ImGui.TextColored( Game.Profile.Theme.HighAccent, $" {nameof(SumOfChildrenWithPreAppraisalsConsideration)}" );
			
			ImGui.Indent( 16 );
			ImGui.TextColored( Game.Profile.Theme.Yellow, $"Threshold: {sumOfChildrenWithPreAppraisalsConsideration.Threshold}" );

			ImGui.Text( "Pre:" );
			if ( sumOfChildrenWithPreAppraisalsConsideration.PreCheckMode is SumOfChildrenWithPreAppraisalsConsideration.PreAppraisalsCheckMode.AllRequired ) {
				ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Green, $"({nameof( SumOfChildrenWithPreAppraisalsConsideration.PreAppraisalsCheckMode.AllRequired )})" );
			}
			else {
				ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Green, $"({nameof( SumOfChildrenWithPreAppraisalsConsideration.PreAppraisalsCheckMode.AnySuffice )})" );
			}
			ImGui.Indent( 8 );
			foreach ( var appraisal in sumOfChildrenWithPreAppraisalsConsideration.Appraisals ) {
				ImGui.TextColored( Game.Profile.Theme.Accent, $"{GetApprisalDebugName( appraisal )}: " );
				ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Yellow, $"{PrettyScores( appraisal.GetPredictedScores() )} >= {sumOfChildrenWithPreAppraisalsConsideration.Threshold}" );
			}
			ImGui.Unindent( 8 );
			
			ImGui.Text( "Sum:" );
			ImGui.Indent( 8 );
			foreach ( var appraisal in sumOfChildrenWithPreAppraisalsConsideration.OptionalAppraisals ) {
				ImGui.TextColored( Game.Profile.Theme.Accent, $"{GetApprisalDebugName( appraisal )}: " );
				ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Yellow, $"+({PrettyScores( appraisal.GetPredictedScores() )})" );
			}
			ImGui.Unindent( 8 );
			
			ImGui.Unindent( 16 );
			return;
		}

		if ( consideration is ThresholdConsideration thresholdConsideration ) {
			ImGui.TextColored( colorCyan, $"{considerationDebugName} = " );
			ImGui.SameLine();
			ImGui.Text( $"{considerationScoresDebugName}" );
			ImGui.SameLine();
			ImGui.TextColored( Game.Profile.Theme.HighAccent, $" {nameof(ThresholdConsideration)}" );
			ImGui.Indent( 16 );
			
			foreach ( var appraisal in thresholdConsideration.Appraisals ) {
				ImGui.TextColored( Game.Profile.Theme.Accent, $"{GetApprisalDebugName( appraisal )}: " );
				ImGui.SameLine();
				ImGui.Text( $"{PrettyScores( appraisal.GetPredictedScores() )}" );
				ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Yellow, $" < {thresholdConsideration.Threshold}" );
			}
			
			ImGui.Unindent( 16 );
		}
		else {
			ImGui.TextColored( colorCyan, $"{considerationDebugName} = " );
			ImGui.SameLine();
			ImGui.Text( $"{considerationScoresDebugName}" );
			ImGui.SameLine();
			ImGui.TextColored( Game.Profile.Theme.HighAccent, $" {consideration.GetType().Name} TODO:" );
			
		}
	}


	private static string GetApprisalDebugName( IAppraisal appraisal ) {
		if ( appraisal is GoapConditionAppraisal goapConditionAppraisal ) {
				if ( goapConditionAppraisal.GoapCondition != null ) {
					return $"{goapConditionAppraisal.GoapCondition.GetType().Name}";
				}
				else {
					return "null GoapCondition";
				}
		}
		return appraisal.GetType().Name;
	}
}
