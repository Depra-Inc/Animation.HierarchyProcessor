// Copyright © 2022 Nikolay Melnikov. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEditor;

namespace Depra.AnimationHierarchyProcessor.Editor
{
    [InitializeOnLoad]
    public static class HierarchyMonitor
    {
        private static readonly AnimatorSnapshotManager ANIMATOR_SNAPSHOT_MANAGER;

        static HierarchyMonitor()
        {
            ANIMATOR_SNAPSHOT_MANAGER = new AnimatorSnapshotManager();
            
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private static void OnHierarchyChanged()
        {
            ANIMATOR_SNAPSHOT_MANAGER.ProcessAnimators();
        }
    }
}