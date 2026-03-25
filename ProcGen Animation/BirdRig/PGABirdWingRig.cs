using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Animation.PGA.Bird
{
    public class PGABirdWingRig : PGARig
    {
        public PGABirdRig parentRig;

        public Transform FlapRoot;
        public Transform TwistRoot;

        public float flapAngle;
        public float TimeOffset;


        public AnimationCurve FlapCurve;

        private float flapTime = 0;
        private float flapSpeed = 1;

        public int CurrentPose = -1;

        public Quaternion OriginRot;

        public bool RightWing = false;

        public float TwistCorrection = 20f;

        private void Awake()
        {
            OriginRot = FlapRoot.localRotation;
        }

        public override void Default()
        {
            base.Default();
        }

        public override void Solve(float delta)
        {
            StateManager();
            base.Solve(delta);
        }

        bool poseUpdated = false;
        public override void Update()
        {
            base.Update();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        void StateManager()
        {
            flapTime = parentRig.NormalizedTime - TimeOffset;

            DoFlap();
        }

        void DoFlap()
        {

            float x = (FlapCurve.Evaluate(flapTime) - .5f) * flapAngle;

            x = Mathf.Lerp(FlapCurve.Evaluate(0) * flapAngle, x, parentRig.GlideMultiplier);
            if (RightWing)
            {
                FlapRoot.localRotation = Quaternion.Euler(0, 0, -x) * OriginRot;
            }
            else
            {
                FlapRoot.localRotation = Quaternion.Euler(0, 0, x) * OriginRot;
            }
        }
    }
}