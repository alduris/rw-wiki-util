using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace WikiUtil.BuiltIn
{
    internal class BakedDataDrawable : CosmeticSprite
    {
        private FLabel label;
        private Phase phase = Phase.Visibility;

        private int maxViz = 0;
        private int maxFloorAlt = 0;
        private int maxSmoothFloorAlt = 0;

        public BakedDataDrawable(Room room)
        {
            this.room = room;
            for (int i = 0; i < room.TileWidth; i++)
            {
                for (int j = 0; j < room.TileHeight; j++)
                {
                    maxViz = Math.Max(maxViz, room.aimap.getAItile(i, j).visibility);

                    int floorAlt = room.aimap.getAItile(i, j).floorAltitude;
                    int smoothFloorAlt = room.aimap.getAItile(i, j).smoothedFloorAltitude;
                    if (floorAlt != 100000) maxFloorAlt = Math.Max(maxFloorAlt, floorAlt);
                    if (smoothFloorAlt != 100000) maxSmoothFloorAlt = Math.Max(maxSmoothFloorAlt, smoothFloorAlt);
                }
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (slatedForDeletetion) return;
            if (phase == Phase.Destroy)
            {
                Destroy();
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            List<FSprite> sprites = [];
            for (int i = 0; i < room.TileWidth; i++)
            {
                for (int j = 0; j < room.TileHeight; j++)
                {
                    sprites.Add(
                        new FSprite("pixel")
                        {
                            scale = 18f,
                            anchorX = 0f,
                            anchorY = 0f,
                            color = new Color(0f, 0f, 0f, 0f)
                        });
                }
            }
            sLeaser.sprites = [..sprites];
            sLeaser.containers = [new FContainer()];
            sLeaser.containers[0].AddChild(label = new FLabel(Custom.GetFont(), "") { anchorX = 0f, anchorY = 0f });

            var container = rCam.ReturnFContainer("HUD2");
            foreach (var sprite in sLeaser.sprites)
            {
                container.AddChild(sprite);
            }
            container.AddChild(sLeaser.containers[0]);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (slatedForDeletetion) return;

            for (int i = 0; i < room.TileWidth; i++)
            {
                for (int j = 0; j < room.TileHeight; j++)
                {
                    sLeaser.sprites[i * room.TileHeight + j].SetPosition(new Vector2(i, j) * 20f + Vector2.one - camPos);
                    sLeaser.sprites[i * room.TileHeight + j].color = GetColor(i, j);
                }
            }

            label.SetPosition((Vector2)Futile.mousePosition + new Vector2(0.01f, 0.01f));
            label.text = LabelText(camPos);
        }

        private string LabelText(Vector2 camPos)
        {
            if (phase == Phase.Destroy) return "";

            var pos = (Vector2)Futile.mousePosition + camPos;

            string prefix = phase switch
            {
                Phase.Visibility => "Visibility: ",
                Phase.FloorAltitude => "Floor altitude: ",
                Phase.SmoothedFloorAltitude => "Smoothed floor altitude: ",
                _ => "",
            };

            int value = phase switch
            {
                Phase.Visibility => room.aimap.getAItile(pos).visibility,
                Phase.FloorAltitude => room.aimap.getAItile(pos).floorAltitude,
                Phase.SmoothedFloorAltitude => room.aimap.getAItile(pos).smoothedFloorAltitude,
                _ => 0
            };

            return prefix + value;
        }

        private Color GetColor(int x, int y)
        {
            if (phase == Phase.Destroy) return new Color(0f, 0f, 0f, 0f);

            int color = phase switch
            {
                Phase.Visibility => room.aimap.getAItile(x, y).visibility,
                Phase.FloorAltitude => room.aimap.getAItile(x, y).floorAltitude,
                Phase.SmoothedFloorAltitude => room.aimap.getAItile(x, y).smoothedFloorAltitude,
                _ => 0,
            };

            int max = phase switch
            {
                Phase.Visibility => maxViz,
                Phase.FloorAltitude => maxFloorAlt,
                Phase.SmoothedFloorAltitude => maxSmoothFloorAlt,
                _ => 0
            };

            if (phase == Phase.FloorAltitude || phase == Phase.SmoothedFloorAltitude)
            {
                if (color == 100000)
                    color = 0;
                else
                    color = max - color - 1;
            }

            return Custom.HSL2RGB(0.667f * (1f - color / (float)max), 1f, 0.5f, 0.4f);
        }

        public void NextPhase()
        {
            phase++;
        }

        private enum Phase
        {
            Visibility,
            FloorAltitude,
            SmoothedFloorAltitude,
            Destroy
        }
    }
}
