using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using WikiUtil.Tools;

namespace WikiUtil.BuiltIn
{
    internal class StencilTool : GUIToggleTool, IPauseGame
    {
        internal const string TOOL_ID = "Object Stencil";

        public StencilTool() : base(TOOL_ID, new Keybind(KeyCode.B, true, false, false))
        {
        }

        public override void Update(RainWorld rainWorld)
        {
            if (rainWorld.processManager.currentMainLoop is RainWorldGame)
            {
                base.Update(rainWorld);
            }
            else
            {
                if (toggled)
                {
                    ToggleOff(rainWorld);
                }
                toggled = false;
            }
        }

        public bool Pause => toggled;

        private const float leftWidth = 250f;
        private const float rightWidth = 500f;
        private const float contentHeight = 500f;
        private const float contentMargin = 10f;
        public override Rect WindowSize { get; set; } = new Rect(100f, 100f, leftWidth + rightWidth + contentMargin * 3, 20f + contentHeight);

        private readonly Dictionary<FNode, bool> origVisibility = [];
        private readonly HashSet<RoomCamera.SpriteLeaser> currentSLeasers = [];
        private Color oldCamColor = Color.black;
        private Texture2D textureCache = null;

        public override void ToggleOn(RainWorld rainWorld)
        {
            currentSLeasers.Clear();
            origVisibility.Clear();
            RecursivelySetUpNodes(Futile.stage);
            oldCamColor = Futile.instance.camera.backgroundColor;
            Futile.instance.camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        }

        private void RecursivelySetUpNodes(FNode node)
        {
            if (node is FContainer container)
            {
                foreach (var subnode in container._childNodes)
                {
                    RecursivelySetUpNodes(subnode);
                }
            }
            else
            {
                origVisibility.Add(node, node.isVisible);
                node.isVisible = false;
            }
        }

        public override void ToggleOff(RainWorld rainWorld)
        {
            foreach (var (node, visibility) in origVisibility)
            {
                node.isVisible = visibility;
            }
            origVisibility.Clear();
            currentSLeasers.Clear();
            Futile.instance.camera.backgroundColor = oldCamColor;
        }

        private Vector2 sLeaserListScroll;

        public override void OnGUI(RainWorld rainWorld)
        {
            if (rainWorld.processManager.currentMainLoop is RainWorldGame game)
            {
                // Update cache maybe, hopefully by now it has been updated
                if (textureCache == null && currentSLeasers.Count > 0 && currentSLeasers.All(NoMeshesDirty))
                {
                    CreateTextureCache();
                }

                // Left side: list of sleasers, toggleable
                bool changed = false;
                List<RoomCamera.SpriteLeaser> sLeasersToAdd = null;
                List<RoomCamera.SpriteLeaser> sLeasersToRemove = null;

                var sLeasers = game.cameras[0].spriteLeasers;
                sLeaserListScroll = GUI.BeginScrollView(new Rect(contentMargin, 20f, leftWidth, contentHeight), sLeaserListScroll, new Rect(0f, 0f, leftWidth - 20f, 24f * (sLeasers.Count + 1)));
                GUI.Label(new Rect(0f, 0f, leftWidth - 20f, 24f), "Note: objects must be in camera view.");
                for (int i = 0; i < sLeasers.Count; i++)
                {
                    bool isSelected = currentSLeasers.Contains(sLeasers[i]);
                    if (GUI.Toggle(new Rect(0f, 24f * (i + 1), leftWidth - 20f, 24f), isSelected, sLeasers[i].drawableObject.GetType().Name) != isSelected)
                    {
                        sLeasersToAdd ??= [];
                        sLeasersToRemove ??= [];
                        if (isSelected)
                        {
                            currentSLeasers.Remove(sLeasers[i]);
                            sLeasersToRemove.Add(sLeasers[i]);
                        }
                        else
                        {
                            currentSLeasers.Add(sLeasers[i]);
                            sLeasersToAdd.Add(sLeasers[i]);
                        }
                        changed = true;
                    }
                }
                GUI.EndScrollView();

                if (changed)
                {
                    foreach (var s in sLeasersToRemove)
                    {
                        HideSLeaser(s);
                    }
                    foreach (var s in sLeasersToAdd)
                    {
                        RestoreSLeaser(s);
                    }

                    if (textureCache != null)
                    {
                        UnityEngine.Object.Destroy(textureCache);
                        textureCache = null;
                    }
                }

                // Right side: preview
                if (currentSLeasers.Count > 0 && textureCache != null)
                {
                    const float rightStartX = contentMargin + leftWidth + contentMargin;
                    const float textureAreaHeight = contentHeight - contentMargin - 24f;
                    GUI.Label(
                        new Rect(
                            rightStartX + rightWidth / 2f - textureCache.width / 2f,
                            20f + textureAreaHeight / 2f - textureCache.height / 2f,
                            Mathf.Min(rightWidth, textureCache.width),
                            Mathf.Min(textureAreaHeight, textureCache.height)
                        ),
                        textureCache
                    );

                    if (GUI.Button(new Rect(rightStartX, 20f + contentHeight - 24f - contentMargin, rightWidth, 24f), "Download"))
                    {
                        const string FOLDER_NAME = "stencil";
                        string fileName;
                        if (currentSLeasers.Count == 1)
                        {
                            fileName = currentSLeasers.First().drawableObject.GetType().Name;
                        }
                        else
                        {
                            const int NAME_CAP = 240;
                            fileName = string.Join("_", currentSLeasers.OrderBy(x => x.drawableObject.GetType().Name, StringComparer.OrdinalIgnoreCase).Select(x => x.drawableObject.GetType().Name));
                            if (fileName.Length > NAME_CAP)
                            {
                                fileName = fileName.Substring(0, NAME_CAP);
                            }
                        }
                        string fullpath = ToolDatabase.GetPathTo(FOLDER_NAME, fileName + ".png");
                        int i = 2;
                        while (File.Exists(fullpath))
                        {
                            fullpath = ToolDatabase.GetPathTo(FOLDER_NAME, $"{fileName}-{i}.png");
                            i++;
                        }
                        File.WriteAllBytes(fullpath, textureCache.EncodeToPNG());
                    }
                }
            }
        }

        private void RestoreSLeaser(RoomCamera.SpriteLeaser sLeaser)
        {
            if (sLeaser == null) return;

            if (sLeaser.containers != null)
            {
                foreach (var container in sLeaser.containers)
                {
                    RecursivelyRestoreNode(container);
                }
            }
            if (sLeaser.sprites != null)
            {
                foreach (var sprite in sLeaser.sprites)
                {
                    RecursivelyRestoreNode(sprite);
                }
            }

            void RecursivelyRestoreNode(FNode node)
            {
                if (node is FContainer container)
                {
                    foreach (var subnode in container._childNodes)
                    {
                        RecursivelyRestoreNode(subnode);
                    }
                }
                else if (origVisibility.TryGetValue(node, out var visibility))
                {
                    node.isVisible = visibility;
                }
            }
        }

        private void HideSLeaser(RoomCamera.SpriteLeaser sLeaser)
        {
            if (sLeaser == null) return;

            if (sLeaser.containers != null)
            {
                foreach (var container in sLeaser.containers)
                {
                    RecursivelyHideNode(container);
                }
            }
            if (sLeaser.sprites != null)
            {
                foreach (var sprite in sLeaser.sprites)
                {
                    RecursivelyHideNode(sprite);
                }
            }

            void RecursivelyHideNode(FNode node)
            {
                if (node is FContainer container)
                {
                    foreach (var subnode in container._childNodes)
                    {
                        RecursivelyHideNode(subnode);
                    }
                }
                else if (origVisibility.ContainsKey(node))
                {
                    node.isVisible = false;
                }
            }
        }

        private bool NoMeshesDirty(RoomCamera.SpriteLeaser sLeaser)
        {
            if (sLeaser == null) return false;

            bool flag = true;
            if (sLeaser.containers != null)
            {
                foreach (var container in sLeaser.containers)
                {
                    flag &= RecursivelyCheckNodes(container);
                    if (!flag) break;
                }
            }
            if (flag && sLeaser.sprites != null)
            {
                foreach (var sprite in sLeaser.sprites)
                {
                    flag &= RecursivelyCheckNodes(sprite);
                    if (!flag) break;
                }
            }

            return flag;

            bool RecursivelyCheckNodes(FNode node)
            {
                bool flag = true;
                if (node is FContainer container)
                {
                    foreach (var subnode in container._childNodes)
                    {
                        flag &= RecursivelyCheckNodes(subnode);
                    }
                }
                else if (origVisibility.ContainsKey(node))
                {
                    flag &= !node._isMatrixDirty;
                }
                return flag;
            }
        }

        private Rect DetermineMeshBounds()
        {
            bool hasRect = false;
            Rect rect = Rect.zero;
            foreach (var s in currentSLeasers)
            {
                var newRect = DetermineMeshBounds(s);
                if (newRect != Rect.zero)
                {
                    if (!hasRect)
                    {
                        rect = newRect;
                        hasRect = true;
                    }
                    else
                    {
                        rect = Rect.MinMaxRect(
                            Mathf.Min(rect.xMin, newRect.xMin),
                            Mathf.Min(rect.yMin, newRect.yMin),
                            Mathf.Max(rect.xMax, newRect.xMax),
                            Mathf.Max(rect.yMax, newRect.yMax)
                            );
                    }
                }
            }
            return rect;
        }

        private Rect DetermineMeshBounds(RoomCamera.SpriteLeaser sLeaser)
        {
            if (sLeaser == null) return Rect.zero;

            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;

            if (sLeaser.containers != null)
            {
                foreach (var container in sLeaser.containers)
                {
                    foreach (var vert in GetMeshVertsFor(container))
                    {
                        minX = Mathf.Min(minX, vert.x);
                        maxX = Mathf.Max(maxX, vert.x);
                        minY = Mathf.Min(minY, vert.y);
                        maxY = Mathf.Max(maxY, vert.y);
                    }
                }
            }
            if (sLeaser.sprites != null)
            {
                foreach (var sprite in sLeaser.sprites)
                {
                    foreach (var vert in GetMeshVertsFor(sprite))
                    {
                        minX = Mathf.Min(minX, vert.x);
                        maxX = Mathf.Max(maxX, vert.x);
                        minY = Mathf.Min(minY, vert.y);
                        maxY = Mathf.Max(maxY, vert.y);
                    }
                }
            }

            if (minX == float.PositiveInfinity) return Rect.zero;

            minX = Mathf.Max(0, minX);
            minY = Mathf.Max(0, minY);
            maxX = Mathf.Min(Futile.screen.pixelWidth, maxX);
            maxY = Mathf.Min(Futile.screen.pixelHeight, maxY);

            return new Rect(minX, minY, maxX - minX, maxY - minY);

            IEnumerable<Vector2> GetMeshVertsFor(FNode node)
            {
                // Disregard invisible items
                if (!node.isVisible || !node._isOnStage) yield break;

                if (node is FContainer container)
                {
                    // Recursively check inside containers
                    foreach (var subnode in container._childNodes)
                    {
                        foreach (var vert in GetMeshVertsFor(subnode))
                        {
                            yield return vert;
                        }
                    }
                }
                else if (origVisibility.ContainsKey(node) && node is FFacetNode facetNode && facetNode._firstFacetIndex != -1)
                {
                    // Check facet type (realistically it should only ever be Quad or Triangle but Futile doesn't give a way to determine how far to jump)
                    int facetLength = -1;
                    if (facetNode._facetType == FFacetType.Quad)
                    {
                        facetLength = 4;
                    }
                    else if (facetNode._facetType == FFacetType.Triangle)
                    {
                        facetLength = 3;
                    }
                    else
                    {
                        Plugin.Logger.LogWarning($"UNKNOWN FACET TYPE! ({facetNode._facetType.name})");
                    }

                    if (facetLength > 0)
                    {
                        int startIndex = facetNode._firstFacetIndex * facetLength;
                        for (int i = 0; i < facetNode._numberOfFacetsNeeded; i++)
                        {
                            for (int j = 0; j < facetLength; j++)
                            {
                                Vector3 vert = facetNode._renderLayer.vertices[startIndex + i * facetLength + j];
                                yield return new Vector2(vert.x, vert.y); // we don't care about z
                            }
                        }
                    }
                }
            }
        }

        private void CreateTextureCache()
        {
            var bounds = DetermineMeshBounds();
            Plugin.Logger.LogDebug(bounds);
            if (textureCache != null)
            {
                UnityEngine.Object.Destroy(textureCache);
            }
            textureCache = new Texture2D(Mathf.CeilToInt(bounds.width), Mathf.CeilToInt(bounds.height), TextureFormat.ARGB32, false);

            int x = Mathf.FloorToInt(bounds.x);
            int y = Futile.screen.pixelHeight - Mathf.FloorToInt(bounds.y) - textureCache.height - 1; // y-axis flipped (origin at bottom left vs top left)
            //Graphics.CopyTexture(Futile.instance.camera.activeTexture, 0, 0, x, y, textureCache.width, textureCache.height, textureCache, 0, 0, 0, 0);

            var oldRT = RenderTexture.active;
            var rt = new RenderTexture(Futile.screen.pixelWidth, Futile.screen.pixelHeight, 32, RenderTextureFormat.ARGB32);
            Graphics.Blit(Futile.instance.camera.activeTexture, rt);
            RenderTexture.active = rt;
            textureCache.ReadPixels(new Rect(x, y, textureCache.width, textureCache.height), 0, 0);
            RenderTexture.active = oldRT;

            textureCache.Apply();
        }
    }
}
