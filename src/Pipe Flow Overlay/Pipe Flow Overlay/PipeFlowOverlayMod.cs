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

        private Sprite _flow;
        private Sprite _noFlow;
        private GameObject _pipeFlowPrefab;
        private ConcurrentBag<GameObject> _flowRenders;
        private ConcurrentDictionary<ConduitFlow.SOAInfo, ConduitFlow> _conduitFlowManagers;
        private ConcurrentDictionary<int, ConduitFlow.FlowDirections> _liquidConduitFlowDirections;
        private ConcurrentDictionary<int, ConduitFlow.FlowDirections> _gasConduitFlowDirections;
        private ConcurrentDictionary<int, SolidConduitFlow.FlowDirection> _solidConduitFlowDirections;

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            Instance = this;
        }

        internal void Initialize()
        {
            _flow = LoadSprite(@"Resources\Flow.png");
            _noFlow = LoadSprite(@"Resources\NoFlow.png");
            _flowRenders = new ConcurrentBag<GameObject>();
            _conduitFlowManagers = new ConcurrentDictionary<ConduitFlow.SOAInfo, ConduitFlow>();
            _liquidConduitFlowDirections = new ConcurrentDictionary<int, ConduitFlow.FlowDirections>();
            _gasConduitFlowDirections = new ConcurrentDictionary<int, ConduitFlow.FlowDirections>();
            _solidConduitFlowDirections = new ConcurrentDictionary<int, SolidConduitFlow.FlowDirection>();
            _pipeFlowPrefab = new GameObject("PipeFlow");
            _pipeFlowPrefab.transform.localScale = new Vector3(TextureScale, TextureScale, 1f);
            _pipeFlowPrefab.SetActive(false);
            _pipeFlowPrefab.transform.SetParent(GameScreenManager.Instance.worldSpaceCanvas.transform);
            Image image = _pipeFlowPrefab.AddComponent<Image>();
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
            image.color = Color.white;
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
            bool none = true;

            if (flow.ToLower().Contains("up"))
            {
                RenderSpriteAtConduit(_flow, conduitGO, 90f);
                none = false;
            }

            if (flow.ToLower().Contains("down"))
            {
                RenderSpriteAtConduit(_flow, conduitGO, -90f);
                none = false;
            }

            if (flow.ToLower().Contains("left"))
            {
                RenderSpriteAtConduit(_flow, conduitGO, 180f);
                none = false;
            }

            if (flow.ToLower().Contains("right"))
            {
                RenderSpriteAtConduit(_flow, conduitGO);
                none = false;
            }

            if (none)
            {
                RenderSpriteAtConduit(_noFlow, conduitGO);
            }
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

        private Sprite LoadSprite(string path)
        {
            string fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), path);

            Debug.Log($"Loading texture from {fullPath}");

            Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGB24, false)
            {
                filterMode = FilterMode.Trilinear
            };

            byte[] data = File.ReadAllBytes(fullPath);
            texture.LoadImage(data);

            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            return Sprite.Create(texture, rect, pivot, 1f);
        }
    }
}
