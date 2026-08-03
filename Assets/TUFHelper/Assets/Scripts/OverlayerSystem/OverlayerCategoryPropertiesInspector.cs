using System;
using System.Reflection;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class OverlayerCategoryPropertiesInspector : MonoBehaviour
{
    public Transform propertiesContainerParent;
    public GameObject togglePrefab;
    public GameObject floatPrefab;
    public GameObject vector2Prefab;
    public GameObject stringPrefab;
    public GameObject anchorPrefab;
    public GameObject colorRampPrefab;

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

            GameObject controlObj = null;

            if (propType == typeof(bool))
            {
                controlObj = Instantiate(togglePrefab, propertiesContainerParent, false);
                var controlScript = controlObj.GetComponent<OverlayerTogglePropertyControl>();

                controlScript.BindProperty(target, prop.Name, visualLabel, prop.GetValue(target), (newValue) =>
                {
                    prop.SetValue(target, newValue);
                });
            }
            else if (propType == typeof(float))
            {
                controlObj = Instantiate(floatPrefab, propertiesContainerParent, false);
                var controlScript = controlObj.GetComponent<OverlayerFloatPropertyControl>();

                var rangeAttr = prop.GetCustomAttribute<SettingsRangeAttribute>();
                if (rangeAttr != null) controlScript.SetLimitations(rangeAttr.MinValue, rangeAttr.MaxValue);

                controlScript.BindProperty(target, prop.Name, visualLabel, prop.GetValue(target), (newValue) =>
                {
                    prop.SetValue(target, newValue);
                });
            }
            else if (propType == typeof(System.Numerics.Vector2))
            {
                controlObj = Instantiate(vector2Prefab, propertiesContainerParent, false);
                var controlScript = controlObj.GetComponent<OverlayerVector2PropertyControl>();

                controlScript.BindProperty(target, prop.Name, visualLabel, prop.GetValue(target), (newValue) =>
                {
                    prop.SetValue(target, newValue);
                });
            }
            else if (propType == typeof(string))
            {
                controlObj = Instantiate(stringPrefab, propertiesContainerParent, false);
                var controlScript = controlObj.GetComponent<OverlayerStringPropertyControl>();

                controlScript.BindProperty(target, prop.Name, visualLabel, prop.GetValue(target), (newValue) =>
                {
                    prop.SetValue(target, newValue);
                });
            }
            else if (propType == typeof(Anchor))
            {
                controlObj = Instantiate(anchorPrefab, propertiesContainerParent, false);
                var controlScript = controlObj.GetComponent<OverlayerAnchorPropertyControl>();

                controlScript.BindProperty(target, prop.Name, visualLabel, prop.GetValue(target), (newValue) =>
                {
                    prop.SetValue(target, newValue);
                });
            }
            else if (propType == typeof(ColorRamp))
            {
                controlObj = Instantiate(colorRampPrefab, propertiesContainerParent, false);
                var controlScript = controlObj.GetComponent<ColorRampEditor>();

                controlScript.BindProperty(target, prop.Name, visualLabel, prop.GetValue(target), (newValue) =>
                {
                    prop.SetValue(target, newValue);
                });
            }

            if (controlObj != null)
            {
                RectTransform rect = controlObj.GetComponent<RectTransform>();

                rect.DOAnchorPosY(-y, 0.5f).SetEase(Ease.OutExpo);

                y += rect.sizeDelta.y;
            }
        }

        propertiesContainerParent.GetComponent<RectTransform>().sizeDelta = new(0, y + 110);
    }
}