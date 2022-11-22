// Copyright © 2022 Nikolay Melnikov. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Depra.AnimationHierarchyProcessor.Editor
{
    internal sealed class AnimatorSnapshotManager
    {
        private readonly Dictionary<Animator, bool> _foundAnimators;
        private readonly Dictionary<Animator, AnimatorSnapshot> _animatorSnapshots;

        public void ProcessAnimators()
        {
            ClearFoundedAnimators();
            var animators = Object.FindObjectsOfType<Animator>();
            foreach (var animator in animators)
            {
                ProcessAnimator(animator);
            }

            ClearDeleted();
            var values = _animatorSnapshots.Values.ToList();
            foreach (var animatorSnapshot in values)
            {
                ProcessSnapshot(animatorSnapshot);
            }
        }

        public AnimatorSnapshotManager()
        {
            _animatorSnapshots = new Dictionary<Animator, AnimatorSnapshot>();
            _foundAnimators = new Dictionary<Animator, bool>();
        }

        private void ProcessSnapshot(AnimatorSnapshot snapshot)
        {
            var updatedSnapshot = CreateSnapshot(snapshot.Animator);
            var oldTransformSnapshots = snapshot.TransformSnapshots;
            var allTransformsUpdated = updatedSnapshot.TransformSnapshots.Keys;
            var clips = AnimationUtility.GetAnimationClips(snapshot.Animator.gameObject);

            foreach (var key in allTransformsUpdated)
            {
                if (oldTransformSnapshots.ContainsKey(key) == false)
                {
                    continue;
                }

                var newTransform = updatedSnapshot.TransformSnapshots[key];
                var oldTransform = snapshot.TransformSnapshots[key];
                if (newTransform.ChildName != oldTransform.ChildName)
                {
                    foreach (var clip in clips)
                    {
                        ProcessChange(clip, oldTransform, newTransform);
                    }
                }

                if (newTransform.Path == oldTransform.Path)
                {
                    continue;
                }

                foreach (var clip in clips)
                {
                    ProcessChange(clip, oldTransform, newTransform);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorApplication.RepaintAnimationWindow();

            _animatorSnapshots[snapshot.Animator] = updatedSnapshot;
        }

        private static void ProcessChange(AnimationClip clip, AnimatorSnapshot.AnimatorSnapshotTransform old,
            AnimatorSnapshot.AnimatorSnapshotTransform updated)
        {
            var objectCurveBinding = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            var curveDataBinding = AnimationUtility.GetCurveBindings(clip);

            for (var i = 0; i < objectCurveBinding.Length; i++)
            {
                var binding = objectCurveBinding[i];
                if (binding.path != old.Path)
                {
                    continue;
                }

                var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
                binding.path = updated.Path;
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            }

            for (var i = 0; i < curveDataBinding.Length; i++)
            {
                var binding = curveDataBinding[i];
                if (binding.path != old.Path)
                {
                    continue;
                }

                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                AnimationUtility.SetEditorCurve(clip, binding, null);
                binding.path = updated.Path;
                AnimationUtility.SetEditorCurve(clip, binding, curve);
            }
        }

        private void ProcessAnimator(Animator animator)
        {
            if (_foundAnimators.ContainsKey(animator))
            {
                _foundAnimators[animator] = true;
            }

            if (_animatorSnapshots.ContainsKey(animator) == false)
            {
                _animatorSnapshots[animator] = CreateSnapshot(animator);
            }
        }

        private static AnimatorSnapshot CreateSnapshot(Animator animator)
        {
            var snapshot = new AnimatorSnapshot {Animator = animator};
            snapshot.InitSnapshotTransform(animator.transform);

            return snapshot;
        }

        private void ClearDeleted()
        {
            foreach (var (key, _) in _foundAnimators.Where(kvp => !kvp.Value).ToList())
            {
                _foundAnimators.Remove(key);
                _animatorSnapshots.Remove(key);
            }
        }

        private void ClearFoundedAnimators()
        {
            foreach (var key in _foundAnimators.Keys)
            {
                _foundAnimators[key] = false;
            }
        }
    }
}