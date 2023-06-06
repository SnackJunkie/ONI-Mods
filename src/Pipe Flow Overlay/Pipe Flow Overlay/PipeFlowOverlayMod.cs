using HarmonyLib;
using KMod;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Pipe_Flow_Overlay
{
    public class PipeFlowOverlayMod : UserMod2
    {
        private const int TextureSize = 1024;
        private const float TextureScale = 0.02f;
        private const string CheckBoxContainerName = "PipeFlowOverlayCheckBoxContainer";
        private const string CheckBoxName = "PipeFlowOverlayCheckBox";
        private const string ToggledIconName = "PipeFlowOverlayCheckBoxToggledIcon";

        internal static PipeFlowOverlayMod Instance;
        private bool ShowOverlay { get; set; }

        private GameObject _pipeFlowPrefab;
        private ConcurrentDictionary<ConduitFlow.SOAInfo, ConduitFlow> _conduitFlowManagers;
        private ConcurrentDictionary<GameObject, (GameObject pipeFlow, ConduitFlow.FlowDirections flow)> _liquidConduitFlowRenders;
        private ConcurrentDictionary<GameObject, (GameObject pipeFlow, ConduitFlow.FlowDirections flow)> _gasConduitFlowRenders;
        private ConcurrentDictionary<GameObject, (GameObject pipeFlow, SolidConduitFlow.FlowDirection flow)> _solidConduitFlowRenders;
        private Dictionary<string, Sprite> _flowSprites;
        private HashedString _overlayMode;
        private Sprite _clear;
        private Sprite _border;
        private Sprite _toggledIcon;

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);

            ShowOverlay = true;
            Instance = this;
        }

        internal void Initialize()
        {
            _conduitFlowManagers = new ConcurrentDictionary<ConduitFlow.SOAInfo, ConduitFlow>();
            _liquidConduitFlowRenders = new ConcurrentDictionary<GameObject, (GameObject pipeFlow, ConduitFlow.FlowDirections flow)>();
            _gasConduitFlowRenders = new ConcurrentDictionary<GameObject, (GameObject pipeFlow, ConduitFlow.FlowDirections flow)>();
            _solidConduitFlowRenders = new ConcurrentDictionary<GameObject, (GameObject pipeFlow, SolidConduitFlow.FlowDirection flow)>();
            LoadSprites();
            _pipeFlowPrefab = new GameObject("PipeFlow");
            _pipeFlowPrefab.transform.localScale = new Vector3(TextureScale, TextureScale, 1f);
            _pipeFlowPrefab.SetActive(false);
            _pipeFlowPrefab.transform.SetParent(GameScreenManager.Instance.worldSpaceCanvas.transform);
            Image image = _pipeFlowPrefab.AddComponent<Image>();
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
            image.color = Color.white;
        }

        private void LoadSprites()
        {
            const string flowSprite = @"Flow.png";
            const string noFlowSprite = @"NoFlow.png";

            Texture2D rightArrow = LoadTexture(flowSprite);
            Texture2D downArrow = LoadTexture(flowSprite, 1);
            Texture2D leftArrow = LoadTexture(flowSprite, 2);
            Texture2D upArrow = LoadTexture(flowSprite, 3);
            Texture2D cross = LoadTexture(noFlowSprite);
            Texture2D clear = new Texture2D(TextureSize, TextureSize);
            Color32[] pixels = clear.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;
            clear.SetPixels32(pixels);
            clear.Apply();
            _clear = CreateSprite(clear);
            _border = CreateSprite(LoadTexture("Border.png", 0, 64, 60));
            _toggledIcon = CreateSprite(LoadTexture("ToggledIcon.png", 0, 54, 40));

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
                    texture = clear;
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

        internal void CreateCheckBox(OverlayLegend.OverlayInfo overlayInfo)
        {
            if (overlayInfo != null)
            {
                if (overlayInfo.mode == OverlayModes.LiquidConduits.ID
                    || overlayInfo.mode == OverlayModes.GasConduits.ID
                    || overlayInfo.mode == OverlayModes.SolidConveyor.ID)
                {
                    GameObject checkBoxContainer = overlayInfo.diagrams.FirstOrDefault(go => go.name == CheckBoxContainerName);
                    if (checkBoxContainer == null)
                    {
                        checkBoxContainer = new GameObject(CheckBoxContainerName);
                        RectTransform rectTransform = checkBoxContainer.AddComponent<RectTransform>();
                        rectTransform.anchorMin = new Vector2(0, 0);
                        rectTransform.anchorMax = new Vector2(0, 0);
                        rectTransform.offsetMin = new Vector2(0, 0);
                        rectTransform.offsetMax = new Vector2(0, 0);
                        rectTransform.pivot = new Vector2(0, 0.5f);
                        rectTransform.sizeDelta = new Vector2(0, 0);
                        CanvasRenderer canvasRenderer = checkBoxContainer.AddComponent<CanvasRenderer>();
                        canvasRenderer.cullTransparentMesh = false;
                        HorizontalLayoutGroup horizontalLayoutGroup = checkBoxContainer.AddComponent<HorizontalLayoutGroup>();
                        horizontalLayoutGroup.childForceExpandHeight = false;
                        horizontalLayoutGroup.childForceExpandWidth = false;
                        horizontalLayoutGroup.spacing = 9;
                        Image checkBoxContainerImage = checkBoxContainer.AddComponent<Image>();
                        checkBoxContainerImage.color = Color.white;

                        GameObject checkBox = new GameObject(CheckBoxName);
                        checkBox.transform.SetParent(checkBoxContainer.transform);
                        rectTransform = checkBox.AddComponent<RectTransform>();
                        rectTransform.anchorMin = new Vector2(0, 0);
                        rectTransform.anchorMax = new Vector2(0, 0);
                        rectTransform.offsetMin = new Vector2(0, 0);
                        rectTransform.offsetMax = new Vector2(0, 0);
                        rectTransform.pivot = new Vector2(0, 0.5f);
                        rectTransform.sizeDelta = new Vector2(0, 0);
                        canvasRenderer = checkBox.AddComponent<CanvasRenderer>();
                        canvasRenderer.cullTransparentMesh = false;
                        KToggle kToggle = checkBox.AddComponent<KToggle>();
                        kToggle.artExtension = new KToggleArtExtensions();
                        kToggle.animationTriggers.selectedTrigger = "Highlighted";
                        LayoutElement layoutElement = checkBox.AddComponent<LayoutElement>();
                        layoutElement.minHeight = 20;
                        layoutElement.minWidth = 20;
                        checkBox.AddComponent<HierarchyReferences>();

                        GameObject border = new GameObject("PipeFlowOverlayCheckBoxBorder");
                        border.transform.SetParent(checkBox.transform);
                        rectTransform = border.AddComponent<RectTransform>();
                        rectTransform.anchorMin = new Vector2(0, 0);
                        rectTransform.anchorMax = new Vector2(1, 1);
                        rectTransform.offsetMin = new Vector2(0, 0);
                        rectTransform.offsetMax = new Vector2(0, 0);
                        rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        rectTransform.sizeDelta = new Vector2(0, 0);
                        canvasRenderer = border.AddComponent<CanvasRenderer>();
                        canvasRenderer.cullTransparentMesh = false;
                        Image borderImage = border.AddComponent<Image>();
                        borderImage.sprite = _border;

                        GameObject toggledIcon = new GameObject(ToggledIconName);
                        toggledIcon.transform.SetParent(checkBox.transform);
                        rectTransform = toggledIcon.AddComponent<RectTransform>();
                        rectTransform.anchorMin = new Vector2(0, 0);
                        rectTransform.anchorMax = new Vector2(1, 1);
                        rectTransform.offsetMin = new Vector2(0, 0);
                        rectTransform.offsetMax = new Vector2(0, 0);
                        rectTransform.pivot = new Vector2(0.5f, 0.5f);
                        rectTransform.sizeDelta = new Vector2(0, 0);
                        canvasRenderer = toggledIcon.AddComponent<CanvasRenderer>();
                        canvasRenderer.cullTransparentMesh = false;
                        Image toggledIconImage = toggledIcon.AddComponent<Image>();
                        toggledIconImage.sprite = _toggledIcon;

                        GameObject checkBoxText = new GameObject("PipeFlowOverlayCheckBoxText");
                        checkBoxText.transform.SetParent(checkBoxContainer.transform);
                        rectTransform = checkBoxText.AddComponent<RectTransform>();
                        rectTransform.anchorMin = new Vector2(0, 0);
                        rectTransform.anchorMax = new Vector2(0, 0);
                        rectTransform.offsetMin = new Vector2(0, 0);
                        rectTransform.offsetMax = new Vector2(0, 0);
                        rectTransform.pivot = new Vector2(0, 0.5f);
                        rectTransform.sizeDelta = new Vector2(0, 0);
                        MyLocText locText = checkBoxText.AddComponent<MyLocText>();
                        locText.text = (string)STRINGS.UI.OVERLAY.FLOW.TOGGLE_TEXT;
                        locText.color = Color.black;
                        locText.fontSize = 18;

                        overlayInfo.diagrams.Add(checkBoxContainer);
                    }

                    KToggle toggle = checkBoxContainer.transform.Find(CheckBoxName)?.GetComponent<KToggle>();
                    if (toggle != null)
                        toggle.isOn = ShowOverlay;
                    checkBoxContainer.transform.Find($"{CheckBoxName}/{ToggledIconName}")?.gameObject.SetActive(ShowOverlay);
                }
            }
        }

        internal void ToggleValueChanged(KToggle kToggle, bool value)
        {
            if (kToggle.gameObject.name == CheckBoxName)
            {
                ShowOverlay = value;
                kToggle.gameObject.transform.Find(ToggledIconName)?.gameObject.SetActive(ShowOverlay);
                ToggleOverlay(_overlayMode);
            }
        }

        private static void LogTransformTree(GameObject go, int depth)
        {
            string prefix = "";
            for (int i = 0; i < depth; i++)
                prefix += "\t";

            Debug.Log($"{prefix}{go.name} - {go.transform.localScale} - {go.transform.localPosition}");
            foreach (Component component in go.GetComponents<Component>())
            {
                string log = $"{prefix} • {component.GetType()}";
                if (component is VerticalLayoutGroup verticalLayoutGroup)
                    log += $" - childControlHeight: {verticalLayoutGroup.childControlHeight} - childControlWidth: {verticalLayoutGroup.childControlWidth} - childForceExpandHeight: {verticalLayoutGroup.childForceExpandHeight} - childForceExpandWidth: {verticalLayoutGroup.childForceExpandWidth} - minWidth: {verticalLayoutGroup.minWidth} - preferredWidth: {verticalLayoutGroup.preferredWidth} - childScaleHeight: {verticalLayoutGroup.childScaleHeight} - childScaleWidth: {verticalLayoutGroup.childScaleWidth} - flexibleHeight: {verticalLayoutGroup.flexibleHeight} - flexibleWidth: {verticalLayoutGroup.flexibleWidth} - layoutPriority: {verticalLayoutGroup.layoutPriority} - minHeight: {verticalLayoutGroup.minHeight} - spacing: {verticalLayoutGroup.spacing}";
                if (component is LayoutElement layoutElement)
                    log += $" - flexibleHeight: {layoutElement.flexibleHeight} - flexibleWidth: {layoutElement.flexibleWidth} - hideFlags: {layoutElement.hideFlags} - ignoreLayout: {layoutElement.ignoreLayout} - layoutPriority: {layoutElement.layoutPriority} - minHeight: {layoutElement.minHeight} - minWidth: {layoutElement.minWidth} - preferredHeight: {layoutElement.preferredHeight} - preferredWidth: {layoutElement.preferredWidth}";
                if (component is HorizontalLayoutGroup horizontalLayoutGroup)
                    log += $" - childControlHeight: {horizontalLayoutGroup.childControlHeight} - childControlWidth: {horizontalLayoutGroup.childControlWidth} - childForceExpandHeight: {horizontalLayoutGroup.childForceExpandHeight} - childForceExpandWidth: {horizontalLayoutGroup.childForceExpandWidth} - childScaleHeight: {horizontalLayoutGroup.childScaleHeight} - childScaleWidth: {horizontalLayoutGroup.childScaleWidth} - flexibleHeight: {horizontalLayoutGroup.flexibleHeight} - flexibleWidth: {horizontalLayoutGroup.flexibleWidth} - layoutPriority: {horizontalLayoutGroup.layoutPriority} - minHeight: {horizontalLayoutGroup.minHeight} - minWidth: {horizontalLayoutGroup.minWidth} - padding: {horizontalLayoutGroup.padding} - preferredHeight: {horizontalLayoutGroup.preferredHeight} - preferredWidth: {horizontalLayoutGroup.preferredWidth} - reverseArrangement: {horizontalLayoutGroup.reverseArrangement} - spacing: {horizontalLayoutGroup.spacing} - transform: {horizontalLayoutGroup.transform} - useGUILayout: {horizontalLayoutGroup.useGUILayout}";
                if (component is RectTransform rectTransform)
                    log += $" - anchoredPosition: {rectTransform.anchoredPosition} - anchoredPosition3D: {rectTransform.anchoredPosition3D} - anchorMax: {rectTransform.anchorMax} - anchorMin: {rectTransform.anchorMin} - eulerAngles: {rectTransform.eulerAngles} - forward: {rectTransform.forward} - hierarchyCapacity: {rectTransform.hierarchyCapacity} - hierarchyCount: {rectTransform.hierarchyCount} - localEulerAngles: {rectTransform.localEulerAngles} - localPosition: {rectTransform.localPosition} - localRotation: {rectTransform.localRotation} - localScale: {rectTransform.localScale} - lossyScale: {rectTransform.lossyScale} - offsetMax: {rectTransform.offsetMax} - offsetMin: {rectTransform.offsetMin} - pivot: {rectTransform.pivot} - position: {rectTransform.position} - rect: {rectTransform.rect} - right: {rectTransform.right} - rotation: {rectTransform.rotation} - sizeDelta: {rectTransform.sizeDelta} - up: {rectTransform.up}";
                if (component is CanvasRenderer canvasRenderer)
                    log += $" - absoluteDepth: {canvasRenderer.absoluteDepth} - clippingSoftness: {canvasRenderer.clippingSoftness} - cull: {canvasRenderer.cull} - cullTransparentMesh: {canvasRenderer.cullTransparentMesh} - hasPopInstruction: {canvasRenderer.hasPopInstruction} - hasRectClipping: {canvasRenderer.hasRectClipping} - materialCount: {canvasRenderer.materialCount} - popMaterialCount: {canvasRenderer.popMaterialCount} - relativeDepth: {canvasRenderer.relativeDepth}";
                if (component is KToggle kToggle)
                    log += $" - disabledTrigger: {kToggle.animationTriggers?.disabledTrigger} - highlightedTrigger: {kToggle.animationTriggers?.highlightedTrigger} - normalTrigger: {kToggle.animationTriggers?.normalTrigger} - pressedTrigger: {kToggle.animationTriggers?.pressedTrigger} - selectedTrigger: {kToggle.animationTriggers?.selectedTrigger} - colorMultiplier: {kToggle.colors.colorMultiplier} - disabledColor: {kToggle.colors.disabledColor} - fadeDuration: {kToggle.colors.fadeDuration} - highlightedColor: {kToggle.colors.highlightedColor} - normalColor: {kToggle.colors.normalColor} - pressedColor: {kToggle.colors.pressedColor} - selectedColor: {kToggle.colors.selectedColor} - enabled: {kToggle.enabled} - GetMouseOver: {kToggle.GetMouseOver} - group: {kToggle.group} - image: {kToggle.image} - interactable: {kToggle.interactable} - isOn: {kToggle.isOn} - mode: {kToggle.navigation.mode} - selectOnDown: {kToggle.navigation.selectOnDown} - selectOnLeft: {kToggle.navigation.selectOnLeft} - selectOnRight: {kToggle.navigation.selectOnRight} - selectOnUp: {kToggle.navigation.selectOnUp} - wrapAround: {kToggle.navigation.wrapAround} - disabledSprite: {kToggle.spriteState.disabledSprite} - highlightedSprite: {kToggle.spriteState.highlightedSprite} - pressedSprite: {kToggle.spriteState.pressedSprite} - selectedSprite: {kToggle.spriteState.selectedSprite} - targetGraphic: {kToggle.targetGraphic} - transition: {kToggle.transition} - useGUILayout: {kToggle.useGUILayout} - artExtension: {kToggle.artExtension != null}";
                if (component is Image image)
                    log += $" - rect: {image.sprite?.rect} - width: {image.sprite?.texture?.width} - height: {image.sprite?.texture?.height} - {image.material} - {image.color}";
                if (component is ImageToggleState imageToggleState)
                    log += $" - enabled: {imageToggleState.enabled} - hideFlags: {imageToggleState.hideFlags} - isActiveAndEnabled: {imageToggleState.isActiveAndEnabled} - IsDisabled: {imageToggleState.IsDisabled} - isNull: {imageToggleState.isNull} - isSpawned: {imageToggleState.isSpawned} - tag: {imageToggleState.tag} - useGUILayout: {imageToggleState.useGUILayout}";
                if (component is LocText locText)
                    log += $" - {locText.text} - {locText.fontSize}";
                Debug.Log(log);
            }

            for (int i = 0; i < go.transform.childCount; i++)
                LogTransformTree(go.transform.GetChild(i).gameObject, depth + 1);
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
            if (!ShowOverlay)
                active = false;

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
                (_, entry) => UpdateFlowDirection(conduitGO, entry.pipeFlow, overwrite ? delta : entry.flow | delta, active));
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
                (_, entry) => UpdateFlowDirection(conduitGO, entry.pipeFlow, overwrite ? delta : entry.flow | delta, active));
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
                (_, entry) => UpdateFlowDirection(conduitGO, entry.pipeFlow, flow, active));
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

            return UpdateFlowDirection(conduitGO, pipeFlow, flow, active);
        }

        private (GameObject pipeFlow, T flow) UpdateFlowDirection<T>(GameObject conduitGO, GameObject pipeFlow, T flow, bool active)
        {
            if (!ShowOverlay)
                active = false;

            bool is_endpoint = false;
            if (flow.ToString().ToLower() == "none" && conduitGO != null)
            {
                Conduit conduit = conduitGO.GetComponent<Conduit>();
                if (conduit != null)
                {
                    IUtilityNetworkMgr utilityNetworkMgr = conduit.GetNetworkManager();
                    if (utilityNetworkMgr != null)
                    {
                        object endpoint = utilityNetworkMgr.GetEndpoint(conduit.Cell);
                        if (endpoint != null)
                        {
                            is_endpoint = true;
                        }
                    }
                }
            }

            if (!_flowSprites.TryGetValue(flow.ToString().ToLower(), out Sprite sprite) || is_endpoint)
            {
                sprite = _clear;
            }

            Image image = pipeFlow.GetComponent<Image>();
            image.sprite = sprite;

            pipeFlow.SetActive(active);

            return (pipeFlow, flow);
        }

        private Texture2D LoadTexture(string fileName, int rotate = 0, int width = TextureSize, int height = TextureSize)
        {
            string fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", fileName);

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false)
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

    class MyLocText : LocText
    {
        protected override void Awake()
        {
            key = "";
            base.Awake();
        }
    }
}
