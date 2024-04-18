using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ImGuiNET;
using Murder;
using Murder.Editor.CustomFields;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Reflection;


namespace Pixpil.Editor.CustomFields; 

public abstract class StackField<T> : CustomField {

	protected abstract bool Push( in EditorMember member, [NotNullWhen( true )] out T? element );
    
    public override (bool modified, object? result) ProcessInput( EditorMember member, object? fieldValue ) {
        bool modified = false;
        Stack< T > elements = ( Stack< T > )fieldValue!;

        if ( modified || elements is null || elements.Count == 0 ) {
            return ( modified, elements );
        }
        
        ImGui.PushID( $"Push ${member.Member.ReflectedType}" );

        if ( Push( member, out T? element ) ) {
            elements.Push( element );
            modified = true;
        }

        ImGui.PopID();
        
        ImGui.Text( "Stack Top:" );
        if ( elements.Count > 0 ) {

            ImGui.PushID( $"{member.Member.ReflectedType}_0" );
            element = elements.Peek();

            ImGui.BeginGroup();
            ImGuiHelpers.IconButton( '', $"{member.Name}_alter", Game.Data.GameProfile.Theme.Accent );

            if ( ImGui.IsItemHovered() ) {
                ImGui.OpenPopup( $"{member.Member.ReflectedType}_{0}_extras" );
                ImGui.SetNextWindowPos( ImGui.GetItemRectMin() + new System.Numerics.Vector2( -12, -2 ) );
            }

            if ( ImGui.BeginPopup( $"{member.Member.ReflectedType}_{0}_extras" ) ) {
                ImGui.BeginGroup();
                if ( ImGuiHelpers.IconButton( '', $"{member.Name}_pop", Game.Data.GameProfile.Theme.HighAccent ) ) {
                    elements.Pop();
                    modified = true;
                }

                ImGuiHelpers.HelpTooltip( "Remove" );

                if ( ImGuiHelpers.IconButton( '', $"{member.Name}_duplicate",
                        Game.Data.GameProfile.Theme.HighAccent ) ) {
                    elements.Push( element );
                    modified = true;
                }

                ImGuiHelpers.HelpTooltip( "Duplicate" );

                ImGui.EndGroup();
                if ( !ImGui.IsWindowAppearing() && ImGui.IsWindowFocused() &&
                     !ImGui.IsMouseHoveringRect( ImGui.GetWindowPos(), ImGui.GetWindowPos() + ImGui.GetWindowSize() ) )
                    ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }

            ImGui.SameLine();

            if ( DrawElement( ref element, member ) ) {
                modified = true;
            }

            ImGui.EndGroup();
            ImGui.PopID();
        } 

        return ( modified, elements );
    }

    protected virtual bool DrawElement( ref T? element, EditorMember member ) {
        if ( DrawValue( member.CreateFrom( typeof( T ), "Value", element: default ), element, out T? modifiedValue ) ) {
            element = modifiedValue;
            return true;
        }

        return false;
    }
}
