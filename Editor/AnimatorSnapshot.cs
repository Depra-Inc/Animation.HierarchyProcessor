// Copyright © 2022 Nikolay Melnikov. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using UnityEngine;

namespace Depra.AnimationHierarchyProcessor.Editor
{
    internal sealed class AnimatorSnapshot
    {
        public Animator Animator;
        public Dictionary<Transform, AnimatorSnapshotTransform> TransformSnapshots;

        public void InitSnapshotTransform(Transform root)
        {
            TransformSnapshots = new Dictionary<Transform, AnimatorSnapshotTransform>();
            var count = root.childCount;
            for (var i = 0; i < count; i++)
            {
                var child = root.GetChild(i);
                CreateAnimatorSnapshotTransformRecursive(child, child.name);
            }
        }

        private void CreateAnimatorSnapshotTransformRecursive(Transform transform, string path)
        {
            var snapshotTransform = new AnimatorSnapshotTransform(transform.name, path);
            TransformSnapshots.Add(transform, snapshotTransform);
            var count = transform.childCount;
            for (var i = 0; i < count; i++)
            {
                var child = transform.GetChild(i);
                CreateAnimatorSnapshotTransformRecursive(child, path + "/" + child.name);
            }
        }

        public sealed class AnimatorSnapshotTransform
        {
            public readonly string ChildName;
            public readonly string Path;

            public AnimatorSnapshotTransform(string childName, string path)
            {
                ChildName = childName;
                Path = path;
            }
        }
    }
}