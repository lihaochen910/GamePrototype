using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Bang.Components;
using Bang.Entities;
using Bang.StateMachines;
using Murder;
using Murder.Assets;
using Murder.Components;
using Murder.Core.Dialogs;
using Murder.Diagnostics;
using Murder.Messages;
using Murder.StateMachines;
using Murder.Utilities;
using Pixpil.Services;
using DialogueServices = Murder.Services.DialogueServices;


namespace Pixpil.StateMachines;

public enum MessageType : byte {
    Monologue = 0,
    Cellphone = 1
}


public enum InputType : byte {
    Time = 1,
    PauseGame = 2,
    Input = 3 // input without pause
}


public class MonologueStateMachine : DialogStateMachine {
    
    private CharacterRuntime? _character;
    private int? _choice;
    
    private readonly InputType _inputType = InputType.Time;
    private readonly MessageType _message = MessageType.Monologue;

    public MonologueStateMachine() {
        State( BeforeTalk );
    }

    public MonologueStateMachine( InputType inputType, MessageType message ) : this() {
        _inputType = inputType;
        _message = message;
    }

    [MemberNotNull( nameof( _character ) )]
    protected override void OnStart() {
        if ( Entity.TryGetSituation() is not SituationComponent situation ) {
            throw new ArgumentNullException( nameof( SituationComponent ) );
        }

        _character = DialogueServices.CreateCharacterFrom( situation.Character, situation.Situation );
        if ( _character is null ) {
            _character = null!;
            Entity.Destroy();
        }
    }

    public IEnumerator< Wait > BeforeTalk() {
        if ( _inputType is InputType.PauseGame ) {
            using PartiallyPauseGame freeze = new ( World );
            yield return Wait.ForRoutine( TalkMonologue() );
        }
        else {
            yield return Wait.ForRoutine( TalkMonologue() );
        }
    }

    public IEnumerator< Wait > TalkMonologue() {
        Debug.Assert( _character is not null );
        if ( _character is null ) {
            yield break;
        }

        while ( true ) {
            if ( _character.NextLine( World, Entity ) is not DialogLine dialogLine ) {
                // Entity.RemoveTriggeredEventTracker();

                // No line was ever added, destroy the dialog.
                if ( !Entity.HasMonologue() ) {
                    Entity.RemoveCustomDraw();
                    Entity.RemoveStateMachine();
                    yield break;
                }

                Entity.RemoveMonologue();
                // Entity.RemoveCellphoneLine();

                yield break;
            }

            if ( dialogLine.Line is Line line ) {
                if ( _message == MessageType.Monologue ) {
                    Entity.SetMonologue( line, _inputType );
                }
                // else if ( _message == MessageType.Cellphone ) {
                //     Entity.SetCellphoneLine( line, Game.NowUnscaled, FetchSpeaker( line.Speaker ) );
                // }

                if ( line.IsText ) {
                    yield return Wait.NextFrame;

                    yield return Wait.ForMessage< NextDialogMessage >();
                }
                else if ( line.Delay is float delay ) {
                    int ms = Calculator.RoundToInt( delay * 1000 );
                    yield return Wait.ForMs( ms );
                }
            }
            else if ( dialogLine.Choice is ChoiceLine choice ) {
                if ( _message == MessageType.Monologue ) {
                    Entity.SetMonologue( choice.Title, choice.Choices, _inputType );
                }
                // else if ( _message == MessageType.Cellphone ) {
                //     string speaker = FetchSpeaker( dialogLine.Line?.Speaker );
                //     Entity.SetCellphoneLine( choice.Title, choice.Choices, Game.NowUnscaled, speaker );
                // }

                yield return Wait.NextFrame;
                yield return Wait.ForMessage< PickChoiceMessage >();

                if ( _choice is not int choiceIndex ) {
                    GameLogger.Error( "How do we not track a choice made by the player?" );

                    Entity.Destroy();
                    yield break;
                }

                _character.DoChoice( choiceIndex, World, Entity );
            }
        }
    }

    private string FetchSpeaker( Guid? speakerGuid ) {
        Debug.Assert( _character is not null );

        Guid player = LibraryServices.GetLibrary().PlayerSpeaker;

        // if ( Entity.TryGetSituation() is SituationComponent situation && !string.IsNullOrEmpty( situation.Sender ) ) {
        //     return situation.Sender;
        // }
        if ( speakerGuid is not null && speakerGuid != player && Game.Data.TryGetAsset< SpeakerAsset >( speakerGuid.Value ) is SpeakerAsset speakerAsset ) {
            return speakerAsset.SpeakerName;
        }
        // else if ( Game.Data.TryGetAsset< SpeakerAsset >( _character.Owner ) is SpeakerAsset character ) {
        //     return character.SpeakerName;
        // }

        return string.Empty;
    }

    protected override void OnMessage( IMessage message ) {
        if ( message is PickChoiceMessage pickChoiceMessage ) {
            if ( pickChoiceMessage.IsCancel ) {
                _choice = null;
            }
            else {
                _choice = pickChoiceMessage.Choice;
            }
        }
    }
}
