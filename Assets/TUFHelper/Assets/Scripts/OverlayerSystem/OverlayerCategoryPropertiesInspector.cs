using System;
using System.Reflection;
using DG.Tweening;
using UnityEngine;

public class OverlayerCategoryPropertiesInspector : MonoBehaviour
{
    [Header("UI Spawn Grid Layout Parent")]
    public Transform propertiesContainerParent;

    [Header("Input Control Element Prefabs")]
    public GameObject togglePrefab;
    public GameObject sliderPrefab;

    public void GenerateInspectorUI(IngameElementSettingsCategory targetCategory)
    {
        foreach (Transform child in propertiesContainerParent)
        {
            Destroy(child.gameObject);
        }

        if (targetCategory == null) return;

        PropertyInfo[] properties = targetCategory.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        float y = 0;
        foreach (PropertyInfo prop in properties)
        {
            var settingsAttr = prop.GetCustomAttribute<ShowInOverlayerSettingsAttribute>();
            if (settingsAttr == null) continue;

            string visualLabel = settingsAttr.LabelName;
            Type propType = prop.PropertyType;

            if (propType == typeof(bool))
            {
                GameObject controlObj = Instantiate(togglePrefab, propertiesContainerParent, false);
                var controlScript = controlObj.GetComponent<OverlayerTogglePropertyControl>();

                controlScript.BindProperty(visualLabel, prop.GetValue(targetCategory), (newValue) => {
                    prop.SetValue(targetCategory, newValue);
                });

                controlObj.GetComponent<RectTransform>().DOAnchorPosY(-y, 0.5f).SetEase(Ease.OutExpo);
                y += controlObj.GetComponent<RectTransform>().sizeDelta.y;
            }
        }
    }
}