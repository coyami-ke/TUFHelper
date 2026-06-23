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
                return _modCanvasTransform;
            }

            Transform existing = mainCanvas.Find("TUFHelper_CustomSubCanvas");
            if (existing != null)
            {
                _modCanvasTransform = existing;
                _modCanvasTransform.gameObject.SetActive(true);
                return _modCanvasTransform;
            }

            GameObject subCanvasObj = new GameObject("TUFHelper_CustomSubCanvas", typeof(RectTransform));
            subCanvasObj.transform.SetParent(mainCanvas, false);

            RectTransform rect = subCanvasObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one; 

            Canvas subCanvas = subCanvasObj.AddComponent<Canvas>();

            subCanvas.overrideSorting = true;
            subCanvas.sortingOrder = 100;

            subCanvasObj.AddComponent<GraphicRaycaster>();

            _modCanvasTransform = subCanvasObj.transform;
            return _modCanvasTransform;
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