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
            Transform mainCanvas = GetMainCanvasTransform();
            if (mainCanvas == null) return;

            foreach (string elementId in _prefabRegistry.Keys)
            {
                GetOrCreateElement(elementId, mainCanvas);
            }
        }

        private void OnReturnToEditor(object sender, ScnGameTransferToEditorEventArgs e)
        {
            foreach (var element in _activeElements.Values)
            {
                if (element != null) element.gameObject.SetActive(false);
            }
        }

        private Transform GetMainCanvasTransform()
        {
            if (scrUIController.instance != null && scrUIController.instance.canvas != null)
            {
                return scrUIController.instance.canvas.transform;
            }

            return GameObject.Find("Canvas")?.transform;
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
                existing.ScaleModifier = 2.125f;
                existing.ApplyScale();
                existing.gameObject.SetActive(true);
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

            script.ScaleModifier = 2.125f;
            script.ApplyScale();

            _activeElements[id] = script;
            return script;
        }
    }
}