using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class OverlayerSettings : MonoBehaviour
{
    private Dictionary<string, string> _prefabRegistry = new();

    public GameObject elementInListPrefab;
    public Transform canvasTransform, listTransform, categoriesParentTransform;

    public List<IngameElementPropertiesCategoryScript> CategoryScripts {  get; private set; }
    
    public List<BasicIngameElement> Elements { get; private set; } = new();
    public void Start()
    {
        SetRegistryIngamePrefabs();

        foreach (var path in _prefabRegistry.Values)
        {
            GameObject prefab = Main.assets.LoadAsset<GameObject>(path);
            if (prefab == null) continue;

            GameObject instance = GameObject.Instantiate(prefab, canvasTransform, false);
            BundleFontFixer.FixFontsIn(instance);

            var element = instance.GetComponentInChildren<BasicIngameElement>();
            if (element != null)
            {
                if (Main.Setting.IngameElementsSettings.ContainsKey(element.ID))
                {
                }
                else
                {
                    var newModel = new IngameElementModel();
                    Main.Setting.IngameElementsSettings[element.ID] = newModel;
                }

                //element.gameObject.SetActive(true);
                element.UpdateVisibility();
                element.OnSettingsOpened();
                element.CreateSettingsHandles();

                Elements.Add(element);
            }
        }

        int i = 0;
        foreach (var element in Elements)
        {
            GameObject instance = GameObject.Instantiate(elementInListPrefab, listTransform, false);
            var script = instance.GetComponent<IngameElementInListScript>();
            if (script != null)
            {
                script.SetElementInfo(element);
                script.Selected += ElementInList_Clicked;
            }

            instance.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -62.5f * i);

            Main.Logger.Log(script.text.text);

            i++;
        }
    }

    private void ElementInList_Clicked(object sender, IngameElementInListScript.IngameElementInListSelectedEventArgs e)
    {
        MonoBehaviour obj = (MonoBehaviour)sender;
        IngameElementInListScript script = obj.GetComponent<IngameElementInListScript>();
        Main.Logger.Log("Selected: " + script.Element.NameInSettings);
    }

    public void SetRegistryIngamePrefabs()
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
}