using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Animation.PGA
{
    public class PGARig : ActuationRig
    {
        [Header("Constraint Management")]
        public List<PGAConstraint> RigConstraints;

        [Header("Animation Profiles")]
        public List<AnimationProfile> profiles;

        /// <summary>
        /// Used while transitioning between poses
        /// </summary>
        public List<AnimationProfile> TransitionProfiles;

        [Header("Keyframe Poses")]
        /// <summary>
        /// ID of pose to be edited
        /// </summary>
        public int CurrentEditing_PoseID = 0;

        [SerializeField] private PGAPose DefaultPose;

        public List<PGAPose> Poses;

        private Coroutine PoseInterpolationCoroutine = null;

        public override void Solve(float delta)
        {
            for (int i = 0; i < RigConstraints.Count; i++)
            {
                if (i > 0)
                {
                    RigConstraints[i].SetDefault();
                }
                RigConstraints[i].Solve(delta);
            }
        }

        #region Pose Management


#if UNITY_EDITOR
        /// <summary>
        /// Assigns Selected Transforms to Pose data of Default Pose
        /// </summary>
        [ContextMenu("[DANGER] Assign Default Pose Data", false, 100)]
        public void SetDefaultPose()
        {
            if (Application.isPlaying)
            {
                return;
            }

            var selection = UnityEditor.Selection.objects;
            if (selection.Length > 0)
            {
                DefaultPose = new PGAPose();
            }
            foreach (var obj in selection)
            {
                Transform objTrans = null;
                if (obj is Transform)
                {
                    objTrans = obj as Transform;
                }
                if (obj is GameObject)
                {
                    objTrans = ((GameObject)obj).transform;
                }

                if (objTrans != null)
                {
                    if (!DefaultPose.Parts.Exists(p => p.PartTransform.Equals(objTrans)))
                    {
                        DefaultPose.Parts.Add(new PGAPosePart(objTrans, objTrans.localRotation));
                    }

                    /*
                    foreach (var trans in objTrans.GetComponentsInChildren<Transform>())
                    {
                        if (!DefaultPose.Parts.Exists(p => p.PartTransform.Equals(trans)))
                        {
                            DefaultPose.Parts.Add(new PGAPosePart(trans, trans.localRotation));
                        }
                    }
                    */
                }
            }
        }

        /// <summary>
        /// Assigns Selected Transforms to Pose data of Current Pose ID
        /// </summary>
        [ContextMenu("[DANGER] Assign Current ID Pose Data", false, 101)]
        public void SetPose()
        {
            SetPose(CurrentEditing_PoseID);
        }
#endif

        /// <summary>
        /// Change Pose to Current ID Pose
        /// </summary>
        [ContextMenu("Set to Current ID Pose", false, 201)]
        public void ApplyPose()
        {
            ApplyPose(CurrentEditing_PoseID);
        }

        [ContextMenu("Set to Default Pose", false, 200)]
        public void ApplyDefaultPoseEditor()
        {
            ApplyDefaultPose();
        }

        /// <summary>
        /// Change Pose to Default Pose
        /// </summary>
        
        public void ApplyDefaultPose(bool interpolate = false)
        {
            if(PoseInterpolationCoroutine != null)
            {
                return;
            }

            if (interpolate)
            {
                PoseInterpolationCoroutine = StartCoroutine(InterpolateToDefaultPose());
            }
            else
            { 
                foreach (var part in DefaultPose.Parts)
                {
                    part.PartTransform.localRotation = part.PartRotation;
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Assigns Selected Transform to Pose data of ID
        /// </summary>
        /// <param name="id">Pose ID</param>
        public void SetPose(int id = 0)
        {
            if (Application.isPlaying || Poses.Count <= id)
            {
                return;
            }
            string name = Poses[id].Name;
            Poses[id] = new PGAPose();
            Poses[id].Name = name;

            var selection = UnityEditor.Selection.objects;
            foreach (var obj in selection)
            {
                Transform objTrans = null;
                if (obj is Transform)
                {
                    objTrans = obj as Transform;
                }
                if (obj is GameObject)
                {
                    objTrans = ((GameObject)obj).transform;
                }

                if (objTrans != null)
                {
                    if (!Poses[id].Parts.Exists(p => p.PartTransform.Equals(objTrans)))
                    {
                        Poses[id].Parts.Add(new PGAPosePart(objTrans, objTrans.localRotation));
                    }

                    /*
                    foreach (var trans in objTrans.GetComponentsInChildren<Transform>())
                    {
                        if (!Poses[id].Parts.Exists(p => p.PartTransform.Equals(trans)))
                        {
                            Poses[id].Parts.Add(new PGAPosePart(trans, trans.localRotation));
                        }
                    }
                    */
                }
            }
        }
#endif


        /// <summary>
        /// Changes the pose to the expected Pose ID. 
        /// </summary>
        /// <param name="id">Pose ID</param>
        public void ApplyPose(int id = 0, bool interpolate = false)
        {
            if(PoseInterpolationCoroutine != null)
            {
                return;
            }

            if (Poses.Count <= id)
                return;

            if (interpolate)
            {
                PoseInterpolationCoroutine = StartCoroutine(InterpolateToPose(id));
            }
            else
            {
                foreach (var part in Poses[id].Parts)
                {
                    part.PartTransform.localRotation = part.PartRotation;
                }

                if (Poses[id].AnimationProfiles.Count > 0)
                {
                    ApplyPoseProfiles(Poses[id].AnimationProfiles);
                }
            }

        }

        /// <summary>
        /// Get the Pose data 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PGAPose GetPose(int id)
        {
            return Poses[id];
        }

        private IEnumerator InterpolateToDefaultPose()
        {
            ApplyPoseProfiles(TransitionProfiles);

            float i = 0f;
            while (i < 1f)
            {
                foreach (var part in DefaultPose.Parts)
                {
                    part.PartTransform.localRotation = Quaternion.Slerp(part.PartTransform.localRotation, part.PartRotation, Time.deltaTime * 3f);
                }

                i += Time.deltaTime * 3f;

                yield return null;
            }

            PoseInterpolationCoroutine = null;
            //Debug.Log("PGA Pose Interpolation Done");


            if (DefaultPose.AnimationProfiles.Count > 0)
            {
                ApplyPoseProfiles(DefaultPose.AnimationProfiles);
            }
        }

        private IEnumerator InterpolateToPose(int id)
        {
            ApplyPoseProfiles(TransitionProfiles);

            float i = 0f;
            while (i < 1f)
            {
                yield return new WaitForEndOfFrame();

                foreach (var part in Poses[id].Parts)
                {
                    part.PartTransform.localRotation = Quaternion.Slerp(part.PartTransform.localRotation, part.PartRotation, i);
                }

                i += Time.deltaTime * 3f;
            }

            PoseInterpolationCoroutine = null;
            //Debug.Log("PGA Pose Interpolation Done");


            if (Poses[id].AnimationProfiles.Count > 0)
            {
                ApplyPoseProfiles(Poses[id].AnimationProfiles);
            }
        }

        #endregion

        /// <summary>
        /// Callback from ApplyPose to set the animation profiles as necessary. Does nothing in base class.
        /// </summary>
        protected virtual void ApplyPoseProfiles(List<AnimationProfile> profiles)
        {

        }

        [ContextMenu("Default Constraints", false, 50)]
        public virtual void Default()
        {
            foreach(var constraint in RigConstraints)
            {
                constraint.SetDefault();
            }
        }
         
        
    }
}