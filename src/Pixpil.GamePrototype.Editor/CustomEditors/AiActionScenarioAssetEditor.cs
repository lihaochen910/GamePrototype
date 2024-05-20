using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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


namespace Pixpil.Editor.CustomEditors; 

[CustomEditorOf( typeof( AiActionScenarioAsset ) )]
internal class AiActionScenarioAssetEditor : CustomEditor {
	
	private AiActionScenarioAsset _aiScenarioAsset = null!;
	private EditorMember _actionDefinesMember;
	private AiActionScenario _currentRevealedAction;

	public override object Target => _aiScenarioAsset;
	
	private Lazy< Dictionary< string, Type > > _actionImplTypes = new( () => {
		var actionImplTypes = ReflectionHelper.GetAllImplementationsOf< AiAction >();
		return CollectionHelper.ToStringDictionary( actionImplTypes, a => a.Name, a => a );
	} );
	
	public AiActionScenarioAssetEditor() {
		_actionDefinesMember = EditorMember.Create( typeof( AiActionScenarioAsset ).GetField( nameof( AiActionScenarioAsset.Actions ), BindingFlags.Public | BindingFlags.Instance ) );
	}

	public override void OpenEditor( ImGuiRenderer imGuiRenderer, RenderContext _, object target, bool overwrite ) {
		_aiScenarioAsset = ( AiActionScenarioAsset )target;
	}

	public override void DrawEditor() {
		
		ImGui.SeparatorText( "Ai Actions" );
        
		if ( ImGui.BeginTabBar( "Ai Actions DrawMode" ) ) {
			if ( ImGui.BeginTabItem( "ListView" ) ) {
				
				if ( !_aiScenarioAsset.Actions.IsDefaultOrEmpty ) {

					ImGui.BeginTable( "ActionsDefines", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp );
					ImGui.TableSetupColumn( "Act", ImGuiTableColumnFlags.WidthFixed, 50 );
					ImGui.TableSetupColumn( "Name", ImGuiTableColumnFlags.WidthFixed, 400 );
					ImGui.TableSetupColumn( "Impls", ImGuiTableColumnFlags.WidthStretch );
					
					ImGui.TableHeadersRow();
					ImGui.TableNextColumn();
					
					for ( var i = 0; i < _aiScenarioAsset.Actions.Length; i++ ) {
						var action = _aiScenarioAsset.Actions[ i ];
						ImGui.PushID( $"ActionsDefine_{i}" );
						
						var actived = action.IsActived;
						if ( ImGui.Checkbox( string.Empty, ref @actived ) ) {
							action.IsActived = actived;
							_aiScenarioAsset.FileChanged = true;
						}
						
						if ( ImGuiHelpers.IconButton( '', $"{action.Name}_remove", Game.Data.GameProfile.Theme.HighAccent ) ) {
							_aiScenarioAsset.Actions = _aiScenarioAsset.Actions.Remove( action );
							_aiScenarioAsset.FileChanged = true;
							break;
						}
						ImGuiHelpers.HelpTooltip( "Remove" );

						if ( i != 0 && ImGuiHelpers.IconButton( '', $"{action.Name}_move_up", Game.Data.GameProfile.Theme.HighAccent ) ) {
							if ( i > 0 ) {
								_aiScenarioAsset.Actions = _aiScenarioAsset.Actions
									.RemoveAt( i ).Insert( i - 1, _aiScenarioAsset.Actions[ i ] );
								_aiScenarioAsset.FileChanged = true;
								break;
							}
						}
						if ( i != 0 ) ImGuiHelpers.HelpTooltip( "Move up" );

						if ( i != _aiScenarioAsset.Actions.Length - 1 && ImGuiHelpers.IconButton( '', $"{action.Name}_move_down", Game.Data.GameProfile.Theme.HighAccent ) ) {
							if ( i < _aiScenarioAsset.Actions.Length - 1 ) {
								_aiScenarioAsset.Actions = _aiScenarioAsset.Actions
									.RemoveAt( i ).Insert( i + 1, _aiScenarioAsset.Actions[ i ] );
								_aiScenarioAsset.FileChanged = true;
								break;
							}
						}
						if ( i != _aiScenarioAsset.Actions.Length - 1 ) ImGuiHelpers.HelpTooltip( "Move down" );
						
						ImGui.TableNextColumn();
						
						var name = action.Name ?? string.Empty;
						ImGui.SetNextItemWidth( 398 );
						ImGui.PushID( $"ActionsDefine_name{i}" );
						if ( ImGui.InputText( string.Empty, ref name, 0xFF ) ) {
							// action.Name = name;
							action.GetType().GetField( nameof( AiActionScenario.Name ), BindingFlags.Public | BindingFlags.Instance ).SetValue( action, name );
							_aiScenarioAsset.FileChanged = true;
						}
						ImGui.PopID();
						ImGui.TextColored( Game.Profile.Theme.Faded, $"#{action.GetHashCode():x8}" );
						ImGui.TableNextColumn();
						

						ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Yellow );
						ImGui.SeparatorText( nameof( GoapScenarioAction.ExecutePolicy ) );
						ImGui.PopStyleColor();
						ImGui.SameLine(); ImGui.Spacing(); ImGui.SameLine(); ImGui.Spacing(); ImGui.SameLine();
						if ( CustomField.DrawValue( ref action, nameof( GoapScenarioAction.ExecutePolicy ) ) ) {
							_aiScenarioAsset.FileChanged = true;
						}
						ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Green );
						ImGui.SeparatorText( "Actions" );
						ImGui.PopStyleColor();
						if ( !action.Impl.IsDefaultOrEmpty ) {
							if ( CustomField.DrawValue( ref action, nameof( GoapScenarioAction.Impl ) ) ) {
								_aiScenarioAsset.FileChanged = true;
							}
						}
						SearchBox.SearchBoxSettings< Type > settings = new ( "Select a AiActionScenario" );
						if ( SearchBox.Search( $"sActionImplItem_new{i}_{name}", settings, _actionImplTypes, SearchBoxFlags.None, out var actionImplNew ) ) {
							// action.Impl = ;
							action.GetType().GetField( nameof( AiActionScenario.Impl ), BindingFlags.Public | BindingFlags.Instance ).SetValue( action, action.Impl.Add( Activator.CreateInstance( actionImplNew, null ) as AiAction ) );
							_aiScenarioAsset.FileChanged = true;
						}
						ImGui.TableNextColumn();

						
						ImGui.PopID();
					}
					
					ImGui.EndTable();
				}
				
				ImGui.EndTabItem();
			}
			
			if ( ImGui.BeginTabItem( "Detail" ) ) {

				Lazy< Dictionary< string, AiActionScenario > > candidateActionDefine = new(() => {
					return CollectionHelper.ToStringDictionary( _aiScenarioAsset.Actions, a => $"{a.Name} #{a.GetHashCode():x8}", a => a );
				} );
                
				SearchBox.SearchBoxSettings< AiActionScenario > settings = new ( "Select a AiActionScenario to Reveal" );
				if ( _currentRevealedAction != null ) {
					settings.InitialSelected = new SearchBox.InitialSelectedValue< AiActionScenario >( $"{_currentRevealedAction.Name} #{_currentRevealedAction.GetHashCode():x8}", _currentRevealedAction );
				}
                if ( SearchBox.Search( "sAiActionScenarioItem", settings, candidateActionDefine, SearchBoxFlags.None, out var actionItem ) ) {
					_currentRevealedAction = actionItem;
				}

				if ( _currentRevealedAction != null ) {
					_aiScenarioAsset.FileChanged |= CustomComponent.ShowEditorOf( ref _currentRevealedAction, CustomComponentsFlags.None );
				}
				
				ImGui.EndTabItem();
			}
			
			ImGui.EndTabBar();
		}
		
		if ( ImGui.Button( "Add Action" ) ) {
			var newAction = new AiActionScenario( "New Action" );
			_currentRevealedAction = newAction;
			_aiScenarioAsset.Actions = _aiScenarioAsset.Actions.Add( newAction );
			_aiScenarioAsset.FileChanged = true;
		}
		
		10.Times( _ => ImGui.Spacing() );

	}
}
