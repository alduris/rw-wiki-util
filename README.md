# Wiki Utils
Collection of features for wikiing in Rain World. Hopefully useful in some way.

## Built-In Tools
These tools can be enabled/disabled and have their keybinds changed through the mod's options menu. For tools that output files, their output location can also be changed there.
| Tool | Default Keybind | Description |
| ---- | --------------- | ----------- |
| Complete Pause | Shift+Esc | While held, completely pauses the game. |
| Icon Grabber | Ctrl+I | Opens a menu where creature and object icons can be downloaded in the style they appear in on the Rain World wiki. |
| Object Stencil | Ctrl+B | While in-game, allows selecting individual objects and creatures to export as a transparent screenshot. |
| Region Creature/Object Scanner | Ctrl+R | Allows scanning regions for their creature/object spawns per-slugcat. Note that this process is not perfect and results should be manually verified afterwards. |
| Screenshotter | F12 | Takes a screenshot in the game's native resolution. |
| Song Records Manager | Ctrl+M | While in story mode, opens a menu allowing played songs to be forgotten so their triggers will play them again. Requested by Lolight2. |
| Text Decryption | Ctrl+D | Opens a menu where text files in the base game and mods can be decrypted, read, and copied to your clipboard. |
| Token Scanner | Ctrl+T | Allows scanning regions for their tokens, pearls, echoes, and more, per-slugcat. Note that this process is not perfect and results should be manually verified afterwards. |

## Public API
Register a tool through `WikiUtil.ToolDatabase.RegisterTool()`! Tools must implement `Tool` in some way; it is recommended to use one of the specific subclasses that handle some of the logic for you: `ActionTool`, `HoldTool`, `ToggleTool`, and `GUIToggleTool` (if you aren't using `GUIToggleTool`, your `Tool` must also implement the interface `IHaveGUI`). Check out the built-in tools for examples of how to make them.

Tools are run every `RainWorld.Update` tick; if you only want it to run while there is a `RainWorldGame` process or similar, you will have to add your own logic for those tools. See the built-in `StencilTool` for a good way to do that.
