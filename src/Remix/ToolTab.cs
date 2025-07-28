using System;
using System.Collections.Generic;
using System.Linq;
using Menu.Remix.MixedUI;
using UnityEngine;
using WikiUtil.Remix.UI;

namespace WikiUtil.Remix
{
    internal class ToolTab(OptionInterface owner) : Tab(owner, "Tools & Keybinds")
    {
        public override void Initialize()
        {
            // Initialize scrollbox
            var box = new OpScrollBox(this, 0f, false, true);

            // Set up top of box
            box.AddItems(
                // Title
                new OpShinyLabel(new Vector2(0f, 560f), new Vector2(600f, 30f), Translate("WIKI UTIL"), FLabelAlignment.Center, true)
                { verticalAlignment = OpLabel.LabelVAlignment.Center },
                // Description
                new OpLabel(new Vector2(0, 530f), new Vector2(600f, 24f), Translate("A collection of utilities for the wiki, by Alduris"), FLabelAlignment.Center, false)
                { verticalAlignment = OpLabel.LabelVAlignment.Center },
                // Horizontal line
                new OpImage(new Vector2(10, 518f), "pixel")
                { scale = new Vector2(570f, 2f), color = Menu.Menu.MenuColor(Menu.Menu.MenuColors.MediumGrey).rgb }
                );

            // Initialize column headers
            var sortedTools = ToolDatabase.GetToolIDs().OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
            var inputBeginX = sortedTools.Max(x => LabelTest.GetWidth(Translate(x), false)) + LabelTest.GetWidth(": ", false) + 10f + 34f + 10f;

            box.AddItems(
                new OpLabel(new Vector2(10f, 484f), new Vector2(34f, 24f), Translate("Enabled/Name"), FLabelAlignment.Left, false)
                { verticalAlignment = OpLabel.LabelVAlignment.Center },
                new OpLabel(new Vector2(inputBeginX, 484f), new Vector2(24f, 24f), Translate("Ctrl"), FLabelAlignment.Left, false)
                { verticalAlignment = OpLabel.LabelVAlignment.Center },
                new OpLabel(new Vector2(inputBeginX + 34f, 484f), new Vector2(24f, 24f), Translate("Alt"), FLabelAlignment.Left, false)
                { verticalAlignment = OpLabel.LabelVAlignment.Center },
                new OpLabel(new Vector2(inputBeginX + 34f * 2, 484f), new Vector2(24f, 24f), Translate("Shift"), FLabelAlignment.Left, false)
                { verticalAlignment = OpLabel.LabelVAlignment.Center },
                new OpLabel(new Vector2(inputBeginX + 34f * 3, 484f), new Vector2(24f, 24f), Translate("Key code"), FLabelAlignment.Left, false)
                { verticalAlignment = OpLabel.LabelVAlignment.Center }
                );

            // The actual inputs
            float y = 474f + 6f;
            foreach (var tool in sortedTools)
            {
                y -= 30f;
                var enabledCB = new OpCheckBox(RemixMenu.EnabledConfig[tool], new Vector2(10f, y));
                var toolName = new OpLabel(new Vector2(44f, y), new Vector2(inputBeginX - 44f, 24f), tool, FLabelAlignment.Left, false)
                { verticalAlignment = OpLabel.LabelVAlignment.Center };
                var ctrlCB  = new OpCheckBox(RemixMenu.ControlConfig[tool], new Vector2(inputBeginX, y));
                var altCB   = new OpCheckBox(RemixMenu.AltConfig[tool], new Vector2(inputBeginX + 34f, y));
                var shiftCB = new OpCheckBox(RemixMenu.ShiftConfig[tool], new Vector2(inputBeginX + 34f*2, y));
                var keyInput = new OpKeyBinder(RemixMenu.KeycodeConfig[tool], new Vector2(inputBeginX + 34f * 3, y), new Vector2(120f, 24f));

                box.AddItems(enabledCB, toolName, ctrlCB, altCB, shiftCB, keyInput);
            }

            // Set size of scrollbox for the scrollbar! Otherwise we can't scroll~
            box.SetContentSize(600f - y + 10f);
        }

        public override void Update() { }
    }
}
