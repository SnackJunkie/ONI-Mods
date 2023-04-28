using HarmonyLib;
using KMod;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Pipe_Flow_Overlay
{
    public class PipeFlowOverlayMod : UserMod2
    {
        private const int TextureSize = 1024;
        private const float TextureScale = 0.02f;

        internal static PipeFlowOverlayMod Instance;

        private GameObject _pipeFlowPrefab;
        private ConcurrentDictionary<ConduitFlow.SOAInfo, ConduitFlow> _conduitFlowManagers;
        private ConcurrentDictionary<GameObject, (GameObject pipeFlow, ConduitFlow.FlowDirections flow)> _liquidConduitFlowRenders;
        private ConcurrentDictionary<GameObject, (GameObject pipeFlow, ConduitFlow.FlowDirections flow)> _gasConduitFlowRenders;
        private ConcurrentDictionary<GameObject, (GameObject pipeFlow, SolidConduitFlow.FlowDirection flow)> _solidConduitFlowRenders;
        private Dictionary<string, Sprite> _flowSprites;
        private HashedString _overlayMode;

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            Instance = this;
        }

        internal void Initialize()
        {
            _conduitFlowManagers = new ConcurrentDictionary<ConduitFlow.SOAInfo, ConduitFlow>();
            _liquidConduitFlowRenders = new ConcurrentDictionary<GameObject, (GameObject pipeFlow, ConduitFlow.FlowDirections flow)>();
            _gasConduitFlowRenders = new ConcurrentDictionary<GameObject, (GameObject pipeFlow, ConduitFlow.FlowDirections flow)>();
            _solidConduitFlowRenders = new ConcurrentDictionary<GameObject, (GameObject pipeFlow, SolidConduitFlow.FlowDirection flow)>();
            LoadFlowSprites();
            _pipeFlowPrefab = new GameObject("PipeFlow");
            _pipeFlowPrefab.transform.localScale = new Vector3(TextureScale, TextureScale, 1f);
            _pipeFlowPrefab.SetActive(false);
            _pipeFlowPrefab.transform.SetParent(GameScreenManager.Instance.worldSpaceCanvas.transform);
            Image image = _pipeFlowPrefab.AddComponent<Image>();
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
            image.color = Color.white;
        }

        private void LoadFlowSprites()
        {
            const string flowSprite = @"Resources\Flow.png";
            const string noFlowSprite = @"Resources\NoFlow.png";

            Texture2D rightArrow = LoadTexture(flowSprite);
            Texture2D downArrow = LoadTexture(flowSprite, 1);
            Texture2D leftArrow = LoadTexture(flowSprite, 2);
            Texture2D upArrow = LoadTexture(flowSprite, 3);
            Texture2D cross = LoadTexture(noFlowSprite);

            _flowSprites = new Dictionary<string, Sprite>();

            for (int i = 0; i <= (int)ConduitFlow.FlowDirections.All; i++)
            {
                string flow = ((ConduitFlow.FlowDirections)i).ToString().ToLower();
                if (!_flowSprites.ContainsKey(flow))
                {
                    Debug.Log($"Generating sprite for flow: {flow}");
                    _flowSprites.Add(flow, GetSprite(flow));
                }
            }


            Sprite GetSprite(string flow)
            {
                Texture2D texture;
                if (flow == "none")
                {
                    texture = cross;
                }
                else
                {
                    texture = new Texture2D(TextureSize, TextureSize);
                    Color32[] pixels = texture.GetPixels32();
                    for (int i = 0; i < pixels.Length; i++)
                        pixels[i] = Color.clear;
                    texture.SetPixels32(pixels);
                    texture.Apply();
                }

                if (flow.Contains("right"))
                {
                    texture = Merge(texture, rightArrow);
                }

                if (flow.Contains("down"))
                {
                    texture = Merge(texture, downArrow);
                }

                if (flow.Contains("left"))
                {
                    texture = Merge(texture, leftArrow);
                }

                if (flow.Contains("up"))
                {
                    texture = Merge(texture, upArrow);
                }

                return CreateSprite(texture);
            }
        }

        internal void RegisterConduitFlowManager(ConduitFlow.SOAInfo soaInfo, ConduitFlow manager)
        {
            _conduitFlowManagers.TryAdd(soaInfo, manager);
        }

        internal ConduitFlow GetConduitFlow(ConduitFlow.SOAInfo soaInfo)
        {
            if (_conduitFlowManagers.TryGetValue(soaInfo, out ConduitFlow manager))
            {
                return manager;
            }

            return null;
        }

        internal void ToggleOverlay(HashedString overlayMode)
        {
            _overlayMode = overlayMode;
            Toggle(ref _liquidConduitFlowRenders, _overlayMode == OverlayModes.LiquidConduits.ID);
            Toggle(ref _gasConduitFlowRenders, _overlayMode == OverlayModes.GasConduits.ID);
            Toggle(ref _solidConduitFlowRenders, _overlayMode == OverlayModes.SolidConveyor.ID);
        }

        private void Toggle<T>(ref ConcurrentDictionary<GameObject, (GameObject pipeFlow, T flow)> dict, bool active)
        {
            foreach (KeyValuePair<GameObject, (GameObject pipeFlow, T flow)> entry in dict.ToArray())
            {
                dict.AddOrUpdate(entry.Key, entry.Value /*Should never be hit*/, (_, current) =>
                {
                    current.pipeFlow.SetActive(active);
                    return current;
                });
            }
        }

        internal void ClearLiquidConduitFlowDirections()
        {
            foreach (var entry in _liquidConduitFlowRenders.ToArray())
            {
                AddOrUpdateLiquidConduitFlowDirection(entry.Key, ConduitFlow.FlowDirections.None, true);
            }
        }

        internal void ClearGasConduitFlowDirections()
        {
            foreach (var entry in _gasConduitFlowRenders.ToArray())
            {
                AddOrUpdateGasConduitFlowDirection(entry.Key, ConduitFlow.FlowDirections.None, true);
            }
        }

        internal void ClearSolidConduitFlowDirections()
        {
            foreach (var entry in _solidConduitFlowRenders.ToArray())
            {
                AddOrUpdateSolidConduitFlowDirection(entry.Key, SolidConduitFlow.FlowDirection.None);
            }
        }

        internal void AddOrUpdateLiquidConduitFlowDirection(GameObject conduitGO, ConduitFlow.FlowDirections delta, bool overwrite = false)
        {
            bool active = _overlayMode == OverlayModes.LiquidConduits.ID;
            _liquidConduitFlowRenders.AddOrUpdate(conduitGO,
                _ => AddFlowDirection(conduitGO, delta, active),
                (_, entry) => UpdateFlowDirection(entry.pipeFlow, overwrite ? delta : entry.flow | delta, active));
        }

        internal void RemoveLiquidConduitFlow(GameObject conduitGO)
        {
            if (_liquidConduitFlowRenders.TryRemove(conduitGO, out var entry))
                Object.Destroy(entry.pipeFlow);
        }

        internal void AddOrUpdateGasConduitFlowDirection(GameObject conduitGO, ConduitFlow.FlowDirections delta, bool overwrite = false)
        {
            bool active = _overlayMode == OverlayModes.GasConduits.ID;
            _gasConduitFlowRenders.AddOrUpdate(conduitGO,
                _ => AddFlowDirection(conduitGO, delta, active),
                (_, entry) => UpdateFlowDirection(entry.pipeFlow, overwrite ? delta : entry.flow | delta, active));
        }

        internal void RemoveGasConduitFlow(GameObject conduitGO)
        {
            if (_gasConduitFlowRenders.TryRemove(conduitGO, out var entry))
                Object.Destroy(entry.pipeFlow);
        }

        internal void AddOrUpdateSolidConduitFlowDirection(GameObject conduitGO, SolidConduitFlow.FlowDirection flow)
        {
            bool active = _overlayMode == OverlayModes.SolidConveyor.ID;
            _solidConduitFlowRenders.AddOrUpdate(conduitGO,
                _ => AddFlowDirection(conduitGO, flow, active),
                (_, entry) => UpdateFlowDirection(entry.pipeFlow, flow, active));
        }

        internal void RemoveSolidConduitFlow(GameObject conduitGO)
        {
            if (_solidConduitFlowRenders.TryRemove(conduitGO, out var entry))
                Object.Destroy(entry.pipeFlow);
        }

        private (GameObject pipeFlow, T flow) AddFlowDirection<T>(GameObject conduitGO, T flow, bool active)
        {
            GameObject pipeFlow = Util.KInstantiateUI(_pipeFlowPrefab, GameScreenManager.Instance.worldSpaceCanvas, true);

            Vector2I xy = Grid.PosToXY(conduitGO.transform.position);

            pipeFlow.transform.position = new Vector3(xy.X + 0.5f, xy.Y + 0.5f, Grid.GetLayerZ(Grid.SceneLayer.SceneMAX));
            pipeFlow.transform.SetAsLastSibling();

            return UpdateFlowDirection(pipeFlow, flow, active);
        }

        private (GameObject pipeFlow, T flow) UpdateFlowDirection<T>(GameObject pipeFlow, T flow, bool active)
        {
            if (_flowSprites.TryGetValue(flow.ToString().ToLower(), out Sprite sprite))
            {
                Image image = pipeFlow.GetComponent<Image>();
                image.sprite = sprite;
            }

            pipeFlow.SetActive(active);

            return (pipeFlow, flow);
        }

        private Texture2D LoadTexture(string path, int rotate = 0)
        {
            string fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), path);

            Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGB24, false)
            {
                filterMode = FilterMode.Trilinear
            };

            byte[] data = File.ReadAllBytes(fullPath);
            texture.LoadImage(data);

            for (int i = 0; i < rotate; i++)
                texture = Rotate(texture);

            return texture;
        }

        private static Sprite CreateSprite(Texture2D texture)
        {
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            return Sprite.Create(texture, rect, pivot, 1f);
        }

        private Texture2D Rotate(Texture2D originalTexture)
        {
            Color32[] original = originalTexture.GetPixels32();
            Color32[] rotated = new Color32[original.Length];

            int h = originalTexture.height;
            int w = originalTexture.width;

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    int iRotated = (j + 1) * h - i - 1;
                    int iOriginal = original.Length - 1 - (i * w + j);
                    rotated[iRotated] = original[iOriginal];
                }
            }

            Texture2D rotatedTexture = new Texture2D(h, w);
            rotatedTexture.SetPixels32(rotated);
            rotatedTexture.Apply();
            return rotatedTexture;
        }

        private Texture2D Merge(Texture2D texture1, Texture2D texture2)
        {
            Color32[] colors1 = texture1.GetPixels32();
            Color32[] colors2 = texture2.GetPixels32();
            Color32[] merge = new Color32[colors1.Length];

            int h = texture1.height;
            int w = texture1.width;

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    int index = (i + 1) * w - j - 1;
                    Color32 color1 = colors1[index];
                    Color32 color2 = colors2[index];

                    if (color1.a > color2.a)
                        merge[index] = color1;
                    else
                        merge[index] = color2;
                }
            }

            Texture2D mergeTexture = new Texture2D(w, h);
            mergeTexture.SetPixels32(merge);
            mergeTexture.Apply();
            return mergeTexture;
        }
    }
}
