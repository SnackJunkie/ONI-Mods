﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace PipeFlowOverlay
{
    internal class PipeFlowOverlayIconController : KMonoBehaviour
    {
        private const int TextureSize = 1024;
        internal static Dictionary<string, Sprite> _flowSprites;
        internal static Sprite _clear;
        private Conduit _conduit;
        private ConduitFlow _conduitFlow;
        private Image _image;

        internal bool IconIsDirty { get; set; }

        protected override void OnPrefabInit()
        {
            _conduit = gameObject.GetComponentInParent<Conduit>();
            _conduitFlow = _conduit.GetFlowManager();
            _conduitFlow.onConduitsRebuilt += OnConduitsRebuilt;
            _image = gameObject.AddComponent<Image>();
            _image.type = Image.Type.Simple;
            _image.raycastTarget = false;
            _image.color = Color.white;
        }

        protected override void OnCleanUp()
        {
            _conduitFlow.onConduitsRebuilt -= OnConduitsRebuilt;
        }

        private void OnConduitsRebuilt()
        {
            IconIsDirty = true;
        }

#pragma warning disable IDE0051 // Remove unused private members
        private void Update()
#pragma warning restore IDE0051 // Remove unused private members
        {
            if (IconIsDirty)
            {
                ConduitFlow conduitFlow = _conduit.GetFlowManager();
                ConduitFlow.FlowDirections flowDirections = conduitFlow.GetPermittedFlow(_conduit.Cell);
                string flow = flowDirections.ToString().Trim().ToLower();
                if (!_flowSprites.TryGetValue(flow, out Sprite sprite))
                    sprite = _clear;
                _image.sprite = sprite;

                IconIsDirty = false;
            }
        }

        #region Helper methods
        internal static void LoadSprites()
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

        private static Texture2D LoadTexture(string fileName, int rotate = 0, int width = TextureSize, int height = TextureSize)
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

        private static Texture2D Rotate(Texture2D originalTexture)
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

        private static Texture2D Merge(Texture2D texture1, Texture2D texture2)
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

        private static Sprite CreateSprite(Texture2D texture)
        {
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            return Sprite.Create(texture, rect, pivot, 1f);
        }
        #endregion
    }
}