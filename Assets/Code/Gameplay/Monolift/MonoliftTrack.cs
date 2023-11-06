using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInLeaves.Monolift
{
    public class MonoliftTrack : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _positions = new List<GameObject>();
        [SerializeField] private bool _loop = false;

        [Header("Debug")]
        [SerializeField, Range(-5f, 5f)] private float _time = 0;
        [SerializeField] private float _ballGizmoSize = 0.5f;
        [SerializeField] private Color _ballGizmoColor = Color.red;
        [SerializeField] private Color _timeGizmoColor = Color.green;
        [SerializeField] private Color _trackGizmoColor = Color.blue;

        public Vector3 GetPositionAtIndex(int i)
        {
            if (i < 0 || i >= _positions.Count) return Vector3.zero;

            return _positions[i].transform.position;
        }

        public float GetDistanceToNext(int i = 0)
        {
            if (i < 0 || i >= _positions.Count) return 0;

            int nextIndex = (i == _positions.Count - 1) ? 0 : i + 1;

            Vector3 cur = GetPositionAtIndex(i);
            Vector3 next = GetPositionAtIndex(nextIndex);
            float dist = Vector3.Distance(cur, next);
            return dist;
        }

        public Vector3 GetPosition(float t)
        {
            float length = GetLength();
            float total = 0;
            t = t % 1;

            for (int i = 0; i < _positions.Count - 1; i++)
            {
                float dist = GetDistanceToNext(i);
                float percent = dist / length;
                total += percent;

                if (total > t)
                {
                    // We're on the right segment
                    float segmentPercent = (t - (total - percent)) / percent;
                    Vector3 cur = GetPositionAtIndex(i);
                    Vector3 next = GetPositionAtIndex(i + 1);
                    Vector3 pos = Vector3.Lerp(cur, next, segmentPercent);
                    return pos;
                }
            }

            return Vector3.zero;
        }

        public float GetLength()
        {
            if (_positions.Count == 0) return 0;

            float length = 0;
            for (int i = 0; i < _positions.Count - 1; i++)
            {
                Vector3 cur = GetPositionAtIndex(i);
                Vector3 next = GetPositionAtIndex(i + 1);
                length += Vector3.Distance(cur, next);
            }

            if (_loop)
            {
                length += Vector3.Distance(_positions[_positions.Count - 1].transform.position, _positions[0].transform.position);
            }

            return length;
        }

        private void OnDrawGizmos()
        {
            // Draw the track
            for (int i = 0; i < _positions.Count - 1; i++)
            {
                Vector3 cur = GetPositionAtIndex(i);
                Gizmos.color = _ballGizmoColor;
                Gizmos.DrawSphere(cur, _ballGizmoSize);
                Vector3 next = GetPositionAtIndex(i + 1);
                Gizmos.color = _trackGizmoColor;
                Gizmos.DrawLine(cur, next);
            }

            // Draw the last ball
            Vector3 last = GetPositionAtIndex(_positions.Count - 1);
            Gizmos.color = _ballGizmoColor;
            Gizmos.DrawSphere(last, _ballGizmoSize);


            if (_loop)
            {
                Gizmos.color = _trackGizmoColor;
                Gizmos.DrawLine(GetPositionAtIndex(_positions.Count - 1), GetPositionAtIndex(0));
            }

            // Draw the current position
            Gizmos.color = _timeGizmoColor;
            Gizmos.DrawSphere(GetPosition(_time), _ballGizmoSize);

            Gizmos.color = Color.white; // Reset the color
        }
    }
}
