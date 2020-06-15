using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Engine.Core;
using Engine.Graphics;
using Game.Core;
using Game.Ui;

namespace Game.Scenes
{
    public partial class GameScene
    {
        /// <summary>
        /// A single entry in the action menu. Might have sub items that are only shown when
        /// the UI state that corresponds to this entry is active.
        /// </summary>
        private class MenuEntry
        {
            /// <summary>
            /// A sub entry in menu.
            /// </summary>
            /// <remarks>
            ///  Nesting is not allowed here, that's why there is a seperate type for this, and the
            /// composite pattern was not used.
            /// </remarks>
            public class SubMenuEntry
            {
                /// <summary>
                /// The key binding for this entry
                /// </summary>
                public char Keybind { get; set; }
            
                /// <summary>
                /// The label for this entry
                /// </summary>
                public string Label { get; set; }

                /// <summary>
                /// Whether this entry is shown as active or not.
                /// </summary>
                /// <remarks>
                /// This is used for toggles, such as "show resources"
                /// </remarks>
                public Func<bool> IsActive { get; set; }
                    = () => true;
            }

            /// <summary>
            /// The UI state that corresponds to this entry. If this is set, the entry will be greyed-out
            /// when the state is neither <see cref="GameUiState.Main"/> or this state. Also, sub-entries, if
            /// present, will only be drawn if this state is active.
            /// </summary>
            public Optional<GameUiState> UiState { get; set; }
            
            /// <summary>
            /// The key binding for this entry
            /// </summary>
            public char Keybind { get; set; }
            
            /// <summary>
            /// The label for this entry
            /// </summary>
            public string Label { get; set; }
            
            /// <summary>
            /// Whether this entry is shown as active or not.
            /// </summary>
            /// <remarks>
            /// This is used for toggles, such as "show resources"
            /// </remarks>
            public Func<bool> IsActive { get; set; }
                = () => true;
            
            /// <summary>
            /// Optional sub entries
            /// </summary>
            public Optional<List<SubMenuEntry>> SubEntries { get; set; }

            /// <summary>
            /// Render this entry to the given surface at given start position, and return
            /// position of next entry
            /// </summary>
            public Position Render(Surface surface, GameUiState state, Position position)
            {
                // Determine color
                var frontColor = (state == GameUiState.Main || (this.UiState.HasValue && this.UiState.Value == state))
                    && this.IsActive.Invoke()
                    ? UiColors.ActiveText
                    : UiColors.InactiveText;
                
                // Draw the main entry
                surface.DrawKeybinding(position, this.Keybind.ToString(), this.Label, UiColors.Keybinding, frontColor,
                    DefaultColors.Black);
                
                // Draw the sub entries, if needed
                var drawSubEntries =
                    (this.SubEntries.HasValue && (!this.UiState.HasValue || state == this.UiState.Value));

                var subPosition = position + new Position(0, 1);
                
                if (drawSubEntries)
                {
                    var indent = new Position(2, 0);
                    
                    foreach (var subEntry in this.SubEntries.Value)
                    {
                        var subFrontColor = subEntry.IsActive.Invoke() ? frontColor : UiColors.InactiveText;
                        surface.DrawKeybinding(subPosition + indent, subEntry.Keybind.ToString(), subEntry.Label, UiColors.Keybinding, subFrontColor,
                            DefaultColors.Black);
                        subPosition += new Position(0, 1);
                    }
                }

                return subPosition;
            }
        }

        /// <summary>
        /// The game action menu, drawn to the right of the game view
        /// </summary>
        private List<MenuEntry> _gameMenu;

        /// <summary>
        /// Initialize game menu
        /// </summary>
        private void InitializeMenu()
        {
            this._gameMenu = new List<MenuEntry>
            {
                new MenuEntry
                {
                    Keybind = 'C',
                    Label = "Place city",
                    UiState = GameUiState.PlaceCity,
                    SubEntries = new List<MenuEntry.SubMenuEntry>
                    {
                        new MenuEntry.SubMenuEntry
                        {
                            Keybind = 'p',
                            Label = "New Province",
                            IsActive = () => this._newProvince
                        }
                    }
                },
                new MenuEntry
                {
                    Keybind = 'R',
                    Label = "Show resources",
                    IsActive = () => this._terrainView.ShowResources
                },
                new MenuEntry
                {
                    Keybind = 'T',
                    Label = "Gen Test Data"
                },
                new MenuEntry
                {
                    Keybind = 'Q',
                    Label = "Save and quit"
                }
            };
        }
    }
}