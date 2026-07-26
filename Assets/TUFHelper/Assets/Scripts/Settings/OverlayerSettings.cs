using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using TMPro;
using TUFHelper;
using UnityEngine;
using UnityEngine.UI;

public class OverlayerSettings : MonoBehaviour
{
    private Dictionary<string, string> _prefabRegistry = new();

    public GameObject elementInListPrefab, categoryPrefab;
    public Transform canvasTransform, listTransform, categoriesParentTransform, categoryPropertiesTransform, categoryPropertiesContentTransform;

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
                element.IsInSettings = true;
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

            i++;
        }
    }

    private void ElementInList_Clicked(object sender, IngameElementInListScript.IngameElementInListSelectedEventArgs e)
    {
        MonoBehaviour obj = (MonoBehaviour)sender;
        IngameElementInListScript script = obj.GetComponent<IngameElementInListScript>();

        if (categoryPropertiesContentTransform != null)
        {
            for (int j = categoryPropertiesContentTransform.childCount - 1; j >= 0; j--)
            {
                Transform child = categoryPropertiesContentTransform.GetChild(j);
                Destroy(child.gameObject);
            }
        }

        if (categoriesParentTransform != null)
        {
            for (int i = categoriesParentTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = categoriesParentTransform.GetChild(i);
                Destroy(child.gameObject);
            }
        }

        AddCategories(script.Element.Model);

        foreach (var basicElement in Elements)
        {
            basicElement.IsSelected = false;
        }

        BasicIngameElement element = Elements.FirstOrDefault(b => b.NameInSettings == script.text.text);
        if (element != null)
        {
            element.IsSelected = true;
        }
    }
    private void AddCategories(IngameElementModel model)
    {
        

        GameObject spawnedTransformCategory = Instantiate(categoryPrefab);
        RectTransform categoryTransformRect = spawnedTransformCategory.GetComponent<RectTransform>();
        categoryTransformRect.SetParent(categoriesParentTransform, false);

        var transformScript = spawnedTransformCategory.GetComponent<IngameElementPropertiesCategoryScript>();
        if (transformScript != null)
        {
            transformScript.SetModelTarget("Transform", model, categoryPropertiesTransform);
        }

        categoryTransformRect.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutExpo);

        int i = 0;
        foreach (var category in model.Categories)
        {
            GameObject spawnedCategory = Instantiate(categoryPrefab);
            RectTransform categoryRect = spawnedCategory.GetComponent<RectTransform>();
            categoryRect.SetParent(categoriesParentTransform, false);

            var categoryScript = spawnedCategory.GetComponent<IngameElementPropertiesCategoryScript>();
            if (categoryScript != null)
            {
                categoryScript.SetCategory(category.Value, categoryPropertiesTransform);
            }

            categoryRect.DOAnchorPosX((i + 1) * 202.5f, 0.5f).SetEase(Ease.OutExpo);
            i++;
        }
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