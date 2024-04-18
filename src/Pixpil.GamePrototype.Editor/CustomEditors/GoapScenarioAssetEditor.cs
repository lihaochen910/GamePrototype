using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using DigitalRune.Linq;
using DigitalRune.Utility;
using ImGuiNET;
using Murder;
using Murder.Core.Graphics;
using Murder.Editor.Attributes;
using Murder.Editor.CustomComponents;
using Murder.Editor.CustomEditors;
using Murder.Editor.CustomFields;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Reflection;
using Murder.Utilities;
using Pixpil.AI;
using Pixpil.Assets;


namespace Pixpil.Editor.CustomEditors; 

[CustomEditorOf( typeof( GoapScenarioAsset ) )]
internal class GoapScenarioAssetEditor : CustomEditor {
	
	private GoapScenarioAsset _goapScenarioAsset = null!;
	private EditorMember _conditionDefinesMember;

	public override object Target => _goapScenarioAsset;
	
	private Lazy< Dictionary< string, Type > > _actionImplTypes = new( () => {
		var actionImplTypes = ReflectionHelper.GetAllImplementationsOf< GoapAction >();
		return CollectionHelper.ToStringDictionary( actionImplTypes, a => a.Name, a => a );
	} );

	private Lazy< Dictionary< string, Type > > _conditionImplTypes = new(() => {
		var conditionImplTypes = ReflectionHelper.GetAllImplementationsOf< GoapCondition >();
		return CollectionHelper.ToStringDictionary( conditionImplTypes, a => a.Name, a => a );
	} );

	public GoapScenarioAssetEditor() {
		_conditionDefinesMember = EditorMember.Create( typeof( GoapScenarioAsset ).GetField( nameof( GoapScenarioAsset.ConditionDefines ), BindingFlags.Public | BindingFlags.Instance ) );
	}

	public override void OpenEditor( ImGuiRenderer imGuiRenderer, RenderContext _, object target, bool overwrite ) {
		_goapScenarioAsset = ( GoapScenarioAsset )target;
	}

	public override void DrawEditor() {
		
		Lazy< Dictionary< string, string > > candidateConditionDefine = new(() => {
			IEnumerable< string > conditionDefines = _goapScenarioAsset.ConditionDefines;
			return CollectionHelper.ToStringDictionary( conditionDefines, a => a, a => a );
		});
		
		ImGui.SeparatorText( "Condition Defines" );
		ImGui.Indent( 12 );
		bool showConditionDefine = true;
		{
			if ( CustomField.DrawValue( ref _goapScenarioAsset, _conditionDefinesMember ) ) {
				_goapScenarioAsset.FileChanged = true;
			}
			
			if ( ImGui.BeginTabBar( "GoapCondition Configs" ) ) {
				if ( ImGui.BeginTabItem( "Defines" ) ) {
					showConditionDefine = true;
					ImGui.EndTabItem();
				}
				if ( ImGui.BeginTabItem( "Impl" ) ) {
					showConditionDefine = false;
					ImGui.EndTabItem();
				}
				ImGui.EndTabBar();
			}

			if ( showConditionDefine ) {
				if ( !_goapScenarioAsset.ConditionDefines.IsDefaultOrEmpty ) {
				
					ImGui.BeginTable( "ConditionDefines", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp );
					ImGui.TableSetupColumn( "Id" );
					ImGui.TableSetupColumn( "Name" );
					ImGui.TableSetupColumn( "Operating" );

					ImGui.TableHeadersRow();
					ImGui.TableNextColumn();
					
					for ( var i = 0; i < _goapScenarioAsset.ConditionDefines.Length; i++ ) {
						var condition = _goapScenarioAsset.ConditionDefines[ i ];
						var name = condition;
						ImGui.PushID( $"ConditionDefine_{i}" );
						
						ImGui.TextColored( Game.Profile.Theme.Accent, $"{i}" );
						ImGui.TableNextColumn();
						
						ImGui.SetNextItemWidth( ImGui.GetContentRegionAvail().X );
						ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Yellow );
						if ( ImGui.InputText( string.Empty, ref name, 0xFF ) ) {
							_goapScenarioAsset.ConditionDefines = _goapScenarioAsset.ConditionDefines.RemoveAt( i ).Insert( i, name );
							ModifyConditionDefine( _goapScenarioAsset, condition, name );
							_goapScenarioAsset.FileChanged = true;
							break;
						}
						ImGui.PopStyleColor();
						ImGui.TableNextColumn();
						
						if ( ImGuiHelpers.IconButton( '', $"{condition}_remove", Game.Data.GameProfile.Theme.HighAccent ) ) {
							_goapScenarioAsset.ConditionDefines = _goapScenarioAsset.ConditionDefines.Remove( condition );
							ModifyConditionDefine( _goapScenarioAsset, condition, null );
							_goapScenarioAsset.FileChanged = true;
							break;
						}
						ImGuiHelpers.HelpTooltip( "Remove" );
						ImGui.SameLine();

						if ( i != 0 && ImGuiHelpers.IconButton( '', $"{condition}_move_up", Game.Data.GameProfile.Theme.HighAccent ) ) {
							_goapScenarioAsset.ConditionDefines = _goapScenarioAsset.ConditionDefines
								.RemoveAt( i ).Insert( i - 1, _goapScenarioAsset.ConditionDefines[ i ] );
							_goapScenarioAsset.FileChanged = true;
							break;
						}
						if ( i != 0 ) ImGuiHelpers.HelpTooltip( "Move up" );
						ImGui.SameLine();

						if ( i != _goapScenarioAsset.ConditionDefines.Length - 1 && ImGuiHelpers.IconButton( '', $"{condition}_move_down", Game.Data.GameProfile.Theme.HighAccent ) ) {
							_goapScenarioAsset.ConditionDefines = _goapScenarioAsset.ConditionDefines
								.RemoveAt( i ).Insert( i + 1, _goapScenarioAsset.ConditionDefines[ i ] );
							_goapScenarioAsset.FileChanged = true;
							break;
						}
						if ( i != _goapScenarioAsset.ConditionDefines.Length - 1 ) ImGuiHelpers.HelpTooltip( "Move down" );
						ImGui.TableNextColumn();
						
						// if ( ImGui.BeginPopupContextItem( $"ConditionDefine_ContextItem_{i}" ) ) {
						// 	if ( ImGui.MenuItem( "Remove", string.Empty, false ) ) {
						// 		ImGui.CloseCurrentPopup();
						// 		_goapScenarioAsset.ConditionDefines = _goapScenarioAsset.ConditionDefines.Remove( condition );
						// 		_goapScenarioAsset.FileChanged = true;
						// 		break;
						// 	}
						// 	if ( ImGui.MenuItem( "Move up", string.Empty, false ) ) {
						// 		
						// 		break;
						// 	}
						// 	if ( ImGui.MenuItem( "Move down", string.Empty, false ) ) {
						// 		
						// 		break;
						// 	}
						// 	ImGui.EndPopup();
						// }
						
						ImGui.PopID();
					}
					
					ImGui.EndTable();
					
				}
				
				if ( ImGui.Button( "Add Condition" ) ) {
					_goapScenarioAsset.ConditionDefines = _goapScenarioAsset.ConditionDefines.Add( "New Condition" );
					_goapScenarioAsset.FileChanged = true;
				}
			}
			else {
				if ( !_goapScenarioAsset.ConditionDefines.IsDefaultOrEmpty ) {
			
					ImGui.BeginTable( "ConditionImplDefines", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp );
					ImGui.TableSetupColumn( "Id", ImGuiTableColumnFlags.WidthFixed );
					ImGui.TableSetupColumn( "Detail", ImGuiTableColumnFlags.WidthStretch );

					ImGui.TableHeadersRow();
					ImGui.TableNextColumn();

					foreach ( var conditionItem in _goapScenarioAsset.ConditionDefines ) {
						ImGui.PushID( $"ConditionImplDefine_{conditionItem.GetHashCode()}" );
						
						// ImGui.Text( $"{conditionItem}" );
						ImGui.TextColored( Game.Profile.Theme.Yellow, conditionItem );
						ImGui.TableNextColumn();

						var condImpl = _goapScenarioAsset.Conditions.ContainsKey( conditionItem )
							? _goapScenarioAsset.Conditions[ conditionItem ]
							: null;

						if ( condImpl is not null ) {
							if ( CustomComponent.ShowEditorOf( condImpl ) ) {
								_goapScenarioAsset.FileChanged = true;
							}
						}
				
						if ( SearchBox.Search( $"sConditionImplTypes_{conditionItem.GetHashCode()}", condImpl != null, condImpl != null ? condImpl.GetType().Name : "Select a type of GoapCondition", _conditionImplTypes, SearchBoxFlags.None, out var newConditionImplType ) ) {
							if ( newConditionImplType is null ) {
								var dict = _goapScenarioAsset.Conditions.ToDictionary();
								if ( dict.ContainsKey( conditionItem ) ) {
									dict.Remove( conditionItem );
								}
								_goapScenarioAsset.Conditions = dict.ToImmutableDictionary();
								_goapScenarioAsset.FileChanged = true;
							}
							else {
								condImpl = Activator.CreateInstance( newConditionImplType, null ) as GoapCondition;
								var dict = _goapScenarioAsset.Conditions.ToDictionary();
								dict[ conditionItem ] = condImpl;
								_goapScenarioAsset.Conditions = dict.ToImmutableDictionary();
								_goapScenarioAsset.FileChanged = true;
							}
						}
						ImGui.TableNextColumn();

						ImGui.PopID();
					}
			
					ImGui.EndTable();
			
				}
			}
			
		}
		ImGui.Unindent( 12 );
		
		10.Times( _ => ImGui.Spacing() );
		ImGui.SeparatorText( "Goals" );
		
		// if ( CustomField.DrawValue( ref _goapScenarioAsset, nameof( GoapScenarioAsset.Goals ) ) ) {
		// 	_goapScenarioAsset.FileChanged = true;
		// }

		// if ( ImGui.CollapsingHeader( "Goap Goals:", ImGuiTreeNodeFlags.DefaultOpen ) ) {
			if ( _goapScenarioAsset.Goals != ImmutableArray< GoapScenarioGoal >.Empty ) {
				
				ImGui.BeginTable( "GoalDefines", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit );
				ImGui.TableSetupColumn( "Goal", ImGuiTableColumnFlags.WidthFixed, 300 );
				ImGui.TableSetupColumn( "Necessary conditions", ImGuiTableColumnFlags.WidthStretch );
			
				ImGui.TableHeadersRow();
				ImGui.TableNextColumn();

				for ( var i = 0; i < _goapScenarioAsset.Goals.Length; i++ ) {
					ImGui.PushID( $"GoalDefine_{i}" );
					
					// ImGui.Text( $"{_goapScenarioAsset.ConditionDefines[ i ].Id}" );
					// ImGui.TableNextColumn();

					var goal = _goapScenarioAsset.Goals[ i ];
					
					var name = goal.Name;
					ImGui.PushID( $"GoalDefine_name{i}" );
					if ( ImGuiHelpers.DeleteButton( $"del_{name}" ) ) {
						_goapScenarioAsset.Goals = _goapScenarioAsset.Goals.Remove( goal );
						_goapScenarioAsset.FileChanged = true;
						continue;
					}
					ImGui.SameLine();
					ImGui.SetNextItemWidth( 250 );
					ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Green );
					if ( ImGui.InputText( string.Empty, ref name, 0xFF ) ) {
						goal.Name = name;
						_goapScenarioAsset.FileChanged = true;
					}
					ImGui.PopStyleColor();

					ImGui.Text( "\t\t" );
					ImGui.SameLine();
					bool isDefault = goal.IsDefault;
					if ( ImGui.Checkbox( nameof( goal.IsDefault ), ref isDefault ) ) {
						goal.IsDefault = isDefault;
						_goapScenarioAsset.FileChanged = true;
					}
					ImGui.PopID();
					ImGui.TableNextColumn();
					
					ImGui.SetNextItemWidth( 1000 );

					foreach ( var conditionKV in goal.Conditions ) {
						ImGui.PushID( $"GoalDefine_Condition_name{conditionKV.Key}" );
						var @checked = conditionKV.Value;
						if ( ImGui.Checkbox( string.Empty, ref @checked ) ) {
							var builder = ImmutableDictionary.CreateBuilder< string, bool >();
							if ( goal.Conditions != ImmutableDictionary< string, bool >.Empty ) {
								builder.AddRange( goal.Conditions );
							}
							builder[ conditionKV.Key ] = @checked;
							goal.Conditions = builder.ToImmutable();
							_goapScenarioAsset.FileChanged = true;
							break;
						}
						ImGui.PopID();
						ImGui.SameLine();
						
						if ( SearchBox.Search( $"sConditionItem_{conditionKV.Key}", true, conditionKV.Key, candidateConditionDefine, SearchBoxFlags.None, out var conditionItem ) ) {
							var builder = ImmutableDictionary.CreateBuilder< string, bool >();
							if ( goal.Conditions != null ) {
								builder.AddRange( goal.Conditions );
							}
							if ( conditionItem is null ) {
								builder.Remove( conditionKV.Key );
							}
							else {
								builder[ conditionItem ] = true;
							}
							goal.Conditions = builder.ToImmutable();
							_goapScenarioAsset.FileChanged = true;
							break;
						}
					}
					
					if ( SearchBox.Search( "sConditionItem_new", false, "Select a Condition", candidateConditionDefine, SearchBoxFlags.None, out var conditionItemNew ) ) {
						var builder = ImmutableDictionary.CreateBuilder< string, bool >();
						if ( goal.Conditions != null ) {
							builder.AddRange( goal.Conditions );
						}
						builder[ conditionItemNew ] = true;
						goal.Conditions = builder.ToImmutable();
						_goapScenarioAsset.FileChanged = true;
					}
					
					ImGui.TableNextColumn();
					
					ImGui.PopID();
				}
				
				ImGui.EndTable();
			}
			
			if ( ImGui.Button( "Add Goal" ) ) {
				_goapScenarioAsset.Goals = _goapScenarioAsset.Goals.Add( new GoapScenarioGoal { Name = "New Goal" } );
				_goapScenarioAsset.FileChanged = true;
			}
		// }
		
		
		10.Times( _ => ImGui.Spacing() );
		ImGui.SeparatorText( "Actions" );

		var showActionAndConditions = false;
		var showActionAndImpls = false;
		if ( ImGui.BeginTabBar( "Action Configs" ) ) {
			if ( ImGui.BeginTabItem( "Action & Condition" ) ) {
				showActionAndConditions = true;
				ImGui.EndTabItem();
			}
			else {
				showActionAndConditions = false;
			}
			
			if ( ImGui.BeginTabItem( "Action & Impl" ) ) {
				showActionAndImpls = true;
				ImGui.EndTabItem();
			}
			else {
				showActionAndImpls = false;
			}
			
			ImGui.EndTabBar();
		}
		
		if ( _goapScenarioAsset.Actions != ImmutableArray< GoapScenarioAction >.Empty ) {

			ImGui.BeginTable( "ActionsDefines", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp );
			if ( showActionAndConditions ) {
				ImGui.TableSetupColumn( "Act", ImGuiTableColumnFlags.WidthFixed, 50 );
				ImGui.TableSetupColumn( "Name", ImGuiTableColumnFlags.WidthFixed, 300 );
				ImGui.TableSetupColumn( "Cost", ImGuiTableColumnFlags.WidthFixed, 60 );
				ImGui.TableSetupColumn( "Conditions", ImGuiTableColumnFlags.WidthStretch );
			}
			else {
				ImGui.TableSetupColumn( "Act", ImGuiTableColumnFlags.WidthFixed, 50 );
				ImGui.TableSetupColumn( "Name", ImGuiTableColumnFlags.WidthFixed, 350 );
				ImGui.TableSetupColumn( "Cost", ImGuiTableColumnFlags.WidthFixed, 60 );
				ImGui.TableSetupColumn( "Impls", ImGuiTableColumnFlags.WidthStretch );
			}
			
			ImGui.TableHeadersRow();
			ImGui.TableNextColumn();
			
			for ( var i = 0; i < _goapScenarioAsset.Actions.Length; i++ ) {
				var action = _goapScenarioAsset.Actions[ i ];
				ImGui.PushID( $"ActionsDefine_{i}" );
				
				var actived = action.IsActived;
				if ( ImGui.Checkbox( string.Empty, ref @actived ) ) {
					action.IsActived = actived;
					_goapScenarioAsset.FileChanged = true;
				}
				
				if ( ImGuiHelpers.IconButton( '', $"{action.Name}_remove", Game.Data.GameProfile.Theme.HighAccent ) ) {
					_goapScenarioAsset.Actions = _goapScenarioAsset.Actions.Remove( action );
					_goapScenarioAsset.FileChanged = true;
					break;
				}
				ImGuiHelpers.HelpTooltip( "Remove" );

				if ( i != 0 && ImGuiHelpers.IconButton( '', $"{action.Name}_move_up", Game.Data.GameProfile.Theme.HighAccent ) ) {
					if ( i > 0 ) {
						_goapScenarioAsset.Actions = _goapScenarioAsset.Actions
							.RemoveAt( i ).Insert( i - 1, _goapScenarioAsset.Actions[ i ] );
						_goapScenarioAsset.FileChanged = true;
						break;
					}
				}
				if ( i != 0 ) ImGuiHelpers.HelpTooltip( "Move up" );

				if ( i != _goapScenarioAsset.Actions.Length - 1 && ImGuiHelpers.IconButton( '', $"{action.Name}_move_down", Game.Data.GameProfile.Theme.HighAccent ) ) {
					if ( i < _goapScenarioAsset.Actions.Length - 1 ) {
						_goapScenarioAsset.Actions = _goapScenarioAsset.Actions
							.RemoveAt( i ).Insert( i + 1, _goapScenarioAsset.Actions[ i ] );
						_goapScenarioAsset.FileChanged = true;
						break;
					}
				}
				if ( i != _goapScenarioAsset.Actions.Length - 1 ) ImGuiHelpers.HelpTooltip( "Move down" );
				
				ImGui.TableNextColumn();
				
				var name = action.Name;
				ImGui.SetNextItemWidth( 300 );
				ImGui.PushID( $"ActionsDefine_name{i}" );
				if ( ImGui.InputText( string.Empty, ref name, 0xFF ) ) {
					action.Name = name;
					_goapScenarioAsset.FileChanged = true;
				}
				ImGui.PopID();
				ImGui.TableNextColumn();

				var cost = action.Cost;
				ImGui.PushID( $"ActionsDefine_cost{i}" );
				if ( ImGui.DragInt( string.Empty, ref cost, 1 ) ) {
					action.Cost = cost;
					_goapScenarioAsset.FileChanged = true;
				}
				ImGui.PopID();
				ImGui.TableNextColumn();

				if ( showActionAndConditions ) {
					ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Yellow );
					ImGui.SeparatorText( "Pre" );
					ImGui.PopStyleColor();
					if ( action.Pre != ImmutableDictionary< string, bool >.Empty ) {
						foreach ( var preKV in action.Pre ) {
							var preCondChecked = preKV.Value;
							ImGui.PushID( $"ActionPreConditionItem_checked_{preKV.Key}" );
							if ( ImGui.Checkbox( string.Empty, ref preCondChecked ) ) {
								var builder = ImmutableDictionary.CreateBuilder< string, bool >();
								if ( action.Pre != ImmutableDictionary< string, bool >.Empty ) {
									builder.AddRange( action.Pre );
								}
								builder[ preKV.Key ] = preCondChecked;
								action.Pre = builder.ToImmutable();
								_goapScenarioAsset.FileChanged = true;
							}
							ImGui.PopID();
							ImGui.SameLine();
							if ( SearchBox.Search( $"sActionPreConditionItem_{preKV.Key}", true, $"{preKV.Key} \t#Pre", candidateConditionDefine, SearchBoxFlags.None, out var conditionItem ) ) {
								var builder = ImmutableDictionary.CreateBuilder< string, bool >();
								if ( action.Pre != null ) {
									builder.AddRange( action.Pre );
								}
								if ( conditionItem is null ) {
									builder.Remove( preKV.Key );
								}
								else {
									builder[ conditionItem ] = preCondChecked;
								}
								action.Pre = builder.ToImmutable();
								_goapScenarioAsset.FileChanged = true;
							}
						}
					}
					if ( SearchBox.Search( $"sActionPreConditionItem_new{i}_{name}", false, "Select a Pre Condition", candidateConditionDefine, SearchBoxFlags.None, out var conditionItemNew ) ) {
						var builder = ImmutableDictionary.CreateBuilder< string, bool >();
						if ( action.Pre != null ) {
							builder.AddRange( action.Pre );
						}
						builder[ conditionItemNew ] = true;
						action.Pre = builder.ToImmutable();
						_goapScenarioAsset.FileChanged = true;
					}
					ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Green );
					ImGui.SeparatorText( "Post" );
					ImGui.PopStyleColor();
					if ( action.Post != ImmutableDictionary< string, bool >.Empty ) {
						foreach ( var postKV in action.Post ) {
							var postCondChecked = postKV.Value;
							ImGui.PushID( $"ActionPostConditionItem_checked_{postKV.Key}" );
							if ( ImGui.Checkbox( string.Empty, ref postCondChecked ) ) {
								var builder = ImmutableDictionary.CreateBuilder< string, bool >();
								if ( action.Post != ImmutableDictionary< string, bool >.Empty ) {
									builder.AddRange( action.Post );
								}
								builder[ postKV.Key ] = postCondChecked;
								action.Post = builder.ToImmutable();
								_goapScenarioAsset.FileChanged = true;
							}
							ImGui.PopID();
							ImGui.SameLine();
							if ( SearchBox.Search( $"sActionPostConditionItem_{postKV.Key}", true, $"{postKV.Key} \t#Post", candidateConditionDefine, SearchBoxFlags.None, out var conditionItem ) ) {
								var builder = ImmutableDictionary.CreateBuilder< string, bool >();
								if ( action.Post != null ) {
									builder.AddRange( action.Post );
								}
								if ( conditionItem is null ) {
									builder.Remove( postKV.Key );
								}
								else {
									builder[ conditionItem ] = postCondChecked;
								}
								action.Post = builder.ToImmutable();
								_goapScenarioAsset.FileChanged = true;
							}
						}
					}
					if ( SearchBox.Search( $"sActionPostConditionItem_new{i}_{name}", false, "Select a Post Condition", candidateConditionDefine, SearchBoxFlags.None, out var conditionItemPostNew ) ) {
						var builder = ImmutableDictionary.CreateBuilder< string, bool >();
						if ( action.Post != null ) {
							builder.AddRange( action.Post );
						}
						builder[ conditionItemPostNew ] = true;
						action.Post = builder.ToImmutable();
						_goapScenarioAsset.FileChanged = true;
					}
					ImGui.TableNextColumn();
					
				}
				else {
					ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Yellow );
					ImGui.SeparatorText( nameof( GoapScenarioAction.ExecutePolicy ) );
					ImGui.PopStyleColor();
					ImGui.SameLine(); ImGui.Spacing(); ImGui.SameLine(); ImGui.Spacing(); ImGui.SameLine();
					if ( CustomField.DrawValue( ref action, nameof( GoapScenarioAction.ExecutePolicy ) ) ) {
						_goapScenarioAsset.FileChanged = true;
					}
					ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Green );
					ImGui.SeparatorText( "Actions" );
					ImGui.PopStyleColor();
					if ( !action.Impl.IsDefaultOrEmpty ) {
						if ( CustomField.DrawValue( ref action, nameof( GoapScenarioAction.Impl ) ) ) {
							_goapScenarioAsset.FileChanged = true;
						}
					}
					if ( SearchBox.Search( $"sActionImplItem_new{i}_{name}", false, "Select a GoapAction", _actionImplTypes, SearchBoxFlags.None, out var actionImplNew ) ) {
						action.Impl = action.Impl.Add( Activator.CreateInstance( actionImplNew, null ) as GoapAction );
						_goapScenarioAsset.FileChanged = true;
					}
					ImGui.TableNextColumn();
				}
				
				ImGui.PopID();
			}
			
			ImGui.EndTable();
		}
		
		if ( ImGui.Button( "Add Action" ) ) {
			_goapScenarioAsset.Actions = _goapScenarioAsset.Actions.Add( new GoapScenarioAction { Name = "New Action" } );
			_goapScenarioAsset.FileChanged = true;
		}
		
		10.Times( _ => ImGui.Spacing() );
		
	}


	private void ModifyConditionDefine( GoapScenarioAsset asset, string originDefine, string newDefine ) {
		foreach ( var action in asset.Actions ) {
			if ( newDefine is null ) {
				if ( action.Pre.ContainsKey( originDefine ) ) {
					action.Pre = action.Pre.Remove( originDefine );
				}
				if ( action.Post.ContainsKey( originDefine ) ) {
					action.Post = action.Post.Remove( originDefine );
				}
			}
			else {
				if ( action.Pre.ContainsKey( originDefine ) && !action.Pre.ContainsKey( newDefine ) ) {
					var value = action.Pre[ originDefine ];
					action.Pre = action.Pre.Remove( originDefine ).Add( newDefine, value );
				}
				if ( action.Post.ContainsKey( originDefine ) && !action.Post.ContainsKey( newDefine ) ) {
					var value = action.Post[ originDefine ];
					action.Post = action.Post.Remove( originDefine ).Add( newDefine, value );
				}
			}
		}

		foreach ( var goal in asset.Goals ) {
			if ( newDefine is null ) {
				if ( goal.Conditions.ContainsKey( originDefine ) ) {
					goal.Conditions = goal.Conditions.Remove( originDefine );
				}
			}
			else {
				if ( goal.Conditions.ContainsKey( originDefine ) && !goal.Conditions.ContainsKey( newDefine ) ) {
					var value = goal.Conditions[ originDefine ];
					goal.Conditions = goal.Conditions.Remove( originDefine ).Add( newDefine, value );
				}
			}
		}

		if ( newDefine is null ) {
			if ( asset.Conditions.ContainsKey( originDefine ) ) {
				asset.Conditions = asset.Conditions.Remove( originDefine );
			}
		}
		else {
			if ( asset.Conditions.ContainsKey( originDefine ) && !asset.Conditions.ContainsKey( newDefine ) ) {
				var value = asset.Conditions[ originDefine ];
				asset.Conditions = asset.Conditions.Remove( originDefine ).Add( newDefine, value );
			}
		}
	}
}
