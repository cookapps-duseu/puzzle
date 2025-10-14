using System.Collections.Generic;
using System.Linq;
using RabbitDog;
using RabbitDog.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Template
{
    public class TopPanelSingleUseHelper : SingletonMonoBehaviour<TopPanelSingleUseHelper>
    {
        private readonly Dictionary<TopPanelType, TopPanelBase> panels = new ();
        private readonly Dictionary<TopPanelType, RectTransformLayoutSnapshot> panelLayouts = new ();
        private Transform topUIOriginTr;

        private List<TopPanelBar> topUIs = new ();

        public async Awaitable Initialize(string prefabAddress)
        {
            var topUIOrigin = await Addressables.InstantiateAsync(prefabAddress, transform).WaitUntilDone();
            topUIOriginTr = topUIOrigin.transform;
            int childCount = topUIOriginTr.childCount;
            for (var i = 0; i < childCount; i++)
            {
                Transform child = topUIOriginTr.GetChild(i);
                var panel = child.GetComponent<TopPanelBase>();
                panels.TryAdd(panel.PanelType, panel);
                panelLayouts.TryAdd(panel.PanelType, new RectTransformLayoutSnapshot(panel.CachedRectTr));
            }

            topUIOrigin.SetActive(false);
        }

        public void Clear()
        {
            foreach ((_, TopPanelBase ui) in panels)
            {
                if (ui == null) continue;

                ui.CachedRectTr.SetParent(topUIOriginTr);
                ApplyLayout(ui.PanelType, ui.CachedRectTr);
            }
            panels.Clear();
            panelLayouts.Clear();

            Addressables.ReleaseInstance(topUIOriginTr.gameObject);
            Destroy(topUIOriginTr.gameObject);
        }

        public TopPanelBase GetPanel(TopPanelType type)
        {
            return panels[type];
        }

        public void Push(TopPanelBar topUI)
        {
            topUIs.Add(topUI);
            for (var i = 0; i < topUI.UsePanelTypes.Length; i++)
            {
                TopPanelType type = topUI.UsePanelTypes[i];
                topUI.AddPanel(type, panels[type].CachedRectTr);
            }
        }

        public void Pop(TopPanelBar topUI)
        {
            topUIs.Remove(topUI);
            for (var i = 0; i < topUI.UsePanelTypes.Length; i++)
            {
                TopPanelType type = topUI.UsePanelTypes[i];
                TopPanelBase panel = panels[type];
                var isOccupied = false;
                for (int j = topUIs.Count - 1; j >= 0; j--)
                {
                    if (topUIs[j].UsePanelTypes.Contains(type))
                    {
                        topUIs[j].AddPanel(type, panel.CachedRectTr);
                        isOccupied = true;
                        break;
                    }
                }

                if (!isOccupied)
                {
                    panel.CachedTr.SetParent(topUIOriginTr, false);
                    ApplyLayout(type, panel.CachedRectTr);
                }
            }
        }

        public void ApplyLayout(TopPanelType type, RectTransform rectTr)
        {
            if (!panelLayouts.TryGetValue(type, out var snapshot))
            {
                return;
            }

            snapshot.Apply(rectTr);
        }
    }

    public readonly struct RectTransformLayoutSnapshot
    {
        public RectTransformLayoutSnapshot(RectTransform rectTransform)
        {
            AnchorMin = rectTransform.anchorMin;
            AnchorMax = rectTransform.anchorMax;
            Pivot = rectTransform.pivot;
            SizeDelta = rectTransform.sizeDelta;
            AnchoredPosition = rectTransform.anchoredPosition;
            LocalScale = rectTransform.localScale;
            LocalRotation = rectTransform.localRotation;
        }

        public Vector2 AnchorMin { get; }
        public Vector2 AnchorMax { get; }
        public Vector2 Pivot { get; }
        public Vector2 SizeDelta { get; }
        public Vector2 AnchoredPosition { get; }
        public Vector3 LocalScale { get; }
        public Quaternion LocalRotation { get; }

        public void Apply(RectTransform rectTransform)
        {
            rectTransform.anchorMin = AnchorMin;
            rectTransform.anchorMax = AnchorMax;
            rectTransform.pivot = Pivot;
            rectTransform.sizeDelta = SizeDelta;
            rectTransform.localScale = LocalScale;
            rectTransform.localRotation = LocalRotation;
            rectTransform.anchoredPosition = AnchoredPosition;
        }
    }
}