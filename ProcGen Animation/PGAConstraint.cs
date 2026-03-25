using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animation.PGA
{
    public abstract class PGAConstraint : MonoBehaviour
    {
        public const float GIZMO_SPHERE_SIZE = 0.058f;

        [Range(0f, 1f)]
        public float Weight = 1f;
        public abstract void Solve(float delta);

        public abstract void SetDefault();

        public abstract Transform GetBone(int id);

        public abstract Transform GetLastBone();
    }
}