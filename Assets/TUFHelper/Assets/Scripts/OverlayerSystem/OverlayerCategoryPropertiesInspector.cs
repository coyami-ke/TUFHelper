using System;
using System.Reflection;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class OverlayerCategoryPropertiesInspector : MonoBehaviour
{
    public Transform propertiesContainerParent;
    public GameObject togglePrefab;
    public GameObject sliderPrefab;

    public void GenerateInspectorUI(object target)
    {
        foreach (Transform child in propertiesContainerParent)
        {
            Destroy(child.gameObject);
        }

        if (target == null) return;

        PropertyInfo[] properties = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);


        float y = 0f;
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

                controlScript.BindProperty(visualLabel, prop.GetValue(target), (newValue) => {
                    prop.SetValue(target, newValue);
                });

                RectTransform rect = controlObj.GetComponent<RectTransform>();
                rect.DOAnchorPosY(-y, 0.5f).SetEase(Ease.OutExpo);

                y += rect.sizeDelta.y;
            }
        }
    }
}