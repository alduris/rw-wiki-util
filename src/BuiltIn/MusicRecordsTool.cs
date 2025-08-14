using System.Collections.Generic;
using UnityEngine;
using WikiUtil.Tools;

namespace WikiUtil.BuiltIn
{
    internal class MusicRecordsTool() : GUIToggleTool(TOOL_ID, new Keybind(KeyCode.M, true, false, false))
    {
        internal const string TOOL_ID = "Song Records Manager";

        public override void Update(RainWorld rainWorld)
        {
            if (rainWorld.processManager.currentMainLoop is RainWorldGame)
            {
                base.Update(rainWorld);
            }
            else
            {
                toggled = false;
            }
        }

        private const float WIDTH = 350f;
        private const float HEIGHT = 200f;

        public override Rect WindowSize { get; set; } = new Rect(100f, 100f, 10f + WIDTH, 20f + HEIGHT);

        private Vector2 scrollPos;

        public override void OnGUI(RainWorld rainWorld)
        {
            if (rainWorld.processManager.currentMainLoop is RainWorldGame game && game.IsStorySession)
            {
                SaveState saveState = game.GetStorySession.saveState;
                List<DeathPersistentSaveData.SongPlayRecord> songRecords = saveState.deathPersistentSaveData.songsPlayRecords;

                scrollPos = GUI.BeginScrollView(new Rect(10f, 20f, WIDTH, HEIGHT), scrollPos, new Rect(0f, 0f, WIDTH - 20f, 24f * songRecords.Count + 24f), false, true);

                const string cycleLabelText = "Cycle";
                float cycleLabelWidth = GUI.skin.label.CalcSize(new GUIContent(cycleLabelText)).x;
                GUI.Label(new Rect(0f, 0f, cycleLabelWidth, 24f), cycleLabelText);
                float songNameWidth = WIDTH - cycleLabelWidth - 60f;
                GUI.Label(new Rect(cycleLabelWidth + 10f, 0f, songNameWidth, 24f), "Song name");

                int toRemove = -1;
                for (int i = 0; i < songRecords.Count; i++)
                {
                    float y = 24f * (songRecords.Count - i); // this still leaves room for the headers while reversing the order of the list

                    GUI.Label(new Rect(0f, y, cycleLabelWidth, 24f), songRecords[i].cycleLastPlayed.ToString());
                    GUI.Label(new Rect(cycleLabelWidth + 10f, y, songNameWidth, 24f), songRecords[i].songName);

                    if (GUI.Button(new Rect(WIDTH - 40f, y + 2f, 20f, 20f), "\u2212"))
                    {
                        toRemove = i;
                    }
                }

                if (toRemove > -1)
                {
                    songRecords.RemoveAt(toRemove);
                }

                GUI.EndScrollView();
            }
        }

    }
}
