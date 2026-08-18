using System;
using System.Collections.Generic;
using System.Reflection;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace TUFHelper
{
    public class IngameUIManager
    {
        private static IngameUIManager _instance;
        public static IngameUIManager Instance => _instance ??= new IngameUIManager();

        private readonly Dictionary<string, BasicIngameElement> _activeElements = new();
        private readonly Dictionary<string, string> _prefabRegistry = new();

        // Reference to our dedicated container
        private Transform _modCanvasTransform;

        public void Initialize()
        {
            ADOFAIGameplayHandler.Editor_PlayButtonPressed += OnPlay;
            ADOFAIGameplayHandler.Editor_ScnGameTransferToEditor += OnReturnToEditor;
            DiscoverElementsViaReflection();
        }

        private void DiscoverElementsViaReflection()
        {
            _prefabRegistry.Clear();
            Type baseType = typeof(BasicIngameElement);
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (Type type in assembly.GetTypes())
            {
                if (baseType.IsAssignableFrom(type) && !type.IsAbstract)
                {
                    var attribute = type.GetCustomAttribute<RegisterIngameElementAttribute>();
                    if (attribute != null)
                    {
                        _prefabRegistry[attribute.ID] = attribute.PrefabPath;
                    }
                }
            }
        }

        private void OnPlay(object sender, PlayButtonEventArgs e)
        {
            Transform mainCanvas = GameObject.Find("Canvas")?.transform;
            if (mainCanvas == null) return;

            Transform targetContainer = GetOrCreateModCanvas(mainCanvas);
            if (targetContainer == null) return;

            foreach (string elementId in _prefabRegistry.Keys)
            {
                GetOrCreateElement(elementId, targetContainer);
            }
        }

        private void OnReturnToEditor(object sender, ScnGameTransferToEditorEventArgs e)
        {
            if (_modCanvasTransform != null)
            {
                _modCanvasTransform.gameObject.SetActive(false);
            }

            foreach (var element in _activeElements.Values)
            {
                if (element != null) element.gameObject.SetActive(false);
            }
        }
        private Transform GetOrCreateModCanvas(Transform mainCanvas)
        {
            if (_modCanvasTransform != null)
            {
                _modCanvasTransform.gameObject.SetActive(true);
                UpdateModCanvasSize();
                return _modCanvasTransform;
            }

            Transform existing = mainCanvas.Find("TUFHelper_CustomSubCanvas");
            if (existing != null)
            {
                _modCanvasTransform = existing;
                _modCanvasTransform.gameObject.SetActive(true);
                UpdateModCanvasSize();
                return _modCanvasTransform;
            }

            GameObject subCanvasObj = new GameObject("TUFHelper_CustomSubCanvas", typeof(RectTransform));
            subCanvasObj.transform.SetParent(mainCanvas, false);

            Canvas subCanvas = subCanvasObj.AddComponent<Canvas>();
            subCanvas.overrideSorting = true;
            subCanvas.sortingOrder = 100;

            subCanvasObj.AddComponent<GraphicRaycaster>();

            _modCanvasTransform = subCanvasObj.transform;

            UpdateModCanvasSize();

            return _modCanvasTransform;
        }

        public void UpdateModCanvasSize()
        {
            if (_modCanvasTransform == null) return;

            RectTransform rect = _modCanvasTransform.GetComponent<RectTransform>();
            Canvas parentCanvas = scrUIController.instance != null ? scrUIController.instance.canvas : null;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            if (parentCanvas != null)
            {
                Vector2 canvasSize = parentCanvas.GetComponent<RectTransform>().rect.size;
                rect.sizeDelta = canvasSize / 2.125f;

                Main.Logger.Log("Canvas Size: " + rect.sizeDelta);
            }
        }

        public T GetElement<T>(string id) where T : BasicIngameElement
        {
            if (_activeElements.TryGetValue(id, out var element) && element != null)
            {
                return element as T;
            }
            return null;
        }

        private BasicIngameElement GetOrCreateElement(string id, Transform parentCanvas)
        {
            if (_activeElements.TryGetValue(id, out var existing) && existing != null)
            {
                existing.UpdateVisibility();
                return existing;
            }

            if (!_prefabRegistry.TryGetValue(id, out string path)) return null;

            GameObject prefab = Main.assets.LoadAsset<GameObject>(path);
            if (prefab == null) return null;

            GameObject instance = GameObject.Instantiate(prefab, parentCanvas, false);
            BundleFontFixer.FixFontsIn(instance);

            BasicIngameElement script = instance.GetComponentInChildren<BasicIngameElement>();
            if (script == null)
            {
                GameObject.Destroy(instance);
                return null;
            }

            _activeElements[id] = script;
            return script;
        }
    }
}