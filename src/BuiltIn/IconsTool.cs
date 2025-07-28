using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using WikiUtil.Tools;
using static WikiUtil.BuiltIn.IconsToolHelper;
using IconData = (string type, int data);

namespace WikiUtil.BuiltIn
{

    internal class IconsTool : ToggleTool, IHaveGUI
    {
        private readonly Dictionary<IconData, Texture2D> creatureIcons = [];
        private readonly Dictionary<IconData, Texture2D> objectIcons = [];
        private string[] creatureNames = [];
        private string[] objectNames = [];

        private Texture2D defaultTexture = null;

        internal const string TOOL_ID = "Icon Grabber";
        public IconsTool() : base(TOOL_ID, new Keybind(KeyCode.I, true, false, false))
        {
        }

        public override void ToggleOn(RainWorld rainWorld)
        {
            // Cache creature and object names
            creatureNames = [.. CreatureTemplate.Type.values.entries.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)];
            objectNames = [.. AbstractPhysicalObject.AbstractObjectType.values.entries.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)];

            // Cache default texture
            if (defaultTexture == null)
            {
                defaultTexture = GetElementFromAtlas(Futile.atlasManager.GetElementWithName("Symbol_Unknown"));
                Iconify(defaultTexture);
            }
        }

        public override void ToggleUpdate(RainWorld rainWorld) { }

        private Vector2 pickerOffset;
        private int creaturePicker = 0;
        private int objectPicker = -1;
        private string dataString = "0";

        public bool ShowWindow => toggled;

        private const float leftWidth = 300f;
        private const float rightWidth = 150f;
        private const float contentMargin = 10f;
        public Rect WindowSize { get; set; } = new Rect(100f, 100f, 3 * contentMargin + leftWidth + rightWidth, 540f);

        public void OnGUI(RainWorld rainWorld)
        {
            int creatureCount = creatureNames.Length;
            int objectCount = objectNames.Length;

            int creatureRows = Mathf.CeilToInt(creatureCount / 2f);
            int objectRows = Mathf.CeilToInt(objectCount / 2f);

            // Define scrollbox
            pickerOffset = GUI.BeginScrollView(
                new Rect(contentMargin, 20f, leftWidth, 500f),
                pickerOffset,
                new Rect(0f, 0f, leftWidth - 20f, 60f + 24f * (creatureRows + objectRows)),
                false, true);

            // Creatures
            GUI.Label(new Rect(0f, 0f, leftWidth, 20f), "CREATURES");
            creaturePicker = GUI.SelectionGrid(new Rect(0f, 24f, leftWidth - 20f, creatureRows * 24f - 4f), creaturePicker, creatureNames, 2);

            if (creaturePicker > -1 && objectPicker > -1)
            {
                objectPicker = -1;
            }

            // Objects
            float objectStartY = 24f * creatureRows + 28f;
            GUI.Label(new Rect(0f, objectStartY, 124f, 20f), "OBJECTS");
            objectPicker = GUI.SelectionGrid(new Rect(0f, objectStartY + 24f, leftWidth - 20f, objectRows * 24f - 4f), objectPicker, objectNames, 2);

            if (objectPicker > -1 && creaturePicker > -1)
            {
                creaturePicker = -1;
            }

            // Complete scrollbox
            GUI.EndScrollView();

            // Now for the other parts of it
            const float rightStart = 2 * contentMargin + leftWidth;
            GUI.Label(new Rect(rightStart, 20f, rightWidth, 20f), "Data: (enter integer)");
            dataString = GUI.TextField(new Rect(rightStart, 44f, rightWidth, 20f), dataString, 11);
            if (int.TryParse(dataString, out int dataInt))
            {
                Texture2D iconTexture = defaultTexture;
                if (creaturePicker > -1 && !creatureIcons.TryGetValue((creatureNames[creaturePicker], dataInt), out iconTexture))
                {
                    iconTexture = CreatureIcon(creatureNames[creaturePicker], dataInt);
                    creatureIcons[(creatureNames[creaturePicker], dataInt)] = iconTexture;
                }
                else if (objectPicker > -1 && !objectIcons.TryGetValue((objectNames[objectPicker], dataInt), out iconTexture))
                {
                    iconTexture = ItemIcon(objectNames[objectPicker], dataInt);
                    objectIcons[(objectNames[objectPicker], dataInt)] = iconTexture;
                }

                Vector2 size = new Vector2(iconTexture.width, iconTexture.height) * 2f;
                GUI.Label(new Rect((new Vector2(rightStart + rightWidth / 2f, 250f) - size / 2f).Round(), size), iconTexture);

                if (creaturePicker > -1 || objectPicker > -1)
                {
                    string iconName = (creaturePicker > -1) ? creatureNames[creaturePicker] : objectNames[objectPicker];
                    if (GUI.Button(new Rect(rightStart, 500f, rightWidth, 20f), "Download"))
                    {
                        string fullpath = ToolDatabase.GetPathTo("icons", Util.SafeString(iconName) + ".png");
                        File.WriteAllBytes(fullpath, iconTexture.EncodeToPNG());
                    }
                }
            }
        }
    }
}
