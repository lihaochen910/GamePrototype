using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using DigitalRune.Linq;
using ImGuiNET;
using Murder;
using Murder.Core.Graphics;
using Murder.Editor.Attributes;
using Murder.Editor.CustomComponents;
using Murder.Editor.CustomEditors;
using Murder.Editor.CustomFields;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Reflection;
using Murder.Editor.Utilities;
using Murder.Utilities;
using Pixpil.AI;
using Pixpil.Assets;
using Pixpil.Editor.CustomFields;
using Pixpil.GamePrototype.Editor.CustomComponents;
using ReflectionHelper = DigitalRune.Utility.ReflectionHelper;


namespace Pixpil.Editor.CustomEditors; 

[CustomEditorOf( typeof( UtilityAiAsset ) )]
internal class UtilityAiAssetEditor : CustomEditor {
	
	private UtilityAiAsset _utilityAiAsset = null!;
	private readonly EditorMember _rootReasonerMember;

	public override object Target => _utilityAiAsset;
	
	private Lazy< Dictionary< string, Type > > _reasonerTypes = new( () => {
		var types = ReflectionHelper.GetAllImplementationsOf< UtilityAiReasoner >();
		return CollectionHelper.ToStringDictionary( types, a => a.Name, a => a );
	} );

	private UtilityAiConsideration _currentRevealedConsideration;
	private bool _showOtherConsiderationsInConsiderationView = false;
	
	
	public UtilityAiAssetEditor() {
		_rootReasonerMember = EditorMember.Create( typeof( UtilityAiAsset ).GetField( nameof( UtilityAiAsset.RootReasoner ), BindingFlags.Public | BindingFlags.Instance ) );
	}
	
	private UtilityAiAsset GetOwnerUtilityAiAsset() => _utilityAiAsset;
	
	public override void OpenEditor( ImGuiRenderer imGuiRenderer, RenderContext _, object target, bool overwrite ) {
		_utilityAiAsset = ( UtilityAiAsset )target;
		OnRootReasonerChanged();
	}

	public override void CloseEditor( Guid target ) {
		if ( Game.Data.TryGetAsset< UtilityAiAsset >( target ) is {} utilityAiAsset ) {
			if ( _utilityAiAsset.RootReasoner != null ) {
				if ( _utilityAiAsset.RootReasoner.DefaultConsideration != null ) {
					_utilityAiAsset.RootReasoner.DefaultConsideration.FuncGetOwnerUtilityAiAsset = null;
				}
				foreach ( var consideration in _utilityAiAsset.RootReasoner.Considerations ) {
					if ( consideration != null ) {
						consideration.FuncGetOwnerUtilityAiAsset = null;
					}
				}
			}
		}
	}

	public override void DrawEditor() {
		
		if ( ImGui.BeginTabBar( "UtilityAiAsset_TabBar" ) ) {
			if ( ImGui.BeginTabItem( "Reasoner" ) ) {
				
				float editorSize = ImGui.GetContentRegionAvail().X * ( 3f / 5f );
				float debugRegionSize = ImGui.GetContentRegionAvail().X - editorSize;
				if ( ImGui.BeginTable( "UtilityAiReasonerWorkspace", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingFixedFit ) ) {
					ImGui.TableSetupColumn( "ReasonerEditor", ImGuiTableColumnFlags.WidthStretch, editorSize );
					ImGui.TableSetupColumn( "ReasonerDebug", ImGuiTableColumnFlags.WidthStretch, debugRegionSize );
					ImGui.TableNextRow();
					
					ImGui.TableNextColumn();
					{
						3.Times( _ => ImGui.Spacing() );
					
						ImGui.SeparatorText( "Root Reasoner" );
						ImGui.Indent( 12 );

						// SearchBox.PushWindowSize( new ( 350, 350 ) );
						SearchBox.SearchBoxSettings< Type > settings = new ( "Select a UtilityAiReasoner Type" );
						if ( _utilityAiAsset.RootReasoner != null ) {
							settings.InitialSelected = new SearchBox.InitialSelectedValue< Type >( _utilityAiAsset.RootReasoner.GetType().Name, _utilityAiAsset.RootReasoner.GetType() );
						}
						if ( SearchBox.Search( "sUtilityAiReasoner_new", settings, _reasonerTypes, SearchBoxFlags.None, out var reasonerTypeNew ) ) {

							bool isReplace = _utilityAiAsset.RootReasoner != null;
							if ( isReplace ) {
								var defaultConsideration = _utilityAiAsset.RootReasoner.DefaultConsideration;
								var considerations = _utilityAiAsset.RootReasoner.Considerations;
								var newRootReasoner = Activator.CreateInstance( reasonerTypeNew, null ) as UtilityAiReasoner;
								newRootReasoner.DefaultConsideration = defaultConsideration;
								newRootReasoner.Considerations = considerations;
								_utilityAiAsset.GetType().GetField( nameof( UtilityAiAsset.RootReasoner ), BindingFlags.Public | BindingFlags.Instance ).SetValue( _utilityAiAsset, newRootReasoner );
							}
							else {
								_utilityAiAsset.GetType().GetField( nameof( UtilityAiAsset.RootReasoner ), BindingFlags.Public | BindingFlags.Instance ).SetValue( _utilityAiAsset, Activator.CreateInstance( reasonerTypeNew, null ) as UtilityAiReasoner );
							}
							
							_utilityAiAsset.FileChanged = true;
						}
						// SearchBox.PopWindowSize();

						if ( _utilityAiAsset.RootReasoner is not null ) {
							ImGui.Separator();
							5.Times( _ => ImGui.Spacing() );
							ImGui.TextColored( Game.Profile.Theme.HighAccent, Prettify.FormatName( _utilityAiAsset.RootReasoner.GetType().Name ) );
							2.Times( _ => ImGui.Spacing() );
							ImGui.Indent( 12 );
							ImGui.Separator();
							ImGui.Spacing();
							ImGui.TextColored( Game.Profile.Theme.Faded, $"{'\uf0eb'} {_utilityAiAsset.RootReasoner.Description}" );
							ImGui.Spacing();
							ImGui.Separator();
							ImGui.Unindent( 12 );
							5.Times( _ => ImGui.Spacing() );
						}
						
						// ImGui.Indent( 36 );
						ImGui.BeginTable( "UtilityAiAsset_RootReasoner", 1, ImGuiTableFlags.SizingStretchProp );
						if ( CustomField.DrawValue( ref _utilityAiAsset, _rootReasonerMember ) ) {
							OnRootReasonerChanged();
							_utilityAiAsset.FileChanged = true;
						}
						ImGui.EndTable();
						// ImGui.Unindent( 36 );
						
						ImGui.Unindent( 12 );
						
						5.Times( _ => ImGui.Spacing() );
						ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.BgFaded );
						ImGui.SeparatorText( "Finish Reasoner Define" );
						ImGui.PopStyleColor();
						10.Times( _ => ImGui.Spacing() );
					}
					
					ImGui.TableNextColumn();
					{
						if ( _utilityAiAsset.RootReasoner is not null ) {
							ImGui.Separator();
							5.Times( _ => ImGui.Spacing() );
							ImGui.TextColored( Game.Profile.Theme.HighAccent, Prettify.FormatName( _utilityAiAsset.RootReasoner.GetType().Name ) );
							2.Times( _ => ImGui.Spacing() );
							ImGui.Indent( 12 );
							ImGui.Separator();
							ImGui.Spacing();
							ImGui.TextColored( Game.Profile.Theme.Faded, $"{'\uf0eb'} {_utilityAiAsset.RootReasoner.Description}" );
							ImGui.Spacing();
							ImGui.Separator();
							ImGui.Unindent( 12 );
							5.Times( _ => ImGui.Spacing() );
						
							ImGui.SeparatorText( "Default" );
							UtilityAiAgentComponentEditor.DrawDebugConsiderationNonRuntime( _utilityAiAsset.RootReasoner.DefaultConsideration );
							5.Times( _ => ImGui.Spacing() );
						
							ImGui.SeparatorText( "Considerations" );
							foreach ( var consideration in _utilityAiAsset.RootReasoner.Considerations ) {
								UtilityAiAgentComponentEditor.DrawDebugConsiderationNonRuntime( consideration );
								8.Times( _ => ImGui.Spacing() );
							}
						}
					}
					
					ImGui.EndTable();
				}
				ImGui.EndTabItem();
			}
			
			if ( ImGui.BeginTabItem( "Actions" ) ) {
				if ( !_utilityAiAsset.Actions.IsDefaultOrEmpty ) {

					if ( ImGui.BeginTable( "ActionsDefines", 2, ImGuiTableFlags.Borders ) ) {
						
						ImGui.TableSetupColumn( "Act", ImGuiTableColumnFlags.WidthFixed, 50 );
						ImGui.TableSetupColumn( "Name", ImGuiTableColumnFlags.WidthStretch );

						ImGui.TableHeadersRow();
						ImGui.TableNextColumn();

						for ( var i = 0; i < _utilityAiAsset.Actions.Length; i++ ) {
							var action = _utilityAiAsset.Actions[ i ];
							ImGui.PushID( $"UtilityAiAsset_ActionsDefine_{i}" );

							var actived = action.IsActived;
							if ( ImGui.Checkbox( string.Empty, ref actived ) ) {
								action.IsActived = actived;
								_utilityAiAsset.FileChanged = true;
							}
							
							if ( ImGuiHelpers.IconButton( '', $"UtilityAiAsset_{action.Name}_remove", Game.Data.GameProfile.Theme.HighAccent ) ) {
								_utilityAiAsset.Actions = _utilityAiAsset.Actions.Remove( action );
								_utilityAiAsset.FileChanged = true;
								break;
							}
							ImGuiHelpers.HelpTooltip( "Remove" );

							if ( i != 0 && ImGuiHelpers.IconButton( '', $"UtilityAiAsset_{action.Name}_move_up", Game.Data.GameProfile.Theme.HighAccent ) ) {
								if ( i > 0 ) {
									_utilityAiAsset.Actions = _utilityAiAsset.Actions.RemoveAt( i ).Insert( i - 1, _utilityAiAsset.Actions[ i ] );
									_utilityAiAsset.FileChanged = true;
									break;
								}
							}
							if ( i != 0 ) ImGuiHelpers.HelpTooltip( "Move up" );

							if ( i != _utilityAiAsset.Actions.Length - 1 && ImGuiHelpers.IconButton( '', $"UtilityAiAsset_{action.Name}_move_down", Game.Data.GameProfile.Theme.HighAccent ) ) {
								if ( i < _utilityAiAsset.Actions.Length - 1 ) {
									_utilityAiAsset.Actions = _utilityAiAsset.Actions.RemoveAt( i ).Insert( i + 1, _utilityAiAsset.Actions[ i ] );
									_utilityAiAsset.FileChanged = true;
									break;
								}
							}
							if ( i != _utilityAiAsset.Actions.Length - 1 ) ImGuiHelpers.HelpTooltip( "Move down" );
							
							ImGui.TableNextColumn();
							
							var name = action.Name;
							// ImGui.SetNextItemWidth( 300 );
							ImGui.PushID( $"UtilityAiAsset_ActionsDefine_name{i}" );
							if ( ImGui.InputText( string.Empty, ref name, 0xFF ) ) {
								action.Name = name;
								// action.GetType().GetField( nameof( UtilityAiAction.Name ), BindingFlags.Public | BindingFlags.Instance ).SetValue( action, name );
								_utilityAiAsset.FileChanged = true;
							}
							ImGui.PopID();
							ImGui.TableNextColumn();
							
							ImGui.PopID();
						}
						
						ImGui.EndTable();
					}
					
				}

				if ( ImGui.Button( "Add Action" ) ) {
					_utilityAiAsset.Actions = _utilityAiAsset.Actions.Add( new UtilityAiAction( "New Action" ) );
					_utilityAiAsset.FileChanged = true;
				}
				
				ImGui.EndTabItem();
			}

			if ( _utilityAiAsset.RootReasoner != null && ImGui.BeginTabItem( "Consideration" ) ) {
				
				Lazy< Dictionary< string, UtilityAiConsideration > > candidateConsiderationsDefine = new(() => {
					return CollectionHelper.ToStringDictionary( _utilityAiAsset.RootReasoner.Considerations, c => {
						var indexName = $"{_utilityAiAsset.RootReasoner.Considerations.IndexOf( c )}";
						var actionName = c.Action != null ? c.Action.Name : "nullptr";
						return $"{indexName}. {Prettify.FormatName( c.GetType().Name )} -> {actionName} ({c.Name})";
					}, c => c );
				} );

				if ( _currentRevealedConsideration != null &&
					 _utilityAiAsset.RootReasoner.Considerations.IndexOf( _currentRevealedConsideration ) < 0 ) {
					_currentRevealedConsideration = null;
				}

				var currentRevealedConsiderationName = _currentRevealedConsideration != null ? $"[{_utilityAiAsset.RootReasoner.Considerations.IndexOf( _currentRevealedConsideration )}] {_currentRevealedConsideration.GetType().Name}" : "Select a Consideration to reveal";
				SearchBox.SearchBoxSettings< UtilityAiConsideration > settings = new ( "Select a Consideration to reveal" );
				if ( _currentRevealedConsideration != null ) {
					settings.InitialSelected = new SearchBox.InitialSelectedValue< UtilityAiConsideration >( currentRevealedConsiderationName, _currentRevealedConsideration );
				}
				if ( SearchBox.Search( "sUtilityAiConsideration_Reveal", settings, candidateConsiderationsDefine, SearchBoxFlags.None, out var consideration ) ) {
					_currentRevealedConsideration = consideration;
				}
				
				ImGui.Separator();
				SearchBox.SearchBoxSettings< Type > settings2 = new ( "Add UtilityAiConsideration" );
				if ( SearchBox.Search( "sUtilityAiConsideration_", settings2, UtilityAiConsiderationField.UtilityAiConsiderationTypes, SearchBoxFlags.None, out var type ) ) {
					var newConsideration = Activator.CreateInstance( type, null ) as UtilityAiConsideration;
					_utilityAiAsset.RootReasoner.Considerations = _utilityAiAsset.RootReasoner.Considerations.Add( newConsideration );
					foreach ( var rc in _utilityAiAsset.RootReasoner.Considerations ) {
						if ( rc.FuncGetOwnerUtilityAiAsset != null ) {
							newConsideration.FuncGetOwnerUtilityAiAsset = rc.FuncGetOwnerUtilityAiAsset;
							break;
						}
					}
					_currentRevealedConsideration = newConsideration;
					_utilityAiAsset.FileChanged = true;
				}
				ImGui.Separator();
				
				if ( _currentRevealedConsideration != null ) {
					
					float editorSize = ImGui.GetContentRegionAvail().X * ( 3f / 5f );
					float debugRegionSize = ImGui.GetContentRegionAvail().X - editorSize;
					if ( ImGui.BeginTable( "UtilityAiReasonerWorkspace", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingFixedFit ) ) {
						ImGui.TableSetupColumn( "ReasonerEditor", ImGuiTableColumnFlags.WidthStretch, editorSize );
						ImGui.TableSetupColumn( "ReasonerDebug", ImGuiTableColumnFlags.WidthStretch, debugRegionSize );
						ImGui.TableNextRow();

						ImGui.TableNextColumn();
						{
							ImGui.Text( "Replace With:" );
							var ( replaced, newConsideration ) = UtilityAiConsiderationField.DrawReplaceUtilityAiConsiderationSearchBox( _currentRevealedConsideration );
							if ( replaced ) {
								var currentIndex = _utilityAiAsset.RootReasoner.Considerations.IndexOf( _currentRevealedConsideration );
								_utilityAiAsset.RootReasoner.Considerations = _utilityAiAsset.RootReasoner.Considerations.RemoveAt( currentIndex ).Insert( currentIndex, newConsideration );
								_utilityAiAsset.FileChanged = true;
							}
						
							5.Times( _ => ImGui.Spacing() );
							
							ImGui.TextColored( Game.Profile.Theme.Accent, Prettify.FormatName( _currentRevealedConsideration.GetType().Name ) );
							ImGui.SameLine();
							ImGui.Text( "->" );
							ImGui.SameLine();
							ImGui.TextColored( Game.Profile.Theme.Green, _currentRevealedConsideration.Action != null ? _currentRevealedConsideration.Action.Name : "nullptr" );

							if ( _currentRevealedConsideration.Action != null ) {
								ImGui.SameLine();
								2.Times( _ => { ImGui.Spacing(); ImGui.SameLine(); } );
								ImGui.SetNextItemWidth( ImGui.GetContentRegionAvail().X );
								ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Yellow );
								var considerationName = _currentRevealedConsideration.Name;
								if ( ImGui.InputTextWithHint( string.Empty, "Add some tips for this consideration!", ref considerationName, 0xFF ) ) {
									_currentRevealedConsideration.Name = considerationName;
									_utilityAiAsset.FileChanged = true;
								}
								ImGui.PopStyleColor();
							}
							
							2.Times( _ => ImGui.Spacing() );
							ImGui.Indent( 12 );
							ImGui.Separator();
							ImGui.Spacing();
							ImGui.TextColored( Game.Profile.Theme.Faded, $"{'\uf0eb'} {_currentRevealedConsideration.Description}" );
							ImGui.Spacing();
							ImGui.Separator();
							ImGui.Spacing();
							UtilityAiConsiderationField.DrawConsiderationPredictedScore( _currentRevealedConsideration );
							ImGui.Spacing();
							ImGui.Separator();
							ImGui.Unindent( 12 );
					
							3.Times( _ => ImGui.Spacing() );

							bool hasValidAction = _currentRevealedConsideration.Action != null;
							if ( hasValidAction ) {
								ImGui.SeparatorText( $"{'\uf6e2'} Action" );
							}
							else {
								ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Red );
								ImGui.SeparatorText( $"{'\uf071'} Assign Action For The Consideration First!" );
								ImGui.PopStyleColor();
							}
							// ImGui.Text( "Action:" );
							if ( _currentRevealedConsideration.FuncGetOwnerUtilityAiAsset is not null ) {
								var utilityAiAsset = _currentRevealedConsideration.FuncGetOwnerUtilityAiAsset.Invoke();
								Lazy< Dictionary< string, UtilityAiAction > > candidateActionDefine = new(() => {
									return CollectionHelper.ToStringDictionary( utilityAiAsset.Actions, a => a.Name, a => a );
								} );
						
								// ImGui.SameLine();
								ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Green );
								SearchBox.SearchBoxSettings< UtilityAiAction > settings3 = new ( "Select a UtilityAiAction" );
								if ( _currentRevealedConsideration.Action != null ) {
									settings3.InitialSelected = new SearchBox.InitialSelectedValue< UtilityAiAction >( _currentRevealedConsideration.Action.Name, _currentRevealedConsideration.Action );
								}
								if ( SearchBox.Search( "sUtilityAiConsideration_Action", settings3, candidateActionDefine, SearchBoxFlags.None, out var action ) ) {
									_currentRevealedConsideration.Action = action;
									_utilityAiAsset.FileChanged = true;
								}
								ImGui.PopStyleColor();
							}
							else {
								// ImGui.SameLine();
								ImGui.TextColored( Game.Profile.Theme.Red, "nullptr" );
							}

							if ( hasValidAction ) {
								ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Green );
								ImGui.SeparatorText( $"{'\uf00c'}" );
								ImGui.PopStyleColor();
							}
							else {
								ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Red );
								ImGui.SeparatorText( $"{'\uf00d'}" );
								ImGui.PopStyleColor();
							}
							2.Times( _ => ImGui.Spacing() );
							
							ImGui.BeginTable( "UtilityAiAsset_RootReasoner_Reveal", 1, ImGuiTableFlags.SizingStretchProp );
							if ( CustomComponent.DrawAllMembers( _currentRevealedConsideration, [ nameof( UtilityAiConsideration.Action ), nameof( UtilityAiConsideration.Name ) ] ) ) {
								OnRootReasonerChanged();
								_utilityAiAsset.FileChanged = true;
							}
							ImGui.EndTable();
							
							5.Times( _ => ImGui.Spacing() );
							ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.BgFaded );
							ImGui.SeparatorText( "Finish Reasoner Define" );
							ImGui.PopStyleColor();
							10.Times( _ => ImGui.Spacing() );
							
							
						}
						
						ImGui.TableNextColumn();
						{
							ImGui.Separator();
							5.Times( _ => ImGui.Spacing() );
							ImGui.TextColored( Game.Profile.Theme.HighAccent, Prettify.FormatName( _utilityAiAsset.RootReasoner.GetType().Name ) );
							2.Times( _ => ImGui.Spacing() );
							ImGui.Indent( 12 );
							ImGui.Separator();
							ImGui.Spacing();
							ImGui.TextColored( Game.Profile.Theme.Faded, $"{'\uf0eb'} {_utilityAiAsset.RootReasoner.Description}" );
							ImGui.Spacing();
							ImGui.Separator();
							ImGui.Unindent( 12 );
							5.Times( _ => ImGui.Spacing() );

							ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Faded );
							ImGui.Checkbox( "Show Other Consideration", ref _showOtherConsiderationsInConsiderationView );
							ImGui.PopStyleColor();
							5.Times( _ => ImGui.Spacing() );

							ImGui.SeparatorText( "Default" );
							if ( _utilityAiAsset.RootReasoner.DefaultConsideration != null ) {
								UtilityAiAgentComponentEditor.DrawDebugConsiderationNonRuntime( _utilityAiAsset.RootReasoner.DefaultConsideration );
								5.Times( _ => ImGui.Spacing() );
							}
							else {
								ImGui.TextColored( Game.Profile.Theme.Faded, "No Default Consideration." );
								5.Times( _ => ImGui.Spacing() );
							}
					
							ImGui.SeparatorText( "Current" );
							UtilityAiAgentComponentEditor.DrawDebugConsiderationNonRuntime( _currentRevealedConsideration );
							5.Times( _ => ImGui.Spacing() );

							if ( _showOtherConsiderationsInConsiderationView ) {
								ImGui.SeparatorText( "Other" );
								foreach ( var otherConsideration in _utilityAiAsset.RootReasoner.Considerations ) {
									if ( otherConsideration != _currentRevealedConsideration ) {
										UtilityAiAgentComponentEditor.DrawDebugConsiderationNonRuntime( otherConsideration );
										5.Times( _ => ImGui.Spacing() );
									}
								}
								5.Times( _ => ImGui.Spacing() );
							}
						}
						
						ImGui.EndTable();
						
					}
					
				}
				
				ImGui.EndTabItem();
			}

			// if ( _utilityAiAsset.RootReasoner != null && ImGui.BeginTabItem( "Reasoner Debug" ) ) {
			// 	
			// }
			
			ImGui.EndTabBar();
		}
		
	}

	private void OnRootReasonerChanged() {
		if ( _utilityAiAsset is { RootReasoner: not null } ) {
			if ( _utilityAiAsset.RootReasoner.DefaultConsideration != null ) {
				_utilityAiAsset.RootReasoner.DefaultConsideration.FuncGetOwnerUtilityAiAsset = GetOwnerUtilityAiAsset;
			}
			foreach ( var consideration in _utilityAiAsset.RootReasoner.Considerations ) {
				if ( consideration != null ) {
					consideration.FuncGetOwnerUtilityAiAsset = GetOwnerUtilityAiAsset;
				}
			}
		}
	}
}
