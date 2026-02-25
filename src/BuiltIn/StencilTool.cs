using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using WikiUtil.Tools;
using Object = UnityEngine.Object;

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

        public bool Pause
        {
            get
            {
                if (tickPlay)
                {
                    tickPlay = false;
                    return false;
                }
                return toggled;
            }
        }

        private const float leftWidth = 250f;
        private const float rightWidth = 500f;
        private const float contentHeight = 600f;
        private const float contentMargin = 10f;
        private const float filterListHeight = contentHeight * 0.35f - contentMargin * 0.5f;
        private const float toggleListHeight = contentHeight - contentMargin - filterListHeight;
        public override Rect WindowSize { get; set; } = new Rect(100f, 100f, leftWidth + rightWidth + contentMargin * 3, 20f + contentHeight);

        private readonly Dictionary<FNode, bool> origVisibility = [];
        private readonly HashSet<RoomCamera.SpriteLeaser> currentSLeasers = [];
        private readonly Dictionary<Type, bool> sLeaserTypeToggle = [];
        private readonly List<Type> sortedSLeaserTypes = [];
        private Color oldCamColor = Color.black;
        private Texture2D textureCache = null;

        private Camera camera;
        private RenderTexture outputTex;
        private bool tickPlay = false;

        public override void ToggleOn(RainWorld rainWorld)
        {
            if (rainWorld.processManager.currentMainLoop is RainWorldGame game)
            {
                currentSLeasers.Clear();
                origVisibility.Clear();
                sLeaserTypeToggle.Clear();
                sortedSLeaserTypes.Clear();
                RecursivelySetUpNodes(Futile.stage);
                SetUpSLeaserTypes(game.cameras[0]);
                oldCamColor = Futile.instance.camera.backgroundColor;
                Futile.instance.camera.backgroundColor = new Color(0f, 0f, 0f, 0f);

                if (camera != null)
                {
                    Object.Destroy(camera.gameObject);
                }

                var go = new GameObject("StencilTool Camera");
                camera = go.AddComponent<Camera>();
                UpdateCamera(Vector2.zero, Vector2.one);
            }
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

        private void SetUpSLeaserTypes(RoomCamera rCam)
        {
            foreach (var sLeaser in rCam.spriteLeasers)
            {
                var type = sLeaser.drawableObject.GetType();
                if (!sLeaserTypeToggle.ContainsKey(type))
                {
                    sLeaserTypeToggle.Add(type, true);
                    sortedSLeaserTypes.Add(type);
                }
            }
            sortedSLeaserTypes.Sort(new TypeComparer());
        }

        public override void ToggleOff(RainWorld rainWorld)
        {
            foreach (var (node, visibility) in origVisibility)
            {
                node.isVisible = visibility;
            }
            origVisibility.Clear();
            currentSLeasers.Clear();
            sLeaserTypeToggle.Clear();
            sortedSLeaserTypes.Clear();
            Futile.instance.camera.backgroundColor = oldCamColor;

            Object.Destroy(camera?.gameObject);
            Object.Destroy(outputTex);
            camera = null;
            outputTex?.Release();
            outputTex = null;
        }

        private void UpdateCamera(Vector2 center, Vector2 size)
        {
            var origSize = size;
            size = new Vector2(Mathf.Clamp(size.x, 1f, 16384f), Mathf.Clamp(size.y, 1f, 16384f));
            camera.aspect = size.x / size.y;
            camera.orthographic = true;
            camera.orthographicSize = size.y / 2f;
            camera.nearClipPlane = 1f;
            camera.farClipPlane = 100f;
            camera.gameObject.transform.position = new Vector3(center.x, center.y, -50f);
            camera.depth = -1000f;
            if (outputTex == null || outputTex.width != Mathf.CeilToInt(size.x) || outputTex.height != Mathf.CeilToInt(size.y))
            {
                outputTex?.Release();
                outputTex = new RenderTexture(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y), 8)
                {
                    filterMode = FilterMode.Point
                };
                camera.targetTexture = outputTex;
            }
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            //camera.cullingMask = CULLING_MASK;
            camera.enabled = true;
            Plugin.Logger.LogDebug($"UPDATING CAMERA: pos={center}, size={size}, origSize={origSize}");
        }

        private Vector2 typeListScroll;
        private Vector2 sLeaserListScroll;

        public override void OnGUI(RainWorld rainWorld)
        {
            if (rainWorld.processManager.currentMainLoop is RainWorldGame game)
            {
                // Update cache maybe, hopefully by now it has been updated
                if (textureCache == null && currentSLeasers.Count > 0 && currentSLeasers.All(NoMeshesDirty))
                {
                    UpdateTexture();
                }

                // Left side: list of sleasers, toggleable
                bool changed = false;
                List<RoomCamera.SpriteLeaser> sLeasersToAdd = null;
                List<RoomCamera.SpriteLeaser> sLeasersToRemove = null;

                var types = sortedSLeaserTypes;
                typeListScroll = GUI.BeginScrollView(new Rect(contentMargin, 20f, leftWidth, filterListHeight), typeListScroll, new Rect(0f, 0f, leftWidth - 20f, 24f * (types.Count + 2)));
                GUI.Label(new Rect(0f, 0f, leftWidth - 20f, 24f), "Filter types here");
                if (GUI.Button(new Rect(0f, 24f, leftWidth - 20f, 24f), "Deselect all"))
                {
                    sLeasersToRemove ??= [];
                    sLeasersToRemove.AddRange(currentSLeasers);
                    changed = true;
                    foreach (var type in sortedSLeaserTypes)
                    {
                        sLeaserTypeToggle[type] = false;
                    }
                }
                for (int i = 0; i < types.Count; i++)
                {
                    bool isSelected = sLeaserTypeToggle[sortedSLeaserTypes[i]];
                    bool newSelect;
                    if ((newSelect = GUI.Toggle(new Rect(0f, 24f * (i + 2), leftWidth - 20f, 24f), sLeaserTypeToggle[sortedSLeaserTypes[i]], sortedSLeaserTypes[i].Name)) != isSelected)
                    {
                        sLeaserTypeToggle[sortedSLeaserTypes[i]] = newSelect;
                        sLeasersToRemove ??= [];
                        var toRemove = currentSLeasers.Where(x => x.drawableObject.GetType().Equals(sortedSLeaserTypes[i])).ToList();
                        if (toRemove.Any())
                        {
                            foreach (var x in toRemove)
                            {
                                currentSLeasers.Remove(x);
                                sLeasersToRemove.Add(x);
                            }
                            changed = true;
                        }
                    }
                }
                GUI.EndScrollView();

                var sLeasers = game.cameras[0].spriteLeasers.Where(x => sLeaserTypeToggle[x.drawableObject.GetType()]).ToList();
                sLeaserListScroll = GUI.BeginScrollView(new Rect(contentMargin, 20f + contentHeight - toggleListHeight, leftWidth, toggleListHeight), sLeaserListScroll, new Rect(0f, 0f, leftWidth - 20f, 24f * (sLeasers.Count + 1)));
                GUI.Label(new Rect(0f, 0f, leftWidth - 20f, 24f), "Toggle individual SpriteLeasers here");
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
                    if (sLeasersToRemove != null)
                    {
                        foreach (var s in sLeasersToRemove)
                        {
                            HideSLeaser(s);
                        }
                    }
                    if (sLeasersToAdd != null)
                    {
                        foreach (var s in sLeasersToAdd)
                        {
                            RestoreSLeaser(s);
                        }
                    }

                    if (textureCache != null)
                    {
                        Object.Destroy(textureCache);
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
                            rightStartX + rightWidth / 2f,
                            20f + textureAreaHeight / 2f,
                            Mathf.Min(rightWidth, outputTex.width),
                            Mathf.Min(textureAreaHeight, outputTex.height)
                        ),
                        outputTex
                    );

                    if (GUI.Button(new Rect(rightStartX, 20f + contentHeight - 24f - contentMargin, rightWidth, 24f), "Download"))
                    {
                        const string FOLDER_NAME = "stencil";
                        int nameCap = 240 - ToolDatabase.GetPathTo(FOLDER_NAME).Length;
                        string fileName;
                        if (currentSLeasers.Count == 1)
                        {
                            fileName = currentSLeasers.First().drawableObject.GetType().Name;
                        }
                        else
                        {
                            fileName = string.Join("_", currentSLeasers.OrderBy(x => x.drawableObject.GetType().Name, StringComparer.OrdinalIgnoreCase).Select(x => x.drawableObject.GetType().Name));
                        }
                        if (fileName.Length > nameCap)
                        {
                            fileName = fileName.Substring(0, nameCap);
                        }
                        string fullpath = ToolDatabase.GetPathTo(FOLDER_NAME, fileName + ".png");
                        int i = 2;
                        while (File.Exists(fullpath) && i < 1000)
                        {
                            fullpath = ToolDatabase.GetPathTo(FOLDER_NAME, $"{fileName}-{i}.png");
                            i++;
                        }
                        UpdateTexture();
                        File.WriteAllBytes(fullpath, textureCache.EncodeToPNG());
                        Plugin.Logger.LogInfo("Saved stencil to: " + fullpath);
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
                else if (origVisibility.TryGetValue(node, out bool visible) && visible)
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

            /*minX = Mathf.Max(0, minX);
            minY = Mathf.Max(0, minY);
            maxX = Mathf.Min(Futile.screen.pixelWidth, maxX);
            maxY = Mathf.Min(Futile.screen.pixelHeight, maxY);*/

            return new Rect(minX, minY, maxX - minX, maxY - minY);

            IEnumerable<Vector2> GetMeshVertsFor(FNode node)
            {
                // Disregard invisible items
                if (!node.isVisible || !node._isOnStage || node.x == -10000f || node.y == -10000f) yield break;

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

        private void UpdateTexture()
        {
            var bounds = DetermineMeshBounds();
            UpdateCamera(bounds.position + bounds.size / 2f, bounds.size);

            if (textureCache == null || textureCache.width != outputTex.width || textureCache.height != outputTex.height)
            {
                if (textureCache != null) Object.Destroy(textureCache);
                textureCache = new Texture2D(outputTex.width, outputTex.height, TextureFormat.ARGB32, false);
            }
            var oldRT = RenderTexture.active;
            RenderTexture.active = outputTex;
            textureCache.ReadPixels(new Rect(0, 0, outputTex.width, outputTex.height), 0, 0);
            RenderTexture.active = oldRT;
            CropWhitespace(textureCache);
            textureCache.Apply();

            /*if (textureCache != null)
            {
                UnityEngine.Object.Destroy(textureCache);
            }
            textureCache = new Texture2D(Math.Max(1, Mathf.CeilToInt(bounds.width)), Math.Max(1, Mathf.CeilToInt(bounds.height)), TextureFormat.ARGB32, false);

            int x = Mathf.FloorToInt(bounds.x);
            int y = Futile.screen.pixelHeight - Mathf.FloorToInt(bounds.y) - textureCache.height - 1; // y-axis flipped (origin at bottom left vs top left)
            //Graphics.CopyTexture(Futile.instance.camera.activeTexture, 0, 0, x, y, textureCache.width, textureCache.height, textureCache, 0, 0, 0, 0);

            var oldRT = RenderTexture.active;
            var rt = new RenderTexture(Futile.screen.pixelWidth, Futile.screen.pixelHeight, 32, RenderTextureFormat.ARGB32);
            Graphics.Blit(Futile.instance.camera.activeTexture, rt);
            RenderTexture.active = rt;
            textureCache.ReadPixels(new Rect(x, y, textureCache.width, textureCache.height), 0, 0);
            RenderTexture.active = oldRT;

            CropWhitespace(textureCache);
            textureCache.Apply();*/
        }

        private void CropWhitespace(Texture2D texture)
        {
            if (texture.width <= 1 && texture.height <= 1) return;

            Color[] oldPixels = texture.GetPixels();
            int minX = 0, maxX = texture.width - 1, minY = 0, maxY = texture.height - 1;
            bool foundMinX = false, foundMaxX = false, foundMinY = false, foundMaxY = false;

            while (minX < maxX && (!foundMinX || !foundMaxX))
            {
                for (int y = minY; y < maxY; y++)
                {
                    if (!foundMinX && oldPixels[PosToIndex(minX, y)].a > 0f)
                    {
                        foundMinX = true;
                    }
                    if (!foundMaxX && oldPixels[PosToIndex(maxX, y)].a > 0f)
                    {
                        foundMaxX = true;
                    }
                }
                if (!foundMinX) minX++;
                if (!foundMaxX) maxX--;
            }

            while (minX < maxX && minY < maxY && (!foundMinY || !foundMaxY))
            {
                for (int x = minX; x < maxX; x++)
                {
                    if (!foundMinY && oldPixels[PosToIndex(x, minY)].a > 0f)
                    {
                        foundMinY = true;
                    }
                    if (!foundMaxY && oldPixels[PosToIndex(x, maxY)].a > 0f)
                    {
                        foundMaxY = true;
                    }
                }
                if (!foundMinY) minY++;
                if (!foundMaxY) maxY--;
            }

            int width = Math.Max(1, maxX - minX + 1);
            int height = Math.Max(1, maxY - minY + 1);
            minX = Math.Min(minX, texture.width - width);
            minY = Math.Min(minY, texture.height - height);

            Color[] newPixels = texture.GetPixels(minX, minY, width, height);

            texture.Resize(width, height);
            texture.SetPixels(newPixels);

            int PosToIndex(int x, int y) => x + y * texture.width;
        }

        private class TypeComparer : IComparer<Type>
        {
            public int Compare(Type x, Type y)
            {
                return StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name);
            }
        }
    }
}
