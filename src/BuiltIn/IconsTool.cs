using System;
using System.Collections.Generic;
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

            // Load default icons
            foreach (var name in creatureNames)
            {
                if (!creatureIcons.ContainsKey((name, 0)))
                {
                    creatureIcons[(name, 0)] = CreatureIcon(name, 0);
                }
            }
            foreach (var name in objectNames)
            {
                if (!objectIcons.ContainsKey((name, 0)))
                {
                    objectIcons[(name, 0)] = ItemIcon(name, 0);
                }
            }

            if (defaultTexture == null)
            {
                defaultTexture = GetElementFromAtlas(Futile.atlasManager.GetElementWithName("Symbol_Unknown"));
                Iconify(defaultTexture);
            }

            // Sort
            Array.Sort(creatureNames);
            Array.Sort(objectNames);
        }

        public override void ToggleUpdate(RainWorld rainWorld) { }

        private Vector2 pickerOffset;
        private int creaturePicker = 0;
        private int objectPicker = -1;
        private string dataString = "0";

        public bool ShowWindow => toggled;

        public Rect WindowSize { get; set; } = new Rect(100f, 100f, 300f, 540f);

        public void OnGUI(RainWorld rainWorld)
        {
            const float sboxWidth = 400f;
            int creatureCount = creatureNames.Length;
            int objectCount = objectNames.Length;

            int creatureRows = Mathf.CeilToInt(creatureCount / 2f);
            int objectRows = Mathf.CeilToInt(objectCount / 2f);

            // Define scrollbox
            pickerOffset = GUI.BeginScrollView(
                new Rect(10f, 20f, sboxWidth, 500f),
                pickerOffset,
                new Rect(0f, 0f, sboxWidth, 40f + 20f * (creatureRows + objectRows)),
                false, true);

            // Creatures
            GUI.Label(new Rect(0f, 4f, sboxWidth, 20f), "CREATURES");
            creaturePicker = GUI.SelectionGrid(new Rect(0f, 28f, sboxWidth, creatureRows * 24f - 4f), creaturePicker, creatureNames, 2);

            if (creaturePicker > -1 && objectPicker > -1)
            {
                objectPicker = -1;
            }

            // Objects
            float objectStartY = 24f * creatureRows + 28f;
            GUI.Label(new Rect(0f, objectStartY, 124f, 20f), "OBJECTS");
            objectPicker = GUI.SelectionGrid(new Rect(0f, objectStartY + 24f, sboxWidth, objectRows * 24f - 4f), objectPicker, objectNames, 2);

            if (objectPicker > -1 && creaturePicker > -1)
            {
                creaturePicker = -1;
            }

            // Complete scrollbox
            GUI.EndScrollView();

            // Now for the other parts of it
            GUI.Label(new Rect(160f, 20f, 130f, 20f), "Data: (enter integer)");
            dataString = GUI.TextField(new Rect(160f, 44f, 130f, 20f), dataString, 11);
            if (int.TryParse(dataString, out int dataInt))
            {
                Texture2D iconTexture = defaultTexture;
                if (creaturePicker > -1 && !creatureIcons.TryGetValue((creatureNames[creaturePicker], dataInt), out iconTexture))
                {
                    iconTexture = CreatureIcon(creatureNames[creaturePicker], dataInt);
                }
                else if (objectPicker > -1 && !objectIcons.TryGetValue((objectNames[objectPicker], dataInt), out iconTexture))
                {
                    iconTexture = ItemIcon(objectNames[objectPicker], dataInt);
                }

                Vector2 size = new Vector2(iconTexture.width, iconTexture.height) * 2f;
                GUI.Label(new Rect((new Vector2(225f, 250f) - size / 2f).Round(), size), iconTexture);
            }
        }
    }
}
