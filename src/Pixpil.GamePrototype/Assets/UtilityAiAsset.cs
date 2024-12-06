using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Bang;
using Bang.Components;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DigitalRune.Mathematics;
using Murder;
using Murder.Assets;
using Murder.Attributes;
using Murder.Core.Graphics;
using Murder.Utilities;
using Murder.Utilities.Attributes;


namespace Pixpil.AI {
	
	using Pixpil.Assets;
	using Pixpil.Messages;
    
    public enum UtilityAiEvaluateMethod : byte {
    	Interval,
    	Message
    }

	public sealed class UtilityAiAction {
		public string Name;
		public bool IsActived = true;
		
		public UtilityAiAction( string name ) {
			Name = name;
			IsActived = true;
		}
	}


	/// <summary>
	/// encapsulates an Action and generates a score that a Reasoner can use to decide which Consideration to use
	/// </summary>
	public interface IConsideration {

		UtilityAiAction Action { get; set; }

		float GetScore( World world, Entity entity );
	}


	public interface IWithEcsContext {
		
		[System.Text.Json.Serialization.JsonIgnore]
		World World { get; }
			
		[System.Text.Json.Serialization.JsonIgnore]
		Entity Entity { get; }
		
		[System.Text.Json.Serialization.JsonIgnore]
		public bool ContextSetted { get; }

		void SetupContext( World world, Entity entity );
		void ClearContext();
	}


	[Serializable]
	public abstract class UtilityAiConsideration : IConsideration {

		public string Name = string.Empty;

		[Serialize]
		public UtilityAiAction Action {
			get => _action;
			set => _action = value;
		}
		private UtilityAiAction _action;
		
		[JsonIgnore]
		public virtual string Description => string.Empty;
		
		public abstract float GetScore( World world, Entity entity );


		/// <summary>
		/// for debug
		/// </summary>
		/// <returns></returns>
		public virtual (float, float) GetPreictedScores() => ( 0, 0 );


		#region Editor Helper

		[System.Text.Json.Serialization.JsonIgnore, HideInEditor]
		public System.Func< UtilityAiAsset > FuncGetOwnerUtilityAiAsset;
		
		[System.Text.Json.Serialization.JsonIgnore, HideInEditor]
		private Dictionary< IAppraisal, float > _unitScores = new ();
		private float _considerationResultScore;

		public const string UNITSCORE_RESULT = "result";


		protected void CacheDebugUnitScore( IAppraisal appraisal, float score ) {
			// string unitName = appraisal.GetType().Name;
			// if ( appraisal is GoapConditionAppraisal goapConditionAppraisal ) {
			// 	if ( goapConditionAppraisal.GoapCondition != null ) {
			// 		unitName = $"{goapConditionAppraisal.GoapCondition.GetType().Name}";
			// 	}
			// 	else {
			// 		unitName = "null GoapCondition";
			// 	}
			// }
			// CacheDebugUnitScore( unitName, score );
			_unitScores[ appraisal ] = score;
		}
		
		// protected void CacheDebugUnitScore( string unit, float score ) {
		// 	_unitScores[ unit ] = score;
		// }
		
		protected void CacheResultScore( float score ) {
			_considerationResultScore = score;
		}

		public Dictionary< IAppraisal, float > GetDebugUnitScoresDictionary() => _unitScores;

		public float TryGetCachedResultScore() => _considerationResultScore;

		public void ClearDebugUnitScoreCache() {
			_unitScores.Clear();
			_considerationResultScore = 0f;
		}

		#endregion
		
	}


	[Serializable]
	public abstract class UtilityAiReasoner : IWithEcsContext {

		[JsonInclude]
		public UtilityAiConsideration DefaultConsideration { get; set; }

		[JsonInclude]
		public ImmutableArray< UtilityAiConsideration > Considerations = ImmutableArray< UtilityAiConsideration >.Empty;
		
		[JsonIgnore]
		public virtual string Description => string.Empty;
		
		
		protected abstract IConsideration SelectBestConsideration();


		public UtilityAiAction Select() => SelectBestConsideration()?.Action;


		public UtilityAiReasoner AddConsideration( UtilityAiConsideration consideration ) {
			Considerations = Considerations.Add( consideration );
			return this;
		}


		public UtilityAiReasoner SetDefaultConsideration( UtilityAiConsideration defaultConsideration ) {
			DefaultConsideration = defaultConsideration;
			return this;
		}
		
		
		#region Ecs Context

		[System.Text.Json.Serialization.JsonIgnore]
		public World World { get; private set; }
			
		[System.Text.Json.Serialization.JsonIgnore]
		public Entity Entity { get; private set; }
		
		[System.Text.Json.Serialization.JsonIgnore]
		public bool ContextSetted { get; private set; }
		
		public virtual void SetupContext( World world, Entity entity ) {
			World = world;
			Entity = entity;
			ContextSetted = true;
		}
			
		public virtual void ClearContext() {
			World = null;
			Entity = null;
			ContextSetted = false;
		}

		#endregion


		#region Editor Helper

		[JsonIgnore, HideInEditor]
		public System.Func< UtilityAiAsset > FuncGetOwnerUtilityAiAsset;
		
		#endregion
	}


	#region Built-in Reasoner

	/// <summary>
	/// The first Consideration to score above the score of the Default Consideration is selected
	/// </summary>
	public class FirstScoreReasoner : UtilityAiReasoner {

		public override string Description =>
			"The first Consideration to score above the score of the Default Consideration is selected.";
		
		public FirstScoreReasoner() {}
		
		protected override IConsideration SelectBestConsideration() {
#if DEBUG
			var defaultScore = DefaultConsideration.GetScore( World, Entity );
			IConsideration bestConsideration = default;
			foreach ( var consideration in Considerations ) {
				consideration.ClearDebugUnitScoreCache();
				if ( consideration.GetScore( World, Entity ) >= defaultScore ) {
					bestConsideration = consideration;
				}
			}

			if ( bestConsideration is not null ) {
				return bestConsideration;
			}

			return DefaultConsideration;
#else	
			var defaultScore = DefaultConsideration.GetScore( World, Entity );
			foreach ( var consideration in Considerations ) {
				if ( consideration.GetScore( World, Entity ) >= defaultScore ) {
					return consideration;
				}
			}

			return DefaultConsideration;
#endif
		}
	}


	/// <summary>
	/// The Consideration with the highest score is selected
	/// </summary>
	public class HighestScoreReasoner : UtilityAiReasoner {
		
		public override string Description =>
			"The Consideration with the highest score is selected.";
		
		[JsonConstructor]
		public HighestScoreReasoner() {}

		protected override IConsideration SelectBestConsideration() {
			var highestScore = DefaultConsideration != null ? DefaultConsideration.GetScore( World, Entity ) : float.MinValue;
			IConsideration consideration = null;
			for ( var i = 0; i < Considerations.Length; i++ ) {
#if DEBUG
				Considerations[ i ].ClearDebugUnitScoreCache();
#endif
				var score = Considerations[ i ].GetScore( World, Entity );
				if ( score > highestScore ) {
					highestScore = score;
					consideration = Considerations[ i ];
				}
			}

			if ( consideration is null ) {
				return DefaultConsideration;
			}

			return consideration;
		}
	}
	
	
	/// <summary>
	/// The Consideration with the lowest score is selected
	/// </summary>
	public class LowestScoreReasoner : UtilityAiReasoner {
		
		public override string Description =>
			"The Consideration with the lowest score is selected.";
		
		[JsonConstructor]
		public LowestScoreReasoner() {}

		protected override IConsideration SelectBestConsideration() {
			var lowestScore = DefaultConsideration.GetScore( World, Entity );
			IConsideration consideration = null;
			for ( var i = 0; i < Considerations.Length; i++ ) {
#if DEBUG
				Considerations[ i ].ClearDebugUnitScoreCache();
#endif
				var score = Considerations[ i ].GetScore( World, Entity );
				if ( score < lowestScore ) {
					lowestScore = score;
					consideration = Considerations[ i ];
				}
			}

			if ( consideration is null ) {
				return DefaultConsideration;
			}

			return consideration;
		}
	}

	#endregion


	#region Considerations


	#region Appraisals

	/// <summary>
	/// scorer for use with a Consideration
	/// </summary>
	public interface IAppraisal {
		float GetScore( World world, Entity entity );
		(float, float) GetPredictedScores();
	}


	/// <summary>
	/// wraps a Func for use as an Appraisal without having to create a subclass
	/// </summary>
	public class DelegateAppraisal( Func< World, Entity, float > appraisalAction ) : IAppraisal {

		float IAppraisal.GetScore( World world, Entity entity ) => appraisalAction( world, entity );

		(float, float) IAppraisal.GetPredictedScores() => (0, 0);
	}


	public sealed class FixedScoreAppraisal : IAppraisal {
		
		public readonly float Score = 0f;

		public float GetScore( World world, Entity entity ) => Score;

		public (float, float) GetPredictedScores() => ( Score, Score );

		public override string ToString() {
			return $"FixedScoreAppraisal -> {Score}";
		}
	}
	
	
	public sealed class RandomScoreInRangeCondition : IAppraisal {
		
		public readonly float Min = 0f;
		public readonly float Max = 1f;
		
		public float GetScore( World world, Entity entity ) => Game.Random.NextFloat( Min, Max );

		public (float, float) GetPredictedScores() => ( Min, Max );

		
		public override string ToString() {
			return $"RandomScoreInRange -> ({Min}, {Max})";
		}
	}

	#endregion


	/// <summary>
	/// Always returns a fixed score. Serves double duty as a default Consideration.
	/// 总是返回固定的值
	/// </summary>
	public class FixedScoreConsideration : UtilityAiConsideration {
		
		public override string Description =>
			"Always returns a fixed score. Serves double duty as a default Consideration.\nreturn FixedScore.";

		
		public readonly float Score;
		
		public FixedScoreConsideration() => Score = 1f;

		public FixedScoreConsideration( float score = 1f ) => Score = score;

		public override float GetScore( World world, Entity entity ) {
#if DEBUG
			CacheResultScore( Score );
#endif
			return Score;
		}

		public override (float, float) GetPreictedScores() => ( Score, Score );
	}


	/// <summary>
	/// Only scores if all child Appraisals score above the threshold
	/// 按顺序遍历Appraisals, 返回总和. 仅当所有IAppraisal返回值都大于Threshold才计算, 否则返回0
	/// </summary>
	public class AllOrNothingConsideration : UtilityAiConsideration {
		
		public override string Description =>
			"Only scores if all child Appraisals score above the threshold.\nif ANY Appraisal < Threshold, then return 0, else return sum.";

		
		public readonly float Threshold;

		public readonly ImmutableArray< IAppraisal > Appraisals = ImmutableArray< IAppraisal >.Empty;


		public AllOrNothingConsideration() => Threshold = 1f;
		
		public AllOrNothingConsideration( float threshold = 1f ) => Threshold = threshold;


		public override float GetScore( World world, Entity entity ) {
#if DEBUG
			var sum = 0f;
			var returned = false;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var score = Appraisals[ i ].GetScore( world, entity );
				CacheDebugUnitScore( Appraisals[ i ], score );
				if ( score < Threshold ) {
					returned = true;
				}
				else {
					sum += score;
				}
			}

			if ( returned ) {
#if DEBUG
				CacheResultScore( 0 );
#endif
				return 0f;
			}

#if DEBUG
			CacheResultScore( sum );
#endif
			return sum;
#else
			var sum = 0f;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var score = Appraisals[ i ].GetScore( world, entity );
				if ( score < Threshold ) {
					return 0;
				}

				sum += score;
			}

			return sum;
#endif
		}

		public override (float, float) GetPreictedScores() {
			var sum = 0f;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var scores = Appraisals[ i ].GetPredictedScores();
				if ( scores.Item2 >= Threshold ) {
					sum += scores.Item2;
				}
			}

			return ( 0, sum );
		}
	}
	
	
	/// <summary>
	/// 按顺序遍历Appraisals, 返回固定分数. 仅当所有IAppraisal返回值都大于Threshold才计算, 否则返回0
	/// </summary>
	public class AllOrNothingReturnFixedScoreConsideration : UtilityAiConsideration {
		
		public override string Description =>
			"Only scores if all child Appraisals score above the threshold.\nif ANY Appraisal < Threshold, then return 0, else return fixed score.";

		
		public readonly float Threshold;
		public readonly float ReturnScore;

		public readonly ImmutableArray< IAppraisal > Appraisals = ImmutableArray< IAppraisal >.Empty;


		public AllOrNothingReturnFixedScoreConsideration() {
			Threshold = 1f;
			ReturnScore = 1f;
		}
		
		public AllOrNothingReturnFixedScoreConsideration( float threshold = 1f ) => Threshold = threshold;


		public override float GetScore( World world, Entity entity ) {
#if DEBUG
			var returned = false;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var score = Appraisals[ i ].GetScore( world, entity );
				CacheDebugUnitScore( Appraisals[ i ], score );
				if ( score < Threshold ) {
					returned = true;
				}
				else {
					// do nothing.
				}
			}

			if ( returned ) {
#if DEBUG
				CacheResultScore( 0 );
#endif
				return 0f;
			}

#if DEBUG
			CacheResultScore( ReturnScore );
#endif
			return ReturnScore;
#else
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var score = Appraisals[ i ].GetScore( world, entity );
				if ( score < Threshold ) {
					return 0;
				}
			}

			return ReturnScore;
#endif
		}

		public override (float, float) GetPreictedScores() {
			return ( 0, ReturnScore );
		}
	}


	/// <summary>
	/// Scores by summing the score of all child Appraisals
	/// 返回Appraisals计算总和
	/// </summary>
	public class SumOfChildrenConsideration : UtilityAiConsideration {
		
		public override string Description =>
			"Scores by summing the score of all child Appraisals.\nreturn TheSumOf(Appraisals).";
		
		public readonly ImmutableArray< IAppraisal > Appraisals = ImmutableArray< IAppraisal >.Empty;

		public override float GetScore( World world, Entity entity ) {
			var score = 0f;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var appraisalScore = Appraisals[ i ].GetScore( world, entity );
				score += appraisalScore;
#if DEBUG
				CacheDebugUnitScore( Appraisals[ i ], appraisalScore );
#endif
			}

#if DEBUG
			CacheResultScore( score );
#endif
			return score;
		}

		public override (float, float) GetPreictedScores() {
			var min = 0f;
			var max = 0f;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var scores = Appraisals[ i ].GetPredictedScores();
				min += scores.Item1;
				max += scores.Item2;
			}
			return ( min, max );
		}
	}


	/// <summary>
	/// 按顺序遍历Appraisals, 返回总和. 仅IAppraisal返回值比对Threshold成功后才计入总和 ( if IAppraisal (CompareMethod) Threshold, then sum += IAppraisal. return sum. ).
	/// </summary>
	public class SumOfChildrenWithThresholdConsideration : UtilityAiConsideration {
		
		public override string Description =>
			"if Appraisal (CompareMethod) Threshold, then sum += Appraisal. return sum.";
		
		public readonly CompareMethod CompareMethod;
		public readonly float Threshold;
		
		public readonly ImmutableArray< IAppraisal > Appraisals = ImmutableArray< IAppraisal >.Empty;

		public override float GetScore( World world, Entity entity ) {
			var sum = 0f;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var score = Appraisals[ i ].GetScore( world, entity );
#if DEBUG
				CacheDebugUnitScore( Appraisals[ i ], score );
#endif
				if ( NumericCompareHelper.Compare( score, CompareMethod, Threshold ) ) {
					sum += score;
				}
			}

#if DEBUG
			CacheResultScore( sum );
#endif
			return sum;
		}
		
		public override (float, float) GetPreictedScores() {
			var max = 0f;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var scores = Appraisals[ i ].GetPredictedScores();
				if ( NumericCompareHelper.Compare( scores.Item2, CompareMethod, Threshold ) ) {
					max += scores.Item2;
				}
			}
			return ( 0, max );
		}
	}


	/// <summary>
	/// Scores by summing child Appraisals until a child scores below the threshold
	/// 按顺序遍历Appraisals, 返回总和. 如果其中某个IAppraisal返回值小于Threshold, 停止遍历并返回当前总和.
	/// </summary>
	public class ThresholdConsideration : UtilityAiConsideration {
		
		public override string Description =>
			"Scores by summing child Appraisals until a child scores below the threshold.\neach in Appraisals, sum += Appraisal, util Appraisal < Threshold, return sum.";
		
		public readonly float Threshold;
		
		public readonly ImmutableArray< IAppraisal > Appraisals = ImmutableArray< IAppraisal >.Empty;
		
		public ThresholdConsideration() => Threshold = 1f;

		public ThresholdConsideration( float threshold ) => Threshold = threshold;

		public override float GetScore( World world, Entity entity ) {
#if DEBUG
			var sum = 0f;
			var returned = false;
			var returnedValue = sum;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var score = Appraisals[ i ].GetScore( world, entity );
				CacheDebugUnitScore( Appraisals[ i ], score );
				if ( score < Threshold ) {
					returned = true;
					returnedValue = sum;
				}

				sum += score;
			}

			if ( returned ) {
#if DEBUG
				CacheResultScore( returnedValue );
#endif
				return returnedValue;
			}

#if DEBUG
			CacheResultScore( sum );
#endif
			return sum;
#else
			var sum = 0f;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var score = Appraisals[ i ].GetScore( world, entity );
				if ( score < Threshold ) {
					return sum;
				}

				sum += score;
			}

			return sum;
#endif
		}

		public override (float, float) GetPreictedScores() {
			var max = 0f;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var scores = Appraisals[ i ].GetPredictedScores();
				max += scores.Item2;
			}
			return ( 0, max );
		}
	}


	/// <summary>
	/// 返回Appraisals中最小的值
	/// </summary>
	public class MinScoreOfChildrenConsideration : UtilityAiConsideration {

		public override string Description => "Return the smallest value among children.\nreturn MinOf(Appraisals)";

		public readonly ImmutableArray< IAppraisal > Appraisals = ImmutableArray< IAppraisal >.Empty;
		
		public override float GetScore( World world, Entity entity ) {
			var min = 0f;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var score = Appraisals[ i ].GetScore( world, entity );
#if DEBUG
				CacheDebugUnitScore( Appraisals[ i ], score );
#endif
				if ( Numeric.IsLess( score, min ) ) {
					min = score;
				}
			}

#if DEBUG
			CacheResultScore( min );
#endif
			return min;
		}

		public override (float, float) GetPreictedScores() {
			var min = 0f;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var scores = Appraisals[ i ].GetPredictedScores();
				if ( scores.Item1 < min ) {
					min = scores.Item1;
				}
				if ( scores.Item2 < min ) {
					min = scores.Item2;
				}
			}
			return ( min, 0 );
		}
		
	}
	
	
	/// <summary>
	/// 返回Appraisals中最大的值
	/// </summary>
	public class MaxScoreOfChildrenConsideration : UtilityAiConsideration {
		
		public override string Description => "Return the maximum value among children.\nreturn MaxOf(Appraisals)";

		
		public readonly ImmutableArray< IAppraisal > Appraisals = ImmutableArray< IAppraisal >.Empty;
		
		public override float GetScore( World world, Entity entity ) {
			var max = 0f;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var score = Appraisals[ i ].GetScore( world, entity );
#if DEBUG
				CacheDebugUnitScore( Appraisals[ i ], score );
#endif
				if ( Numeric.IsGreater( score, max ) ) {
					max = score;
				}
			}

#if DEBUG
			CacheResultScore( max );
#endif
			return max;
		}
		
		public override (float, float) GetPreictedScores() {
			var max = 0f;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var scores = Appraisals[ i ].GetPredictedScores();
				if ( scores.Item1 > max ) {
					max = scores.Item1;
				}
				if ( scores.Item2 > max ) {
					max = scores.Item2;
				}
			}
			return ( 0, max );
		}
	}


	/// <summary>
	/// 返回Appraisals中的平均值
	/// </summary>
	public class TakeTheAverageOfChildrenConsideration : UtilityAiConsideration {

		public override string Description => "Returns the average value among children.\nreturn AverageOf(Appraisals)";

		public readonly ImmutableArray< IAppraisal > Appraisals = ImmutableArray< IAppraisal >.Empty;
		
		public override float GetScore( World world, Entity entity ) {
			var sum = 0f;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var score = Appraisals[ i ].GetScore( world, entity );
#if DEBUG
				CacheDebugUnitScore( Appraisals[ i ], score );
#endif
				sum += score;
			}

#if DEBUG
			CacheResultScore( Appraisals.Length != 0 ? sum / Appraisals.Length : sum );
#endif
			return Appraisals.Length != 0 ? sum / Appraisals.Length : sum;
		}
	}


	/// <summary>
	/// Appraisals中所有IAppraisal子项必须大于Threshold, 才返回OptionalAppraisals字段中计算总和, 否则返回0.
	/// </summary>
	public class SumOfChildrenWithPreAppraisalsConsideration : UtilityAiConsideration {
		
		public enum PreAppraisalsCheckMode : byte {
			AllRequired,
			AnySuffice
		}
		
		public override string Description => "if EachOf(Appraisals) > Threshold, return TheSumOf(OptionalAppraisals), else 0.";
		
		public readonly float Threshold;
		public readonly PreAppraisalsCheckMode PreCheckMode = PreAppraisalsCheckMode.AllRequired;
		public readonly ImmutableArray< IAppraisal > Appraisals = ImmutableArray< IAppraisal >.Empty;
		public readonly ImmutableArray< IAppraisal > OptionalAppraisals = ImmutableArray< IAppraisal >.Empty;

		public override float GetScore( World world, Entity entity ) {
#if DEBUG
			var sum = 0f;
			var preCheckPassed = PreCheckMode is PreAppraisalsCheckMode.AllRequired ? true : false;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var score = Appraisals[ i ].GetScore( world, entity );
				CacheDebugUnitScore( Appraisals[ i ], score );
				if ( PreCheckMode is PreAppraisalsCheckMode.AllRequired ) {
					if ( score < Threshold ) {
						preCheckPassed = false;
					}
				}
				else {
					if ( score >= Threshold ) {
						preCheckPassed = true;
					}
				}
			}
			
			foreach ( var optionalApp in OptionalAppraisals ) {
				var score = optionalApp.GetScore( world, entity );
				CacheDebugUnitScore( optionalApp, score );
				sum += score;
			}
			
			if ( !preCheckPassed ) {
				CacheResultScore( 0f );
				return 0f;
			}
			
			CacheResultScore( sum );
			return sum;
#else
			var sum = 0f;
			var preCheckPassed = PreCheckMode is PreAppraisalsCheckMode.AllRequired ? true : false;
			for ( var i = 0; i < Appraisals.Length; i++ ) {
				var score = Appraisals[ i ].GetScore( world, entity );
				if ( PreCheckMode is PreAppraisalsCheckMode.AllRequired ) {
					if ( score < Threshold ) {
						return 0f;
					}
				}
				else {
					if ( score >= Threshold ) {
						preCheckPassed = true;
					}
				}
			}

			if ( !preCheckPassed ) {
				return 0f;
			}

			foreach ( var optionalApp in OptionalAppraisals ) {
				var score = optionalApp.GetScore( world, entity );
				sum += score;
			}

			return sum;
#endif
		}

		public override (float, float) GetPreictedScores() {
			float min = default, max = default;
			foreach ( var optionalApp in OptionalAppraisals ) {
				var scores = optionalApp.GetPredictedScores();
				min += scores.Item1;
				max += scores.Item2;
			}
			return ( 0, max );
		}
	}

	#endregion
	

	public readonly struct UtilityAiAgentComponent : IModifiableComponent {
		
		[GameAssetId< UtilityAiAsset >]
		public readonly Guid UtilityAiAsset;
		
		public readonly UtilityAiEvaluateMethod EvaluateMethod = UtilityAiEvaluateMethod.Message; // Request Default.
		public readonly float EvaluateInterval = 1f; // 0 - EveryFrame, in seconds
		
		[System.Text.Json.Serialization.JsonIgnore, HideInEditor]
		public readonly UtilityAiReasoner Reasoner;

		[System.Text.Json.Serialization.JsonIgnore, HideInEditor]
		public readonly bool ReasonerChangedOnly;

		
		public UtilityAiAgentComponent() {}


		public UtilityAiAgentComponent( Guid utilityAiAsset ) {
			UtilityAiAsset = utilityAiAsset;
		}

		
		public UtilityAiAgentComponent( Guid utilityAiAsset, UtilityAiEvaluateMethod evaluateMethod, float evaluateInterval ) {
			UtilityAiAsset = utilityAiAsset;
			EvaluateMethod = evaluateMethod;
			EvaluateInterval = evaluateInterval;
		}
		
		
		public UtilityAiAgentComponent( Guid utilityAiAsset, UtilityAiEvaluateMethod evaluateMethod, float evaluateInterval, UtilityAiReasoner reasoner ) {
			UtilityAiAsset = utilityAiAsset;
			EvaluateMethod = evaluateMethod;
			EvaluateInterval = evaluateInterval;
			Reasoner = reasoner;
			ReasonerChangedOnly = true;
		}
		
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UtilityAiAsset? TryGetUtilityAiAsset() => Game.Data.TryGetAsset< UtilityAiAsset >( UtilityAiAsset );

		public void Subscribe( Action notification ) {}

		public void Unsubscribe( Action notification ) {}
	}


	[RuntimeOnly, DoNotPersistOnSave]
	public readonly struct UtilityAiReasonerComputeResultComponent : IComponent {

		public readonly UtilityAiAction Action;
		
		public UtilityAiReasonerComputeResultComponent( UtilityAiAction action ) {
			Action = action;
		}

	}


	[RuntimeOnly]
	public readonly struct UtilityAiEvaluateTimerComponent : IComponent {
			
		public readonly float Time;
		public UtilityAiEvaluateTimerComponent( float time ) => Time = time;
	}


	public readonly struct UtilityAiPausedEvaluateComponent : IComponent;


	public readonly struct RequestUtilityAiAgentEvaluateMessage : IMessage;


	public readonly struct UtilityAiEvaluateFinishedMessage : IMessage;


	[Watch( typeof( UtilityAiAgentComponent ) )]
	[Filter( ContextAccessorFilter.AnyOf, ContextAccessorKind.Read, typeof( UtilityAiAgentComponent ) )]
	[Filter( ContextAccessorFilter.NoneOf, typeof( UtilityAiPausedEvaluateComponent ) )]
	[Messager( typeof( RequestUtilityAiAgentEvaluateMessage ) )]
	public class UtilityAiComputeSystem : IFixedUpdateSystem, IReactiveSystem, IMessagerSystem, IMurderRenderSystem {

		public void FixedUpdate( Context context ) {
			foreach ( var entity in context.Entities ) {
				var agentComponent = entity.GetUtilityAiAgent();
				if ( agentComponent.EvaluateMethod is not UtilityAiEvaluateMethod.Interval ) {
					continue;
				}

				if ( !entity.HasUtilityAiEvaluateTimer() ) {
					entity.SetUtilityAiEvaluateTimer( 0f );
				}

				var time = entity.GetUtilityAiEvaluateTimer().Time;
				if ( time < 0f ) {
					Evaluate( context.World, entity );
					entity.SetUtilityAiEvaluateTimer( agentComponent.EvaluateInterval );
				}
				else {
					entity.SetUtilityAiEvaluateTimer( time - Game.FixedDeltaTime );
				}
			}
		}

		public void OnAdded( World world, ImmutableArray< Entity > entities ) {}

		public void OnRemoved( World world, ImmutableArray< Entity > entities ) {}

		public void OnModified( World world, ImmutableArray< Entity > entities ) {
			foreach ( var entity in entities ) {
				var utilityAiAgentComponent = entity.GetUtilityAiAgent();
				if ( utilityAiAgentComponent.ReasonerChangedOnly ) {
					continue;
				}
			
				// utilityAiAgentComponent.Reasoner = null;
				entity.SetUtilityAiAgent( new UtilityAiAgentComponent(
					utilityAiAgentComponent.UtilityAiAsset,
					utilityAiAgentComponent.EvaluateMethod,
					utilityAiAgentComponent.EvaluateInterval
				) );
			}
		}

		public void OnMessage( World world, Entity entity, IMessage message ) {
			if ( message is RequestUtilityAiAgentEvaluateMessage ) {
				Evaluate( world, entity );
			}
		}
		
		
		public void Draw( RenderContext render, Context context ) {

			// if ( context.World.TryGetUnique< EditorComponent >() is {} editorComponent && editorComponent.EditorHook != null && editorComponent.EditorHook.ShowStates ) {
			// 	var drawInfo = new DrawInfo( Color.Black, 0.2f ) { Outline = Color.Cyan, Scale = Vector2.One, Offset = new Vector2( 0, 0 ) };
			// 	
			// 	foreach ( var entity in context.Entities ) {
			// 		if ( entity.HasInCamera() && entity.TryGetUtilityAiReasonerComputeResult() is {} utilityAiReasonerComputeResultComponent ) {
			// 			var entityInScreen = entity.GetPosition().ToVector2() + new Vector2( 10f, 18f );
			// 			render.DebugBatch.DrawText( MurderFonts.PixelFont, $"{utilityAiReasonerComputeResultComponent.Action?.Name}", entityInScreen + new Vector2( 0, 0 ), drawInfo );
			// 		}
			// 	}
			// }
		}

		private void Evaluate( World world, Entity entity ) {
			var agentComponent = entity.GetUtilityAiAgent();
			if ( agentComponent.TryGetUtilityAiAsset() is { RootReasoner: not null } utilityAiAsset ) {
				
				bool createReasonerThisFrame = false;
				UtilityAiReasoner reasoner = agentComponent.Reasoner;
				if ( reasoner is null ) {
					reasoner = SerializationHelper.DeepCopy( utilityAiAsset.RootReasoner );
					createReasonerThisFrame = true;
				}
				
				reasoner.SetupContext( world, entity );
				var action = reasoner.Select();
				reasoner.ClearContext();
				
				if ( createReasonerThisFrame ) {
					entity.SetUtilityAiAgent( new UtilityAiAgentComponent(
						agentComponent.UtilityAiAsset,
						agentComponent.EvaluateMethod,
						agentComponent.EvaluateInterval,
						reasoner
					) );
				}
				
				entity.SetUtilityAiReasonerComputeResult( action );
				entity.SendMessage< UtilityAiEvaluateFinishedMessage >();
			}
			
		}

	}

	
	/// <summary>
	/// UtilityAiAction Map to AiAction for Executing.
	/// </summary>
	[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Read, typeof( UtilityAiAgentComponent ), typeof( AiActionExecutorComponent ) )]
	[Messager( typeof( UtilityAiEvaluateFinishedMessage ), typeof( AiActionExecutingFinishedMessage ) )]
	public class UtilityAiActionRouteSystem : IMessagerSystem {
		
		public void OnMessage( World world, Entity entity, IMessage message ) {
			if ( message is UtilityAiEvaluateFinishedMessage ) {
				var computeResultComponent = entity.GetUtilityAiReasonerComputeResult();
				if ( computeResultComponent.Action != null && entity.TryGetAiActionExecutor() is {} aiActionExecutorComponent ) {
					var matchedAction = aiActionExecutorComponent.FindAction( computeResultComponent.Action.Name );
					if ( matchedAction != null ) {
						// entity.ReplaceComponent( new AiActionExecutorComponent( aiActionExecutorComponent.AiActionScenarioAsset, matchedAction ), typeof( AiActionExecutorComponent ), true );
						entity.SetAiActionExecutor( aiActionExecutorComponent.AiActionScenarioAsset, matchedAction );
					}
				}
			}

			if ( message is AiActionExecutingFinishedMessage ) {
				// entity.SetAiActionExecutor( entity.GetAiActionExecutor().AiActionScenarioAsset, null );
				entity.SendMessage< RequestUtilityAiAgentEvaluateMessage >();
			}
		}
		
	}
}


namespace Pixpil.Assets {
	
	using Pixpil.AI;

	public class UtilityAiAsset : GameAsset {
	
		public override string EditorFolder => "#\uf564UtilityAi";

		public override char Icon => '\uf564';
    
		public override Vector4 EditorColor => "#ff79c6".ToVector4Color();


		[Bang.Serialize]
		public readonly UtilityAiReasoner RootReasoner;
		
		[Bang.Serialize]
		public ImmutableArray< UtilityAiAction > Actions = ImmutableArray< UtilityAiAction >.Empty;

	}

}


// namespace Murder.Serialization {
//
// 	using System.Text.Json.Serialization;
// 	using Pixpil.AI;
//
//
// 	[JsonSerializable( typeof( FirstScoreReasoner ) )]
// 	[JsonSerializable( typeof( HighestScoreReasoner ) )]
// 	[JsonSerializable( typeof( LowestScoreReasoner ) )]
// 	public partial class PixpilGamePrototypeSourceGenerationContext {}
//
// }
