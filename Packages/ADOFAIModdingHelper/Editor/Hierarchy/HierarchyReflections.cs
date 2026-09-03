using System;
using System.Reflection;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace ADOFAIModdingHelper.Hierarchy
{
    [InitializeOnLoad]
    internal static class HierarchyReflections
    {
        public static readonly Type SceneHierarchyWindowType;

        public static readonly MethodInfo SceneHierarchyWindowGetAllSceneHierarchyWindowsMethod;
        public static readonly MethodInfo SceneHierarchyWindowGetExpandedIDsMethod;
        public static readonly PropertyInfo SceneHierarchyWindowWindowGUIDProperty;
        public static readonly PropertyInfo SceneHierarchyWindowSceneHierarchyProperty;

        public static readonly Type SceneHierarchyType;
        public static readonly PropertyInfo SceneHierarchyTreeViewProperty;

        public static readonly Type TreeViewType;
        public static readonly PropertyInfo TreeViewDataProperty;

        public static readonly Type TreeViewDataType;
        public static readonly MethodInfo TreeViewDataSetExpandedIDsMethod;
        public static readonly MethodInfo TreeViewDataGetRowsMethod;

        public static readonly MethodInfo SceneHandleToEntityIdMethod;

        public static bool IsReflectionReady()
        {
            return SceneHierarchyWindowType != null
                && SceneHierarchyWindowGetAllSceneHierarchyWindowsMethod != null
                && SceneHierarchyWindowGetExpandedIDsMethod != null
                && SceneHierarchyWindowWindowGUIDProperty != null
                && SceneHierarchyWindowSceneHierarchyProperty != null
                && SceneHierarchyType != null
                && SceneHierarchyTreeViewProperty != null
                && TreeViewType != null
                && TreeViewDataProperty != null
                && TreeViewDataType != null
                && TreeViewDataSetExpandedIDsMethod != null
                && SceneHandleToEntityIdMethod != null;
        }

        static HierarchyReflections()
        {
            if (IsReflectionReady()) return;

            SceneHierarchyWindowType =
                typeof(EditorWindow)
                    .Assembly
                    .GetType("UnityEditor.SceneHierarchyWindow");

            SceneHierarchyWindowGetAllSceneHierarchyWindowsMethod =
                SceneHierarchyWindowType.GetMethod(
                    "GetAllSceneHierarchyWindows",
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            SceneHierarchyWindowGetExpandedIDsMethod =
                SceneHierarchyWindowType.GetMethod(
                    "GetExpandedIDs",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            SceneHierarchyWindowWindowGUIDProperty =
                SceneHierarchyWindowType.GetProperty(
                    "windowGUID",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            SceneHierarchyWindowSceneHierarchyProperty =
                SceneHierarchyWindowType.GetProperty(
                    "sceneHierarchy",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            SceneHierarchyType =
                SceneHierarchyWindowSceneHierarchyProperty.PropertyType;

            SceneHierarchyTreeViewProperty =
                SceneHierarchyType.GetProperty(
                    "treeView",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            TreeViewType =
                SceneHierarchyTreeViewProperty.PropertyType;

            TreeViewDataProperty =
                TreeViewType.GetProperty(
                    "data",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            TreeViewDataType =
                TreeViewDataProperty.PropertyType;

            TreeViewDataSetExpandedIDsMethod =
                TreeViewDataType.GetMethod(
                    "SetExpandedIDs",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            TreeViewDataGetRowsMethod =
                TreeViewDataType.GetMethod(
                    "GetRows",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            SceneHandleToEntityIdMethod =
                typeof(SceneHandle).GetMethod(
                    "ToEntityId",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);
        }
    }
}