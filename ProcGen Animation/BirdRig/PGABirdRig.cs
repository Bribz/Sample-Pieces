using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animation.PGA.Bird
{
    public enum PGABirdState
    {
        Automatic,
        ForcedFlap,
        ForcedGlide
    }

    public class PGABirdRig : PGARig
    {
        public PGABirdWingRig RightWing;
        public PGABirdWingRig LeftWing;

        public Transform RootTransform;

        public AnimationCurve BobCurve;

        public Transform MoveTarget;

        private Vector3 m_startPosition;

        public float NormalizedTime;
        private float m_bobFrequency = 1f;
        public float m_bobAmplitude = -2f;

        public float GlideMultiplier;

        public float RotateSpeed = 90f;
        public float MoveSpeed = 35f;

        public float StateTransitionSpeed = 5f;
        float bobMult = 0f;

        public PGABirdState State = PGABirdState.Automatic;

        private void Awake()
        {
            m_startPosition = RootTransform.localPosition;
        }

        public override void Default()
        {
            base.Default();
        }

        protected override void ApplyPoseProfiles(List<AnimationProfile> profiles)
        {
            base.ApplyPoseProfiles(profiles);
        }

        public override void Solve(float delta)
        {
            RightWing.Solve(delta);
            LeftWing.Solve(delta);

            base.Solve(delta);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //Update Timescale
            NormalizedTime += Time.fixedDeltaTime * m_bobFrequency;
            if(NormalizedTime >= 1f)
            {
                NormalizedTime -= 1f;
            }

            
            switch(State)
            {
                case PGABirdState.Automatic:
                //Set Flap and Bob angle modifiers
                    GlideMultiplier = Mathf.Clamp01(Vector3.Dot(Vector3.up, RootTransform.up)) > .88f ? 1f : .05f;
                    bobMult = Mathf.Lerp(bobMult, Mathf.Clamp(Vector3.Dot(Vector3.up, RootTransform.up), .75f, 1f), Time.deltaTime * StateTransitionSpeed); 
                    break;
                case PGABirdState.ForcedFlap:
                    GlideMultiplier = 1f;
                    bobMult = Mathf.Lerp(bobMult, 1f, Time.deltaTime * StateTransitionSpeed);
                    //bobMult = 1f;
                    break;
                case PGABirdState.ForcedGlide:
                    GlideMultiplier = .05f;
                    bobMult = Mathf.Lerp(bobMult, .75f, Time.deltaTime * StateTransitionSpeed);
                    //bobMult = .75f;
                    break;
            }
            

            //Handle bob
            float y = BobCurve.Evaluate(NormalizedTime) * m_bobAmplitude;
            var nbobMult = Interp.Remap(bobMult, .75f, 1f, 0f, 1f);
            y = Mathf.Lerp(0, y, nbobMult);

            RootTransform.localPosition = m_startPosition + new Vector3(0, y, 0);

            //Move Bird
            if (MoveTarget != null)
            {
                Vector3 toTarget = ((MoveTarget.position + Vector3.up * 2f) - transform.position).normalized;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(toTarget), RotateSpeed * Time.deltaTime);
                transform.position += transform.forward * MoveSpeed * Time.deltaTime;
            }
        }
    }
}