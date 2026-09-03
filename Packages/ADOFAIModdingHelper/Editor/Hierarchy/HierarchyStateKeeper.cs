using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ADOFAIModdingHelper.Hierarchy
{
    internal static class HierarchyStateKeeper
    {
        private static readonly List<WindowState> s_savedStates = new();
        private static GlobalObjectId[] SelectedObjects;

        public static void SaveState()
        {
            s_savedStates.Clear();

            var currentSelectionEntityIds = Selection.entityIds;

            var windows =
                HierarchyUtils.GetAllSceneHierarchyWindows();

            // Scene EntityId -> Scene Path.
            //
            // Scene EntityIds are temporary, so this dictionary exists
            // only for the duration of this save operation.
            var scenePathsByEntityId =
                GetLoadedScenePathsByEntityId();

            var selectedObjects =
                new List<GlobalObjectId>();

            foreach (var window in windows)
            {
                if (window == null)
                    continue;

                var windowGuid =
                    HierarchyUtils.GetWindowGUID(window);

                if (string.IsNullOrEmpty(windowGuid))
                    continue;

                var expandedEntityIds =
                    HierarchyUtils.GetExpandedIDs(window);

                if (expandedEntityIds == null)
                    continue;

                var rowsEntityIds =
                    HierarchyUtils.GetRows(window);

                if (rowsEntityIds == null)
                    continue;

                var expandedObjects =
                    new List<GlobalObjectId>();

                var expandedScenePaths =
                    new List<string>();

                foreach (var entityId in expandedEntityIds)
                {
                    var obj = EditorUtility.EntityIdToObject(entityId);

                    if (obj != null)
                    {
                        var globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(obj);

                        if (!globalObjectId.Equals(default))
                        {
                            expandedObjects.Add(globalObjectId);
                        }

                        continue;
                    }

                    // Scene headers don't resolve through
                    // EntityIdToObject(), so resolve their temporary
                    // EntityId through our scene lookup.
                    if (scenePathsByEntityId.TryGetValue(
                        entityId,
                        out var scenePath))
                    {
                        expandedScenePaths.Add(scenePath);
                    }
                }

                foreach (var entityId in rowsEntityIds)
                {
                    if (!currentSelectionEntityIds.Contains(entityId)) continue;

                    var obj = EditorUtility.EntityIdToObject(entityId);
                    if (obj != null)
                    {
                        var globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(obj);

                        if (!globalObjectId.Equals(default))
                        {
                            Debug.Log("AddingglobalObjectId ");
                            selectedObjects.Add(globalObjectId);
                        }
                    }
                }

                s_savedStates.Add(
                    new WindowState
                    {
                        WindowGUID = windowGuid,
                        ExpandedObjects = expandedObjects.ToArray(),
                        ExpandedScenePaths = expandedScenePaths.ToArray(),
                    });
            }

            SelectedObjects = selectedObjects.ToArray();

            Debug.Log(
                $"[HierarchyStateKeeper] Saved " +
                $"{s_savedStates.Count} Hierarchy window state(s).");
        }

        public static void RestoreState()
        {
            if (s_savedStates.Count == 0)
                return;

            var windows =
                HierarchyUtils.GetAllSceneHierarchyWindows();

            var statesByGuid =
                s_savedStates.ToDictionary(
                    x => x.WindowGUID);

            foreach (var window in windows)
            {
                if (window == null)
                    continue;

                var windowGuid =
                    HierarchyUtils.GetWindowGUID(window);

                if (string.IsNullOrEmpty(windowGuid))
                    continue;

                if (!statesByGuid.TryGetValue(
                    windowGuid,
                    out var savedState))
                {
                    continue;
                }

                RestoreWindowState(
                    window,
                    savedState);

               window.Repaint();
            }

            if (SelectedObjects != null && SelectedObjects.Length != 0)
            {
                var entityIds =
                    new EntityId[SelectedObjects.Length];

                GlobalObjectId
                    .GlobalObjectIdentifiersToEntityIdsSlow(
                        SelectedObjects,
                        entityIds);

                Selection.entityIds =
                    Selection.entityIds.AddEntityIdsNoDupe(entityIds);
            }

            Debug.Log(
                $"[HierarchyStateKeeper] Restored " +
                $"{s_savedStates.Count} saved Hierarchy window state(s).");
        }

        private static Dictionary<EntityId, string> GetLoadedScenePathsByEntityId()
        {
            var result =
                new Dictionary<EntityId, string>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene =
                    SceneManager.GetSceneAt(i);

                if (!scene.IsValid())
                    continue;

                var entityId =
                    scene.handle.ToEntityId();

                if (entityId == EntityId.None)
                    continue;

                result[entityId] =
                    scene.path;
            }

            return result;
        }

        private static void RestoreWindowState(EditorWindow window, WindowState state)
        {
            var expandedEntityIds =
                new List<EntityId>();

            // Resolve scenes.
            if (state.ExpandedScenePaths != null)
            {
                foreach (var scenePath in state.ExpandedScenePaths)
                {
                    if (string.IsNullOrEmpty(scenePath))
                        continue;

                    var scene =
                        SceneManager.GetSceneByPath(scenePath);

                    if (!scene.IsValid())
                        continue;

                    var entityId =
                        scene.handle.ToEntityId();

                    if (entityId == EntityId.None)
                        continue;

                    expandedEntityIds.Add(entityId);
                }
            }

            // Resolve GameObjects.
            if (state.ExpandedObjects != null &&
                state.ExpandedObjects.Length > 0)
            {
                var objectEntityIds =
                    new EntityId[state.ExpandedObjects.Length];

                GlobalObjectId
                    .GlobalObjectIdentifiersToEntityIdsSlow(
                        state.ExpandedObjects,
                        objectEntityIds);

                foreach (var entityId in objectEntityIds)
                {
                    if (entityId == EntityId.None)
                        continue;

                    expandedEntityIds.Add(entityId);
                }
            }

            HierarchyUtils.SetExpandedIDs(
                window,
                expandedEntityIds.ToArray());
        }
    }
}