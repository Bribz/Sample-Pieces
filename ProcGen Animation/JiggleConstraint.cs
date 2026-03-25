using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animation.PGA
{
    public enum RotationalLimit : byte { None, Roll, Tilt, Pan }

    public class JiggleConstraint : PGAConstraint
    {
        [Tooltip("Should the values get updated at runtime? Keeping this off saves on performance")]
        public bool UpdateAtRuntime = false; 

        [Header("Animation Profile")]
        public AnimationProfile Profile;

        /// <summary>
        /// Updates Stiffness of chain. Does not update at runtime
        /// </summary>
        [Header("Variables")]
        public bool ShouldSetDefault = true;
        public RotationalLimit Limit = RotationalLimit.None;
        public float DynamicOffset;
        public float MaximumAngle = 0f;
        public AnimationCurve StiffnessCurve;
        public float Dampening = .98f;
        public bool UseExternalForces;
        public bool UseGravity;
        public bool UsePhysics;
        //public AnimationCurve SpringinessCurve;

        [Header("Bones")]
        public Transform[] Bones;
        public JiggleConstraintObj[] BoneData;

        private Vector3[] m_defaultBonePos;
        private Quaternion[] m_defaultBoneRot;

        public List<WindComponent> AttachedWindComponents;


        private void Awake()
        {
            m_defaultBonePos = new Vector3[Bones.Length];
            m_defaultBoneRot = new Quaternion[Bones.Length];

            BoneData = new JiggleConstraintObj[Bones.Length];

            for (int i = 0; i < Bones.Length; i++)
            {
                BoneData[i] = Bones[i].gameObject.GetOrAddComponent<JiggleConstraintObj>();

                if (i > 0)
                {
                    BoneData[i].SetParentData(BoneData[i - 1]);
                    BoneData[i].SetChainParent(BoneData[0]);
                }
                else
                {
                    BoneData[i].SetParentData(null);
                    BoneData[i].SetChainParent(BoneData[i]);
                    BoneData[i].SetWindAttachments(AttachedWindComponents);
                }

                m_defaultBonePos[i] = Bones[i].position;
                m_defaultBoneRot[i] = Bones[i].rotation;
            }

            if (Profile != null)
            {
                Profile = ScriptableObject.Instantiate(Profile);
                SetAnimationProfile(Profile);
            }
            else
            {
                SetMaxRotationLimit(MaximumAngle);
                SetLimit(Limit);
                SetDynamicOffset(DynamicOffset);
                SetStiffness(StiffnessCurve);
                SetDampening(Dampening);
                //SetSpringiness(SpringinessCurve);
                SetPhysics(UsePhysics, UseExternalForces, UseGravity);
            }
        }

        public void SetAnimationProfile(AnimationProfile profile)
        {
            if (profile != null)
            {
                Profile = profile;
                SetMaxRotationLimit(Profile.JC_MaximumAngle);
                SetLimit(Profile.JC_RotationalLimit);
                SetDynamicOffset(Profile.JC_DynamicOffset);
                SetStiffness(Profile);
                SetDampening(Profile.JC_Dampening);
                //SetSpringiness(SpringinessCurve);
                SetPhysics(Profile.JC_UsePhysics, Profile.JC_UseExternalForces, Profile.JC_UseGravity);
            }
        }

        public void SetDampening(float dampening)
        {
            Dampening = dampening;
            foreach(var bone in BoneData)
            {
                if (!bone.HasProfileOverride)
                {
                    bone.Dampening = Dampening;
                }
            }
        }

        public void SetPhysics(bool usePhysics, bool useExternalForces, bool useGravity)
        {
            UsePhysics = usePhysics;
            UseExternalForces = useExternalForces;
            UseGravity = useGravity;

            foreach(var bone in BoneData)
            {
                if (!bone.HasProfileOverride)
                {
                    bone.UseGravity = UseGravity;
                    bone.UseExternalForces = UseExternalForces;
                    bone.UsePhysics = UsePhysics;
                }
            }
        }

        public void SetMaxRotationLimit(float maxAngle)
        {
            MaximumAngle = maxAngle;
            foreach(var bone in BoneData)
            {
                if (!bone.HasProfileOverride)
                {
                    bone.MaxAngle = maxAngle;
                }
            }
        }

        public void SetLimit(RotationalLimit limit)
        {
            Limit = limit;

            foreach(var bone in BoneData)
            {
                if (!bone.HasProfileOverride)
                {
                    bone.LIMIT = limit;
                }
            }
        }

        public void SetDynamicOffset(float val)
        {
            DynamicOffset = val;
            foreach(var bone in BoneData)
            {
                if (!bone.HasProfileOverride)
                {
                    bone.DynamicOffset = val;
                }
            }
        }

        /*
        public void SetSpringiness(AnimationCurve curve)
        {
            SpringinessCurve = curve;

            if (curve == null)
            {
                return;
            }

            float tVal = 1f / BoneData.Length;

            for (int i = 0; i < BoneData.Length; i++)
            {
                BoneData[i].Springiness = SpringinessCurve.Evaluate(tVal * i);
            }
        }
        */

        public void SetStiffness(AnimationProfile profile)
        {
            StiffnessCurve = profile.JC_StiffnessCurve;

            if (StiffnessCurve == null)
            {
                for (int i = 0; i < BoneData.Length; i++)
                {
                    if (!BoneData[i].HasProfileOverride)
                    {
                        BoneData[i].Stiffness = profile.JC_Stiffness;
                    }
                }
            }
            else
            {
                float tVal = 1f / BoneData.Length;

                for (int i = 0; i < BoneData.Length; i++)
                {
                    if (!BoneData[i].HasProfileOverride)
                    {
                        BoneData[i].Stiffness = StiffnessCurve.Evaluate(tVal * i);
                    }
                }
            }
        }

        public void SetStiffness(AnimationCurve curve)
        {
            StiffnessCurve = curve;

            if(curve == null)
            {
                return;
            }

            float tVal = 1f / BoneData.Length;

            for(int i = 0; i < BoneData.Length; i++)
            {
                if (!BoneData[i].HasProfileOverride)
                {
                    BoneData[i].Stiffness = StiffnessCurve.Evaluate(tVal * i);
                }
            }
        }

        public override void SetDefault()
        {
            if (!ShouldSetDefault)
                return;
            for(int i = 0; i < Bones.Length; i++)
            {
                //Bones[i].position = m_defaultBonePos[i];
                //Bones[i].rotation = m_defaultBoneRot[i];
            }
        }

        public override void Solve(float delta)
        {
            for(int i = 1; i < BoneData.Length; i++)
            {
                BoneData[i].Solve(delta);

                Bones[i].rotation = BoneData[i].ExpectedRot;
            }
        }

        public override Transform GetBone(int id)
        {
            return Bones[id];
        }

        public override Transform GetLastBone()
        {
            return Bones[Bones.Length - 1];
        }

        private void Update()
        {
            if (UpdateAtRuntime)
            {
                SetAnimationProfile(Profile);

                SetMaxRotationLimit(MaximumAngle);
                SetLimit(Limit);
                SetDynamicOffset(DynamicOffset);
                SetStiffness(StiffnessCurve);
                SetDampening(Dampening);
                //SetSpringiness(SpringinessCurve);
                SetPhysics(UsePhysics, UseExternalForces, UseGravity);
            }
        }
    }
}