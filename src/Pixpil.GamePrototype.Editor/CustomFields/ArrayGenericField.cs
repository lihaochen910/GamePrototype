using System;
using System.Collections.Generic;
using ImGuiNET;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Reflection;
using System.Diagnostics.CodeAnalysis;
using Murder;
using Murder.Editor.CustomFields;


namespace Pixpil.Editor.CustomFields; 

public abstract class ArrayGenericField< T > : CustomField {
    
	protected abstract bool Add(in EditorMember member, [NotNullWhen(true)] out T? element);

    public override (bool modified, object? result) ProcessInput( EditorMember member, object? fieldValue ) {
        bool modified = false;
        Array elements = ( Array )fieldValue!;
        if ( elements == null ) elements = Array.Empty< T >();

        ImGui.PushID( $"Add ${member.Member.ReflectedType}" );

        if ( Add( member, out T? element ) ) {
            var list = new List< T >( ( T[] )fieldValue );
            list.Add( element );
            elements = list.ToArray();
            modified = true;
        }

        ImGui.PopID();

        if ( modified || elements.Length == 0 ) {
            return ( modified, elements );
        }

        int maxLength = 128;
        for ( int index = 0; index < Math.Min( maxLength, elements.Length ); index++ ) {
            ImGui.PushID( $"{member.Member.ReflectedType}_{index}" );
            element = ( T )elements.GetValue( index );

            ImGui.BeginGroup();
            ImGuiHelpers.IconButton( '', $"{member.Name}_alter", Game.Data.GameProfile.Theme.Accent );

            if ( ImGui.IsItemHovered() ) {
                ImGui.OpenPopup( $"{member.Member.ReflectedType}_{index}_extras" );
                ImGui.SetNextWindowPos( ImGui.GetItemRectMin() + new System.Numerics.Vector2( -12, -2 ) );
            }

            if ( ImGui.BeginPopup( $"{member.Member.ReflectedType}_{index}_extras" ) ) {
                ImGui.BeginGroup();
                if ( ImGuiHelpers.IconButton( '', $"{member.Name}_remove", Game.Data.GameProfile.Theme.HighAccent ) ) {
                    var list = new List< T >( ( T[] )fieldValue );
                    list.RemoveAt( index );
                    elements = list.ToArray();
                    modified = true;
                }

                ImGuiHelpers.HelpTooltip( "Remove" );

                if ( ImGuiHelpers.IconButton( '', $"{member.Name}_duplicate", Game.Data.GameProfile.Theme.HighAccent ) ) {
                    var list = new List< T >( ( T[] )fieldValue );
                    list.Insert( index, ( T )elements.GetValue( index ) );
                    elements = list.ToArray();
                    modified = true;
                }

                ImGuiHelpers.HelpTooltip( "Duplicate" );

                if ( ImGuiHelpers.IconButton( '', $"{member.Name}_move_up",
                        Game.Data.GameProfile.Theme.HighAccent ) ) {
                    if ( index > 0 ) {
                        var list = new List< T >( ( T[] )fieldValue );
                        list.RemoveAt( index );
                        list.Insert( index - 1, ( T )elements.GetValue( index ) );
                        elements = list.ToArray();
                        modified = true;
                    }
                }

                ImGuiHelpers.HelpTooltip( "Move up" );

                if ( ImGuiHelpers.IconButton( '', $"{member.Name}_move_down",
                        Game.Data.GameProfile.Theme.HighAccent ) ) {
                    if ( index < elements.Length - 1 ) {
                        var list = new List< T >( ( T[] )fieldValue );
                        list.RemoveAt( index );
                        list.Insert( index + 1, ( T )elements.GetValue( index ) );
                        elements = list.ToArray();
                        modified = true;
                    }
                }

                ImGuiHelpers.HelpTooltip( "Move down" );

                ImGui.EndGroup();
                if ( !ImGui.IsWindowAppearing() && ImGui.IsWindowFocused() &&
                     !ImGui.IsMouseHoveringRect( ImGui.GetWindowPos(), ImGui.GetWindowPos() + ImGui.GetWindowSize() ) )
                    ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }

            ImGui.SameLine();

            if ( DrawElement( ref element, member, index ) ) {
                elements.SetValue( element!, index );
                modified = true;
            }

            ImGui.EndGroup();
            ImGui.PopID();
        }

        if ( elements.Length >= maxLength ) {
            ImGui.Text( $"List is too long ({elements.Length} items hidden)..." );
        }

        return ( modified, elements );
    }

    protected virtual bool DrawElement( ref T? element, EditorMember member, int index ) {
        if ( DrawValue( member.CreateFrom( typeof( T ), "Value", element: default ), element, out T? modifiedValue ) ) {
            element = modifiedValue;
            return true;
        }

        return false;
    }
}
