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
        internal HashedString OverlayMode { get; set; }

        private GameObject _pipeFlowPrefab;
        private ConcurrentBag<GameObject> _flowRenders;
        private ConcurrentDictionary<ConduitFlow.SOAInfo, ConduitFlow> _conduitFlowManagers;
        private ConcurrentDictionary<int, ConduitFlow.FlowDirections> _liquidConduitFlowDirections;
        private ConcurrentDictionary<int, ConduitFlow.FlowDirections> _gasConduitFlowDirections;
        private ConcurrentDictionary<int, SolidConduitFlow.FlowDirection> _solidConduitFlowDirections;
        private ConcurrentDictionary<string, Sprite> _flowSprites;
        private Texture2D _rightArrow;
        private Texture2D _downArrow;
        private Texture2D _leftArrow;
        private Texture2D _upArrow;
        private Texture2D _cross;

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            Instance = this;
        }

        internal void Initialize()
        {
            _flowRenders = new ConcurrentBag<GameObject>();
            _conduitFlowManagers = new ConcurrentDictionary<ConduitFlow.SOAInfo, ConduitFlow>();
            _liquidConduitFlowDirections = new ConcurrentDictionary<int, ConduitFlow.FlowDirections>();
            _gasConduitFlowDirections = new ConcurrentDictionary<int, ConduitFlow.FlowDirections>();
            _solidConduitFlowDirections = new ConcurrentDictionary<int, SolidConduitFlow.FlowDirection>();
            LoadInitialFlowSprites();
            _pipeFlowPrefab = new GameObject("PipeFlow");
            _pipeFlowPrefab.transform.localScale = new Vector3(TextureScale, TextureScale, 1f);
            _pipeFlowPrefab.SetActive(false);
            _pipeFlowPrefab.transform.SetParent(GameScreenManager.Instance.worldSpaceCanvas.transform);
            Image image = _pipeFlowPrefab.AddComponent<Image>();
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
            image.color = Color.white;
        }

        private void LoadInitialFlowSprites()
        {
            const string flowSprite = @"Resources\Flow.png";
            const string noFlowSprite = @"Resources\NoFlow.png";

            _flowSprites = new ConcurrentDictionary<string, Sprite>();
            _flowSprites.TryAdd("right", LoadSprite(flowSprite, ref _rightArrow));
            _flowSprites.TryAdd("down", LoadSprite(flowSprite, ref _downArrow, 1));
            _flowSprites.TryAdd("left", LoadSprite(flowSprite, ref _leftArrow, 2));
            _flowSprites.TryAdd("up", LoadSprite(flowSprite, ref _upArrow, 3));
            _flowSprites.TryAdd("none", LoadSprite(noFlowSprite, ref _cross));
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

        internal void ClearFlowOverlay()
        {
            while (_flowRenders.TryTake(out GameObject gameObject))
                Object.Destroy(gameObject);
        }

        internal void ShowLiquidConduitOverlay()
        {
            ClearFlowOverlay();

            ConduitFlow manager = Game.Instance.liquidConduitFlow;

            foreach (KeyValuePair<int, ConduitFlow.FlowDirections> entry in _liquidConduitFlowDirections.ToArray())
            {
                GameObject conduitGO = manager.soaInfo.GetConduitGO(entry.Key);

                ProcessConduit(conduitGO, entry.Value.ToString());
            }
        }

        internal void ShowGasConduitOverlay()
        {
            ClearFlowOverlay();

            ConduitFlow manager = Game.Instance.gasConduitFlow;

            foreach (KeyValuePair<int, ConduitFlow.FlowDirections> entry in _gasConduitFlowDirections.ToArray())
            {
                GameObject conduitGO = manager.soaInfo.GetConduitGO(entry.Key);

                ProcessConduit(conduitGO, entry.Value.ToString());
            }
        }

        internal void ShowSolidConduitOverlay()
        {
            ClearFlowOverlay();

            SolidConduitFlow manager = Game.Instance.solidConduitFlow;

            foreach (KeyValuePair<int, SolidConduitFlow.FlowDirection> entry in _solidConduitFlowDirections.ToArray())
            {
                GameObject conduitGO = manager.GetSOAInfo().GetConduitGO(entry.Key);

                ProcessConduit(conduitGO, entry.Value.ToString());
            }
        }

        private void ProcessConduit(GameObject conduitGO, string flow)
        {
            Sprite sprite = _flowSprites.GetOrAdd(flow.Trim().ToLower(), GetSprite);

            RenderSpriteAtConduit(sprite, conduitGO);
        }

        private Sprite GetSprite(string flow)
        {
            Texture2D texture;
            if (flow == "none")
            {
                texture = _cross;
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
                texture = Merge(texture, _rightArrow);

            if (flow.Contains("down"))
                texture = Merge(texture, _downArrow);

            if (flow.Contains("left"))
                texture = Merge(texture, _leftArrow);

            if (flow.Contains("up"))
                texture = Merge(texture, _upArrow);

            return CreateSprite(texture);
        }

        internal void RenderSpriteAtConduit(Sprite sprite, GameObject conduitGO, float angle = 0)
        {
            GameObject pipeFlow = Util.KInstantiateUI(_pipeFlowPrefab, GameScreenManager.Instance.worldSpaceCanvas, true);

            Image image = pipeFlow.GetComponent<Image>();
            image.sprite = sprite;

            Vector2I xy = Grid.PosToXY(conduitGO.transform.position);

            pipeFlow.transform.position = new Vector3(xy.X + 0.5f, xy.Y + 0.5f, Grid.GetLayerZ(Grid.SceneLayer.SceneMAX));
            pipeFlow.transform.Rotate(0, 0, angle);
            pipeFlow.transform.SetAsLastSibling();

            _flowRenders.Add(pipeFlow);
        }

        internal void ClearLiquidConduitFlowDirections()
        {
            _liquidConduitFlowDirections.Clear();
        }

        internal void ClearGasConduitFlowDirections()
        {
            _gasConduitFlowDirections.Clear();
        }

        internal void ClearSolidConduitFlowDirections()
        {
            _solidConduitFlowDirections.Clear();
        }

        internal void AddLiquidConduitFlowDirection(int idx, ConduitFlow.FlowDirections delta)
        {
            _liquidConduitFlowDirections.AddOrUpdate(idx, delta, (_, current) => current | delta);
        }

        internal void AddGasConduitFlowDirection(int idx, ConduitFlow.FlowDirections delta)
        {
            _gasConduitFlowDirections.AddOrUpdate(idx, delta, (_, current) => current | delta);
        }

        internal void SetSolidConduitFlowDirection(int idx, SolidConduitFlow.FlowDirection directions)
        {
            _solidConduitFlowDirections.AddOrUpdate(idx, directions, (_, __) => directions);
        }

        private Sprite LoadSprite(string path, ref Texture2D texture, int rotate = 0)
        {
            string fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), path);

            Debug.Log($"Loading texture from {fullPath}");

            texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGB24, false)
            {
                filterMode = FilterMode.Trilinear
            };

            byte[] data = File.ReadAllBytes(fullPath);
            texture.LoadImage(data);

            for (int i = 0; i < rotate; i++)
                texture = Rotate(texture);

            return CreateSprite(texture);
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
