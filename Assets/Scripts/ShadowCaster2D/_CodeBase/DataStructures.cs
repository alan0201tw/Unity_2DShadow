using UnityEngine;

namespace ShadowCaster2D
{
    public struct RaycastHit2DPartialInfo
    {
        public bool isHittingCollider;
        public Vector3 point;
        public float distance;
        public float angle;

        public RaycastHit2DPartialInfo(bool _hit, Vector3 _point, float _distance, float _angle)
        {
            isHittingCollider = _hit;
            point = _point;
            distance = _distance;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}