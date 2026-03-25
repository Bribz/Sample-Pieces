using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animation.PGA
{
    public class JiggleConstraintObj : MonoBehaviour
    {
        public AnimationProfile ProfileOverride = null;
        public bool HasProfileOverride { get { return ProfileOverride != null; } }

        public JiggleConstraintObj ChainParent;
        private List<WindComponent> AttachedWinds;

        public JiggleConstraintObj parentConstraint;
        public Transform TargetRoot;
        public Vector3 Target;
        public Vector3 Particle;

        public Vector3 PoleTarget;
        public Vector3 PoleParticle;

        public float DynamicOffset;

        public Quaternion ExpectedRot;
        public Quaternion InitialRot;

        public float Stiffness = 1f;
        public float Springiness = 500f;
        [Range(0f, 1f)]
        public float Dampening = .98f;

        public RotationalLimit LIMIT = RotationalLimit.None;

        public float MaxAngle = 0f;
        public bool UseGravity = false;
        public bool UseExternalForces = false;

        public bool TargetRootIsSelf = false;

        public void Awake()
        {
            /*
            if (transform.parent != null && transform.parent.GetComponent<JiggleConstraintObj>() != null)
            {
                TargetRoot = transform.parent;
                parentConstraint = transform.parent.GetComponent<JiggleConstraintObj>();
            }
            else
            {
                TargetRoot = transform;
                parentConstraint = this;
                TargetRootIsSelf = true;
            }
            */

            InitialRot = transform.localRotation;

            if(ProfileOverride != null)
            {
                Dampening = ProfileOverride.JC_Dampening;
                DynamicOffset = ProfileOverride.JC_DynamicOffset;
                MaxAngle = ProfileOverride.JC_MaximumAngle;
                LIMIT = ProfileOverride.JC_RotationalLimit;
                Stiffness = ProfileOverride.JC_Stiffness;
                UseExternalForces = ProfileOverride.JC_UseExternalForces;
                UseGravity =  ProfileOverride.JC_UseGravity;
                UsePhysics = ProfileOverride.JC_UsePhysics;
            }
        }

        public void SetChainParent(JiggleConstraintObj chainParent)
        {
            ChainParent = chainParent;
        }

        public void SetWindAttachments(List<WindComponent> winds)
        {
            AttachedWinds = winds;

            foreach(var wind in AttachedWinds)
            {
                wind.AttachComponent(this);
            }
        }

        public void SetParentData(JiggleConstraintObj jiggleConstraintObj)
        {
            if (jiggleConstraintObj != null)
            {
                parentConstraint = jiggleConstraintObj;
                TargetRoot = jiggleConstraintObj.transform;
            }
            else
            {
                parentConstraint = this;
                TargetRoot = transform;
                TargetRootIsSelf = true;
            }

            Target = TargetRoot.position + InitialRot * (TargetRoot.forward * DynamicOffset);
            Particle = Target;

            PoleTarget = TargetRoot.position + InitialRot * (TargetRoot.up * DynamicOffset);
            PoleParticle = PoleTarget;
        }

        public void SetProfileOverride(AnimationProfile profile)
        {
            ProfileOverride = profile;

            if (profile != null)
            {
                Dampening = ProfileOverride.JC_Dampening;
                DynamicOffset = ProfileOverride.JC_DynamicOffset;
                MaxAngle = ProfileOverride.JC_MaximumAngle;
                LIMIT = ProfileOverride.JC_RotationalLimit;
                Stiffness = ProfileOverride.JC_Stiffness;
                UseExternalForces = ProfileOverride.JC_UseExternalForces;
                UseGravity = ProfileOverride.JC_UseGravity;
                UsePhysics = ProfileOverride.JC_UsePhysics;
            }
        }

        public void OnDrawGizmosSelected()
        {
            if (parentConstraint != this)//TargetRoot && TargetRoot == transform.parent)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(TargetRoot.position, Target);

                Gizmos.DrawLine(transform.position, PoleTarget + (transform.position - TargetRoot.position));

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(Particle, .02f);
                Gizmos.DrawSphere(PoleParticle + (transform.position - TargetRoot.position), .01f);

                //Gizmos.color = Color.green;
                //Gizmos.DrawLine(PoleParticle + (transform.position - TargetRoot.position), (PoleParticle + (transform.position - TargetRoot.position)) + PoleParticleVel);
            }
        }

        public bool UsePhysics;

        public Vector3 ExternalForces;

        Vector3 ParticleVel;
        Vector3 PoleParticleVel;

        Vector3 ParticleDir;
        Vector3 PoleParticleDir;
        public void Solve(float delta)
        {
            var transformPos = transform.position;
            var targetRootPos = TargetRoot.position;
            var targetRootFor = TargetRoot.forward;
            var targetRootUp = TargetRoot.up;

            if (!UsePhysics)
            {
                Target = targetRootPos + InitialRot * (targetRootFor * DynamicOffset);
                PoleTarget = targetRootPos + InitialRot * (targetRootUp * DynamicOffset);
                
                ExternalForces = Vector3.zero;
                if (UseExternalForces)
                {
                    if (UseGravity)
                    {
                        ExternalForces = Physics.gravity;
                    }
                    ExternalForces += PhysicsManager.instance.GetWindForce(ChainParent, transformPos);
                    ExternalForces += PhysicsManager.instance.GetExplosiveForce(transformPos);
                }

                Quaternion PreRot;

                switch (LIMIT)
                {
                    //Y Axis Only
                    case RotationalLimit.Pan:
                        //PoleParticle = PoleTarget; //Vector3.Lerp(PoleParticle, PoleTarget, Stiffness * delta);
                        //Particle = Vector3.Lerp(Particle, Target, Stiffness * delta);

                        PoleParticleDir = Vector3.Lerp(PoleParticleDir, (PoleTarget - PoleParticle), Springiness * delta);
                        ParticleDir = Vector3.Lerp(ParticleDir, (Target - Particle), Springiness * delta);

                        PoleParticle = PoleTarget; //PoleParticle + PoleParticleDir * (Stiffness * delta) + (ExternalForces * delta);
                        Particle = Particle + ParticleDir * (Stiffness * delta) + (ExternalForces * delta);

                        PreRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(Particle - targetRootPos, targetRootUp), (PoleParticle - targetRootPos));
                        break;

                    //X Axis only
                    case RotationalLimit.Tilt:
                        //PoleParticle = PoleTarget; //Vector3.Lerp(PoleParticle, PoleTarget, Stiffness * delta);
                        //Particle = Vector3.Lerp(Particle, Target, Stiffness * delta);

                        PoleParticleDir = Vector3.Lerp(PoleParticleDir, (PoleTarget - PoleParticle), Springiness * delta);
                        ParticleDir = Vector3.Lerp(ParticleDir, (Target - Particle), Springiness * delta);

                        PoleParticle = PoleTarget;//PoleParticle + PoleParticleDir * (Stiffness * delta) + (ExternalForces * delta);
                        Particle = Particle + ParticleDir * (Stiffness * delta) + (ExternalForces * delta);

                        PreRot = Quaternion.LookRotation(Vector3.ProjectOnPlane((Particle - targetRootPos), TargetRoot.right), (PoleParticle - targetRootPos));
                        break;

                    //Z Axis only
                    case RotationalLimit.Roll:
                        //PoleParticle = Vector3.Lerp(PoleParticle, PoleTarget, Stiffness * delta);
                        //Particle = Target; //Vector3.Lerp(Particle, Target, Stiffness * delta);

                        PoleParticleDir = Vector3.Lerp(PoleParticleDir, (PoleTarget - PoleParticle), Springiness * delta);
                        ParticleDir = Vector3.Lerp(ParticleDir, (Target - Particle), Springiness * delta);

                        PoleParticle = PoleParticle + PoleParticleDir * (Stiffness * delta) + (ExternalForces * delta);
                        Particle = Target; //Particle + ParticleDir * (Stiffness * delta) + (ExternalForces * delta);

                        PreRot = Quaternion.LookRotation(Particle - targetRootPos, (PoleParticle - targetRootPos));
                        break;

                    case RotationalLimit.None:
                    default:
                        PoleParticleDir = Vector3.Lerp(PoleParticleDir, (PoleTarget - PoleParticle), Springiness * delta);
                        ParticleDir = Vector3.Lerp(ParticleDir, (Target - Particle), Springiness * delta);

                        PoleParticle = PoleParticle + PoleParticleDir * (Stiffness * delta) + (ExternalForces * delta);
                        Particle = Particle + ParticleDir * (Stiffness * delta) + (ExternalForces * delta);

                        PreRot = Quaternion.LookRotation(Particle - targetRootPos, (PoleParticle - targetRootPos));
                        break;
                }

                if (MaxAngle > 0f)
                {
                    float currAngle = Quaternion.Angle(PreRot, TargetRoot.rotation);
                    if (currAngle > MaxAngle)
                    {
                        ExpectedRot = Quaternion.RotateTowards(TargetRoot.rotation, PreRot, MaxAngle);
                    }
                    else
                    {
                        ExpectedRot = PreRot;
                    }
                }
                else
                {
                    ExpectedRot = PreRot;
                }
            }
            else
            {
                //PoleParticleDir = Vector3.Lerp(PoleParticleDir, PoleTarget - PoleParticle, Springiness * delta);
                //ParticleDir = Vector3.Lerp(ParticleDir, Target - Particle, Springiness * delta);

                Target = targetRootPos + (targetRootFor * DynamicOffset);
                PoleTarget = targetRootPos + (targetRootUp * DynamicOffset);

                PoleParticleDir = (PoleTarget - PoleParticle);
                ParticleDir = (Target - Particle);

                ParticleVel += ParticleDir * Stiffness;
                PoleParticleVel += PoleParticleDir * Stiffness;

                ExternalForces = Vector3.zero;
                if (UseExternalForces)
                {
                    if (UseGravity)
                    {
                        ExternalForces = Physics.gravity;
                    }
                    ExternalForces += PhysicsManager.instance.GetWindForce(ChainParent, transformPos);
                    ExternalForces += PhysicsManager.instance.GetExplosiveForce(transformPos);
                }

                ParticleVel += ExternalForces;
                PoleParticleVel += ExternalForces;

                if (PoleParticleVel.magnitude > Springiness)
                {
                    PoleParticleVel = PoleParticleVel.normalized * Springiness;
                }
                if (ParticleVel.magnitude > Springiness)
                {
                    ParticleVel = ParticleVel.normalized * Springiness;
                }

                //ParticleVel *= .5f;
                //PoleParticleVel *= .5f;

                ParticleVel *= Dampening;
                PoleParticleVel *= Dampening;

                switch(LIMIT)
                {
                    case RotationalLimit.Pan:
                        PoleParticle = PoleTarget;
                        Particle = Particle + (ParticleVel * delta);

                        ExpectedRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(Particle - targetRootPos, targetRootUp), (PoleParticle - targetRootPos));
                        break;

                    case RotationalLimit.Tilt:
                        PoleParticle = PoleTarget;
                        Particle = Particle + (ParticleVel * delta);

                        ExpectedRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(Particle - targetRootPos, TargetRoot.right), (PoleParticle - targetRootPos));
                        break;

                    case RotationalLimit.Roll:
                        PoleParticle = PoleParticle + (PoleParticleVel * delta);
                        Particle = Target;

                        ExpectedRot = Quaternion.LookRotation(Particle - targetRootPos, (PoleParticle - targetRootPos));
                        break;

                    case RotationalLimit.None:
                    default:
                        PoleParticle = PoleParticle + (PoleParticleVel * delta);
                        Particle = Particle + (ParticleVel * delta);

                        ExpectedRot = Quaternion.LookRotation((Particle - targetRootPos), (PoleParticle - targetRootPos));
                        break;
                }
            }
        }

        /// <summary>
        /// Adjust rotational lock of constraint bones
        /// </summary>
        /// <param name="rot">World Rotation</param>
        public void SetRotationLock(Quaternion rot)
        {
            InitialRot = rot;
        }
    }
}