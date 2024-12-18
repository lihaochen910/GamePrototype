using System;
using System.Collections.Generic;
using System.Numerics;
using Bang.Components;
using Bang.Entities;
using Bang.StateMachines;
using Murder;
using Murder.Assets;
using Murder.Assets.Graphics;
using Murder.Core;
using Murder.Core.Dialogs;
using Murder.Core.Geometry;
using Murder.Core.Graphics;
using Murder.Messages;
using Murder.Services;
using Murder.Utilities;
using Pixpil.Components;
using Pixpil.Core;
using Pixpil.Messages;
using Pixpil.Services;


namespace Pixpil.StateMachines;

public class MonologueUiStateMachine : StateMachine {
    
    private float _lastStartedTime = 0;
    private float _lastEndTime = 0;

    /// <summary>
    /// Whether it reached the end of the dialogue.
    /// </summary>
    private bool _reachedEnd = false;

    private bool _exit = false;

    /// <summary>
    /// Total time that the dialogue appears when player doesn't press skip.
    /// </summary>
    private readonly float _totalSecondsOnAutoSkip = 1.5f;

    private MonologueComponent? _monologue = null;

    // private SpeakerKind _speaker = SpeakerKind.Driver;

    private Entity? _entitySendingMessages = null;
    
    public MonologueUiStateMachine() {
        State( Main );
    }

    private IEnumerator< Wait > Main() {
        _lastStartedTime = Game.NowUnscaled;
        Entity.SetCustomDraw( DrawMessage );

        yield return GoTo( WaitInput );
    }

    private IEnumerator< Wait > WaitInput() {
        while ( true ) {
            if ( _exit || _monologue is not {} monologue ) {
                // World.TryGetUniqueEntity<DriverPortraitComponent>()?.RemovePaused();
                // World.TryGetUniqueEntity<GuestPortraitComponent>()?.RemovePaused();

                Entity.RemoveCustomDraw();
                Entity.RemoveStateMachine();

                yield break;
            }

            if ( monologue.InputType == InputType.PauseGame ) {
                // LDGameSoundPlayer.Instance.SetGlobalParameter(LibraryServices.GetRoadLibrary().MusicFocusParameter, 1);
                // _hadCarLoopSound |= LDGameSoundPlayer.Instance.Stop(LibraryServices.GetRoadLibrary().CarLoop, fadeOut: false);
            }

            if ( monologue.InputType == InputType.Time ) {
                yield return Wait.ForSeconds( _totalSecondsOnAutoSkip );

                _entitySendingMessages?.SendMessage< NextDialogMessage >();
                yield return GoTo( WaitNextMessage );
            }
            else if ( Game.Input.Pressed( InputButtons.Submit ) || Game.Input.Pressed( InputButtons.Interact ) ) {
                if ( _reachedEnd ) {
                    // Let's handcraft a delay so the player doesn't immediately get out of the
                    // dialog screen.
                    float timeSinceReachedEnd = Game.NowUnscaled - _lastEndTime;
                    if ( timeSinceReachedEnd > .1f ) {
                        _entitySendingMessages?.SendMessage< NextDialogMessage >();
                        yield return GoTo( WaitNextMessage );
                    }
                }
                else {
                    _reachedEnd = true;
                    _lastEndTime = Game.NowUnscaled;
                }

                Game.Input.Consume( InputButtons.Submit );
                Game.Input.Consume( InputButtons.Interact );
            }

            yield return Wait.NextFrame;
        }
    }

    private IEnumerator< Wait > WaitNextMessage() {
        yield return Wait.ForMessage< DialogueMessage >();
        yield return GoTo( WaitInput );
    }

    private float _playNextSound;
    private void DrawMessage( RenderContext render ) {
        if ( _monologue is not {} monologue ) {
            return;
        }

        Line line = monologue.Line;
        if ( line.Text is null ) {
            return;
        }
        
        string localizedString = LocalizationServices.TryGetLocalizedString( line.Text ) ?? string.Empty;

        float timeSinceAppeared = Game.NowUnscaled - _lastStartedTime;
        int currentLength = Calculator.CeilToInt( Calculator.ClampTime( timeSinceAppeared, .8f /* dialog duration */ ) *
                                  localizedString.Length );

        if ( _reachedEnd || currentLength == localizedString.Length ) {
            if ( !_reachedEnd ) {
                _lastEndTime = Game.NowUnscaled;
            }

            _reachedEnd = true;

            currentLength = localizedString.Length;
        }

        const int maxWidth = 200;

        float dialogWidth = 20 /* padding */ + Math.Min(
            Game.Data.GetFont( ( int )PPFonts.FusionPixel ).GetLineWidth( localizedString.AsSpan().Slice( 0, currentLength ) ), maxWidth );

        Vector2 textDialogueOffset = new ( 80, 108 );

        // bool isRightSpeaker = _speaker != SpeakerKind.Driver;
        // if ( isRightSpeaker ) {
        //     textDialogueOffset.X = render.Camera.Width - 220;
        // }

        if ( monologue.InputType == InputType.PauseGame ) {
            // If this is a pause, show that to the played by darkening the background.
            render.UiBatch.DrawRectangle( new( 0, 0, render.Camera.Width, render.Camera.Height ),
                Color.Black * 0.8f, 0.5f );
        }

        float sort = monologue.InputType == InputType.PauseGame ? 0.29f : .5f;

        Rectangle sliceSize = new ( 44 + textDialogueOffset.X, textDialogueOffset.Y - 9, dialogWidth + 8, 38 );
        Vector2 textPosition = sliceSize.TopLeft + new Vector2( 13, 19 );

        // if ( isRightSpeaker ) {
        //     textPosition.X = sliceSize.X - sliceSize.Width + 30;
        //     sliceSize.X += 19 - sliceSize.Width;
        // }

        // Draw(Batch2D spriteBatch, string text, Vector2 position, float sort, Color color, Color? strokeColor, Color? shadowColor, int maxWidth)
        // TODO: Break lines.
        // Game.Data.GetFont( ( int )PPFonts.FusionPixel ).Draw()
        
        var drawInfo = new DrawInfo {
            Sort = sort,
            Color = Palette.Colors[ 12 ],
            Shadow = Palette.Colors[ 21 ],
            Debug = true
        };
        var textSize = render.UiBatch.DrawText(
            font: PPFonts.FusionPixel,
            text: localizedString,
            position: textPosition,
            maxWidth: maxWidth,
            visibleCharacters: currentLength,
            drawInfo: drawInfo
        );
        
        if ( _playNextSound < Game.NowUnscaled && currentLength < localizedString.Length ) {
            // if ( isRightSpeaker ) {
            //     LDGameSoundPlayer.Instance.PlayEvent(
            //         _speaker == SpeakerKind.Granny
            //             ? LibraryServices.GetRoadLibrary().GrandmaTextBeep
            //             : LibraryServices.GetRoadLibrary().CarTextBeep, isLoop: false );
            // }
            // else {
            //     LDGameSoundPlayer.Instance.PlayEvent( LibraryServices.GetRoadLibrary().DaggersTextBeep, isLoop: false );
            // }

            _playNextSound = Game.NowUnscaled + 0.09f;
        }


        if ( textSize.X > 1 ) {
            // sliceSize.Height += textSize.Y * 7f;
            sliceSize.Height += textSize.Y * 1f;
        }

        // var skin = LibraryServices.GetUiSkin();
        // RenderServices.Draw9Slice( render.UiBatch, isRightSpeaker ? skin.MessageRight : skin.MessageLeft,
        //     sliceSize, new DrawInfo( sort + 0.01f ) );
        render.UiBatch.DrawRectangle( sliceSize, color: Palette.Colors[ 18 ] * 0.5f, sorting: sort + .01f );

        // ==============
        // Draw portraits
        // ==============
        if ( GetPortraitForLine( line ) is {} portrait &&
             Game.Data.TryGetAsset< SpriteAsset >( portrait.Sprite ) is {} portraitSpriteAsset ) {
            
            var portraitDrawInfo = new DrawInfo { Sort = sort + 0.2f, Debug = true };
            render.UiBatch.DrawSprite( portraitSpriteAsset, new Vector2( textDialogueOffset.X - 20f, textDialogueOffset.Y - 12f ), portraitDrawInfo );
        }
    }

    protected override void OnMessage( IMessage message ) {
        if ( message is DialogueMessage dialogue ) {
            if ( dialogue.Clear ) {
                _exit = true;
                _monologue = null;

                // World.TryGetUniqueEntity<DriverPortraitComponent>()?.RemovePaused();
                // World.TryGetUniqueEntity<GuestPortraitComponent>()?.RemovePaused();
            }
            else {
                _monologue = dialogue.Monologue;

                // _speaker = dialogue.SpeakerKind;
                if ( GetPortraitForLine( _monologue.Value.Line ) is {} portrait ) {
                    if ( dialogue.Monologue.InputType == InputType.PauseGame ) {
                        // World.TryGetUniqueEntity<DriverPortraitComponent>()?.SetPaused();
                        // World.TryGetUniqueEntity<GuestPortraitComponent>()?.SetPaused();
                    }

                    // if ( dialogue.SpeakerKind == SpeakerKind.Driver ) {
                    //     World.TryGetUniqueEntity< DriverPortraitComponent >()?.SetDriverPortrait( portrait );
                    // }
                    // else {
                    //     World.RunCoroutine( ChangeGuestPortrait( portrait ) );
                    // }
                }
            }

            _reachedEnd = false;
            _lastStartedTime = Game.NowUnscaled;
        }
        else if ( message is TargetEntityMessage targetEntityMessage ) {
            _entitySendingMessages = targetEntityMessage.Entity;
        }
    }

    // private IEnumerator< Wait > ChangeGuestPortrait( Portrait portrait ) {
    //     Entity? guest = World.TryGetUniqueEntity< GuestPortraitComponent >();
    //     if ( guest is null ) {
    //         yield break;
    //     }
    //
    //     SaveServices.SetGameplayValue( nameof( GameplayBlackboard.ShowGranny ), true );
    //
    //     Portrait? currentPortrait = guest.TryGetGuestPortrait()?.Portrait;
    //     if ( currentPortrait is not null &&
    //          currentPortrait.Value.Aseprite == portrait.Aseprite &&
    //          currentPortrait.Value.AnimationId == portrait.AnimationId ) {
    //         yield break;
    //     }
    //
    //     // Add transition here!
    //
    //     guest.SetGuestPortrait( portrait );
    // }

    private Portrait? GetPortraitForLine( in Line line ) {
        if ( line.Speaker is {} speakerAsset ) {
            SpeakerAsset? asset = Game.Data.TryGetAsset< SpeakerAsset >( speakerAsset );
            if ( asset is null ) {
                return null;
            }

            string? portrait = line.Portrait ?? Game.Data.TryGetAsset< SpeakerAsset >( speakerAsset )?.DefaultPortrait;
            if ( portrait is null ) {
                return null;
            }

            if ( !asset.Portraits.TryGetValue( portrait, out var result ) ) {
                return null;
            }

            return result.Portrait;
        }

        return null;
    }

    public override void OnDestroyed() {
        base.OnDestroyed();

        // LDGameSoundPlayer.Instance.SetGlobalParameter(LibraryServices.GetRoadLibrary().MusicFocusParameter, 0);
    }
}
