using System.Collections.Generic;
using UnityEngine;

namespace CookApps.WorldTouch
{
    public class TouchCorrectionCylinder : CachedMonoBehaviour
    {
        public static TouchCorrectionCylinder Create(Transform parent, float radius)
        {
            var go = new GameObject("TouchCorrectionCylinder");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            var cylinder = go.AddComponent<TouchCorrectionCylinder>();
            var collider = go.AddComponent<CapsuleCollider>();
            cylinder.capsuleCollider = collider;
            collider.isTrigger = true;
            collider.radius = radius;
            collider.height = 1.0f;
            collider.direction = 2; // Z axis
            return cylinder;
        }

        private CapsuleCollider capsuleCollider;
        private List<Collider> colliders = new ();
        
        public void SetRadius(float radius)
        {
            capsuleCollider.radius = radius;
        }
        
        public void SetHeight(float height)
        {
            capsuleCollider.height = height;
            capsuleCollider.center = new Vector3(0, 0, height * 0.5f);
        }
        
        public void OnTriggerEnter(Collider other)
        {
            var comp = other.gameObject.GetComponentInParent<ITouchListener>();
            if (comp == null)
                return;

            colliders.Add(other);
        }

        public void OnTriggerExit(Collider other)
        {
            var comp = other.gameObject.GetComponentInParent<ITouchListener>();
            if (comp == null)
                return;

            colliders.Remove(other);
        }

        // 실린더의 중심축에서 가장 가까운 오브젝트를 반환
        public ITouchListener GetMostNearestObject()
        {
            colliders.RemoveAll(x =>
            {
                return x == null || x.enabled == false;
            });

            Vector3 cylinderPosition = CachedTr.position;
            Vector3 cylinderForward = CachedTr.forward;

            Collider closest = null;
            float minDistance = float.MaxValue;

            foreach (var collider in colliders)
            {
                Vector3 surfacePoint = collider.ClosestPoint(cylinderPosition);
                Vector3 toSurface = surfacePoint - cylinderPosition;
                Vector3 axisPoint = cylinderPosition + Vector3.Project(toSurface, cylinderForward);

                float radialDistance = Vector3.Distance(surfacePoint, axisPoint);

                if (radialDistance < minDistance)
                {
                    minDistance = radialDistance;
                    closest = collider;
                }
            }

            if (closest == null)
                return null;

            return closest.gameObject.GetComponentInParent<ITouchListener>();
        }

        public void Clear()
        {
            colliders.Clear();
        }
    }
}
