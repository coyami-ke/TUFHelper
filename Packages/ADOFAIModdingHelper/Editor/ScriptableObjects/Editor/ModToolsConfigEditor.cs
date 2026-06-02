using ADOFAIModdingHelper.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ADOFAIModdingHelper.ScriptableObjects.Editor
{
    [CustomEditor(typeof(ModToolsConfig))]
    public class ProjectToolsConfigEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualElement List;
        [SerializeField] private VisualElement ListItem;
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty("AssemblyDefinitions")));

            root.Add(new Divider());

            List<string> projectDlls = AssetDatabase.FindAssets("t:DefaultAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".dll"))
                .Select(Path.GetFileName)
                .ToList();
            var precompProp = serializedObject.FindProperty("PrecompAssemblies");
            root.Add(CreateListView(precompProp, "Precompiled Assemblies", "DllDropdown", projectDlls, "PrecompAssemblies", "(No DLLs Found)"));

            root.Add(new Divider() { });

            var assetBundlesProp = serializedObject.FindProperty("AssetBundles");
            List<string> projectBundles = AssetDatabase.GetAllAssetBundleNames().ToList();
            root.Add(CreateListView(assetBundlesProp, "Bundles to Build", "BundleDropdown", projectBundles, "AssetBundles", "(No Bundles Found)"));

            root.Add(new Divider() {  });

            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);

            while (iterator.NextVisible(false))
            {
                if (IsHandledManually(iterator.name) || iterator.name == "m_Script")
                    continue;

                root.Add(new PropertyField(iterator));
            }

            return root;
        }

        private bool IsHandledManually(string propertyName)
        {
            return propertyName == "AssemblyDefinitions" ||
                   propertyName == "AssetBundles" ||
                   propertyName == "PrecompAssemblies";
        }
        private static ListView CreateListView(
            SerializedProperty prop,
            string header,
            string dropdownName,
            List<string> choices,
            string bindingPath,
            string emptyText = "No Items Found")
        {
            ListView listView = StyledListView();

            listView.bindingPath = bindingPath;
            listView.headerTitle = header;
            listView.showFoldoutHeader = true;
            listView.showAddRemoveFooter = true;
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;
            listView.selectionType = SelectionType.Single;
            listView.style.maxHeight = 512;

            listView.AddToClassList("unity-list-view--with-footer");
            listView.AddToClassList("unity-list-view--reorderable");

            var originalChoices = new List<string>(choices);
            bool hasChoices = originalChoices.Count > 0;

            List<string> BuildChoicesFor(int index)
            {
                if (!hasChoices)
                    return new List<string> { emptyText };

                var currentValue = prop.GetArrayElementAtIndex(index).stringValue;

                var usedElsewhere = new HashSet<string>();
                for (int j = 0; j < prop.arraySize; j++)
                {
                    if (j == index) continue;
                    var v = prop.GetArrayElementAtIndex(j).stringValue;
                    if (!string.IsNullOrEmpty(v)) usedElsewhere.Add(v);
                }

                var available = new List<string> { "None" };
                foreach (var choice in originalChoices)
                {
                    if (!usedElsewhere.Contains(choice) || choice == currentValue)
                        available.Add(choice);
                }
                return available;
            }

            listView.makeItem = () =>
            {
                var container = new VisualElement { style = { flexDirection = FlexDirection.Row } };

                var label = new Label("Element")
                {
                    name = "IndexLabel",
                    style = { minWidth = 95, unityTextAlign = TextAnchor.MiddleLeft }
                };

                var dropdown = new DropdownField
                {
                    name = dropdownName,
                    style = { flexGrow = 1, paddingBottom = 2, paddingTop = 2, minHeight = 18 }
                };

                var textElement = dropdown.Q<TextElement>();
                if (textElement != null)
                {
                    textElement.style.textOverflow = TextOverflow.Ellipsis;
                    textElement.style.overflow = Overflow.Hidden;
                    textElement.style.whiteSpace = WhiteSpace.NoWrap;
                }

                dropdown.RegisterValueChangedCallback(evt =>
                {
                    if (container.userData is not int idx) return;
                    if (idx >= prop.arraySize) return;

                    var property = prop.GetArrayElementAtIndex(idx);

                    if (string.IsNullOrEmpty(evt.newValue) || evt.newValue == "None" || evt.newValue == emptyText)
                    {
                        property.stringValue = string.Empty;
                        dropdown.SetValueWithoutNotify("None");
                    }
                    else
                    {
                        property.stringValue = evt.newValue;
                    }

                    property.serializedObject.ApplyModifiedProperties();
                    listView.schedule.Execute(listView.Rebuild);
                });

                container.Add(label);
                container.Add(dropdown);
                return container;
            };

            listView.bindItem = (element, i) =>
            {
                element.userData = i;

                element.Q<Label>("IndexLabel").text = $"Element {i}";

                var dropdown = element.Q<DropdownField>(dropdownName);
                var property = prop.GetArrayElementAtIndex(i);

                dropdown.choices = BuildChoicesFor(i);

                var display = string.IsNullOrEmpty(property.stringValue) ? "None" : property.stringValue;
                dropdown.SetValueWithoutNotify(display);
            };

            listView.unbindItem = (element, _) =>
            {
                element.userData = null;
            };

            listView.itemsAdded += indices =>
            {
                foreach (var index in indices)
                    prop.GetArrayElementAtIndex(index).stringValue = string.Empty;

                prop.serializedObject.ApplyModifiedProperties();
                listView.schedule.Execute(listView.Rebuild);
            };

            return listView;
        }

        private static ListView StyledListView()
        {
            var bundleListView = new ListView();

            bundleListView.style.borderLeftWidth = 0;
            bundleListView.style.borderRightWidth = 0;
            bundleListView.style.borderTopWidth = 0;
            bundleListView.style.borderBottomWidth = 0;

            var scrollView = bundleListView.Q<ScrollView>();
            if (scrollView != null)
            {
                scrollView.style.borderLeftWidth = 1;
                scrollView.style.borderRightWidth = 1;
                scrollView.style.borderTopWidth = 1;
                scrollView.style.borderBottomWidth = 1;
                scrollView.style.borderBottomColor = new Color(0.12f, 0.12f, 0.12f);
                scrollView.style.borderTopColor = new Color(0.12f, 0.12f, 0.12f);
                scrollView.style.borderLeftColor = new Color(0.12f, 0.12f, 0.12f);
                scrollView.style.borderRightColor = new Color(0.12f, 0.12f, 0.12f);
                scrollView.style.borderBottomLeftRadius = 3;
                scrollView.style.borderBottomRightRadius = 3;
                scrollView.style.borderTopLeftRadius = 3;
                scrollView.style.borderTopRightRadius = 3;

                scrollView.style.marginBottom = 0;
            }
            return bundleListView;
        }
    }

    public class Divider : VisualElement
    {
        public Divider()
        {
            style.height = 1;
            style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            style.marginTop = 5;
            style.marginBottom = 5;
        }
    }
}