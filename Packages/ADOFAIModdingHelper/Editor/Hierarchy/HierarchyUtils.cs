using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ADOFAIModdingHelper.Hierarchy.HierarchyReflections;

namespace ADOFAIModdingHelper.Hierarchy
{
    internal static class HierarchyUtils
    {
        public static EditorWindow[] GetAllSceneHierarchyWindows()
        {
            var result = SceneHierarchyWindowGetAllSceneHierarchyWindowsMethod.Invoke(null, null); 
            if (result is not IEnumerable enumerable) return Array.Empty<EditorWindow>(); 
            return enumerable.Cast<object>().OfType<EditorWindow>().ToArray();
        }

        public static EntityId ToEntityId(this SceneHandle handle)
        {
            var result = SceneHandleToEntityIdMethod.Invoke(handle, null);

            if (result == null)
            {
                Debug.LogError("SceneHandleToEntityId returned null!");
                return EntityId.None;
            }
            return (EntityId)result;
        }

        public static string GetWindowGUID(EditorWindow window)
        {
            if (window == null)
                return null;

            return SceneHierarchyWindowWindowGUIDProperty
                .GetValue(window) as string;
        }

        public static EntityId[] GetExpandedIDs(EditorWindow window)
        {
            if (window == null)
                return null;

            var result =
                SceneHierarchyWindowGetExpandedIDsMethod.Invoke(
                    window,
                    null);

            return result as EntityId[];
        }

        public static EntityId[] GetRows(EditorWindow window)
        {
            if (window == null)
                return null;

            var sceneHierarchy =
                SceneHierarchyWindowSceneHierarchyProperty
                    .GetValue(window);

            if (sceneHierarchy == null)
                return null;

            var treeView =
                SceneHierarchyTreeViewProperty
                    .GetValue(sceneHierarchy);

            if (treeView == null)
                return null;

            var data =
                TreeViewDataProperty
                    .GetValue(treeView);

            var result =
                TreeViewDataGetRowsMethod.Invoke(
                    data,
                    null);

            var EntityList = new List<EntityId>();

            foreach (var item in result as List<TreeViewItem<EntityId>>)
            {
                EntityList.Add(item.id);
            }

            return EntityList.ToArray();
        }

        public static void SetExpandedIDs(
            EditorWindow window,
            EntityId[] entityIds)
        {
            if (window == null || entityIds == null)
                return;

            var sceneHierarchy =
                SceneHierarchyWindowSceneHierarchyProperty
                    .GetValue(window);

            if (sceneHierarchy == null)
                return;

            var treeView =
                SceneHierarchyTreeViewProperty
                    .GetValue(sceneHierarchy);

            if (treeView == null)
                return;

            var data =
                TreeViewDataProperty
                    .GetValue(treeView);

            if (data == null)
                return;

            TreeViewDataSetExpandedIDsMethod.Invoke(
                data,
                new object[]
                {
                    entityIds
                });
        }

        public static EntityId[] AddEntityIdNoDupe(
            this EntityId[] existing,
            EntityId entityId)
        {
            if (entityId == EntityId.None)
                return existing;

            if (existing == null)
                return new[] { entityId };

            if (existing.Contains(entityId))
                return existing;

            var result = new EntityId[existing.Length + 1];

            existing.CopyTo(result, 0);
            result[^1] = entityId;

            return result;
        }

        public static EntityId[] AddEntityIdsNoDupe(
            this EntityId[] existing,
            IEnumerable<EntityId> entityIds)
        {
            if (entityIds == null)
                return existing;

            var result = new HashSet<EntityId>(
                existing ?? Array.Empty<EntityId>());

            foreach (var entityId in entityIds)
            {
                if (entityId != EntityId.None)
                    result.Add(entityId);
            }

            return result.ToArray();
        }

        public static EntityId[] RemoveEntityId(
            this EntityId[] existing,
            EntityId entityId)
        {
            if (existing == null ||
                entityId == EntityId.None)
            {
                return existing;
            }

            if (!existing.Contains(entityId))
                return existing;

            return existing
                .Where(x => x != entityId)
                .ToArray();
        }

        public static EntityId[] RemoveEntityIds(
            this EntityId[] existing,
            IEnumerable<EntityId> entityIds)
        {
            if (existing == null ||
                entityIds == null)
            {
                return existing;
            }

            var idsToRemove =
                new HashSet<EntityId>(entityIds);

            return existing
                .Where(x => !idsToRemove.Contains(x))
                .ToArray();
        }
    }
}