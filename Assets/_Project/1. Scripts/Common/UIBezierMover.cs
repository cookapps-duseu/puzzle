using System;
using RabbitDog;
using RabbitDog.UIExtensions;
using RabbitDog.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Template
{
    public class UIBezierMover : CachedMonoBehaviour
    {
        [Header("UI Bezier Movement")]
        public Vector3 startPoint;
        public Vector3 endPoint;

        [Header("Bezier Curve Controls")]
        [Range(0f, 1f)]
        public float controlPointOffsetRatio = 0.5f;
        [Tooltip("Minimum and maximum offset values (can span negative to positive). Only values outside the dead zone will be applied.")]
        public Vector2 controlPointOffsetMin = new Vector2(-5f, -5f);
        public Vector2 controlPointOffsetMax = new Vector2(5f, 5f);
        public Vector2 controlPointDeadZone = new Vector2(1f, 1f);

        [Header("Scale Over Time")]
        [Tooltip("Local scale at the start of the movement.")]
        public Vector3 startScale = Vector3.one;
        [Tooltip("Local scale at the end of the movement.")]
        public Vector3 endScale = Vector3.one;

        [Header("Arrival Effect Prefab (UI)")]
        [Tooltip("UI prefab (with RectTransform) that will be instantiated/preloaded under the same parent Canvas at the end point.")]
        public InstantParticle instantParticle;

        [Header("Animation Settings")]
        public float duration = 1f;
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private float elapsedTime;
        private Vector2 controlPoint;

        private Action<int> OnComplete;

        private int index;

        public void Initialize(int index, Transform dest = null, Action<int> onComplete = null)
        {
            OnComplete = onComplete;
            this.index = index;
            instantParticle.Ready(dest).Forget();
        }

        public async Awaitable StartMovement()
        {
            elapsedTime = 0f;
            CachedRectTr.position = startPoint;
            CachedRectTr.localScale = startScale;

            Vector2 midPoint = Vector2.Lerp(startPoint, endPoint, controlPointOffsetRatio);
            float offsetX = GenerateOffset(controlPointOffsetMin.x, controlPointOffsetMax.x, controlPointDeadZone.x);
            float offsetY = GenerateOffset(controlPointOffsetMin.y, controlPointOffsetMax.y, controlPointDeadZone.y);
            controlPoint = midPoint + new Vector2(offsetX, offsetY);
            while (true)
            {
                elapsedTime += Time.deltaTime;
                var t = Mathf.Clamp01(elapsedTime / duration);
                var ct = curve.Evaluate(t);

                CachedRectTr.position = CalculateBezierPoint(ct, startPoint, controlPoint, endPoint);
                CachedRectTr.localScale = Vector3.Lerp(startScale, endScale, t);
                if (t < 1f)
                {
                    await Awaitable.NextFrameAsync();
                    if (destroyCancellationToken.IsCancellationRequested)
                        break;
                }
                else
                {
                    break;
                }
            }

            instantParticle.Play(endPoint);
            
            gameObject.SetActive(false);
            Destroy(gameObject);
            OnComplete?.Invoke(index);
        }

        float GenerateOffset(float min, float max, float deadZone)
        {
            float val;
            do { val = Random.Range(min, max); }
            while (Mathf.Abs(val) < deadZone);
            return val;
        }

        Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }
    }
}
