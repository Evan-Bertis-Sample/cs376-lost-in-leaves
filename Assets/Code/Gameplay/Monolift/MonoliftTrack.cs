using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace LostInLeaves.Monolift
{
    public class MonoliftTrack : MonoBehaviour
    {
        [field: SerializeField] public List<GameObject> Positions { get; private set; } = new List<GameObject>();
        [field: SerializeField] public bool Loop { get; private set; } = false;

        public Vector3 GetPositionAtIndex(int i)
        {
            if (i < 0 || i >= Positions.Count) return Vector3.zero;

            return Positions[i].transform.position;
        }

        public float GetDistanceToNext(int i = 0)
        {
            if (i < 0 || i >= Positions.Count) return 0;

            int nextIndex = (i == Positions.Count - 1) ? 0 : i + 1;

            Vector3 cur = GetPositionAtIndex(i);
            Vector3 next = GetPositionAtIndex(nextIndex);
            float dist = Vector3.Distance(cur, next);
            return dist;
        }

        public void SetPoint(int index, Vector3 position)
        {
            if (index < 0 || index >= Positions.Count) return;

            Positions[index].transform.position = position;
        }

        public Vector3 GetPosition(float t)
        {
            float length = GetLength();
            float total = 0;
            t = t % 1;

            for (int i = 0; i < Positions.Count - 1; i++)
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
            if (Positions.Count == 0) return 0;

            float length = 0;
            for (int i = 0; i < Positions.Count - 1; i++)
            {
                Vector3 cur = GetPositionAtIndex(i);
                Vector3 next = GetPositionAtIndex(i + 1);
                length += Vector3.Distance(cur, next);
            }

            if (Loop)
            {
                length += Vector3.Distance(Positions[Positions.Count - 1].transform.position, Positions[0].transform.position);
            }

            return length;
        }

        public void AddPoint(int index = -1, Vector3 position = default)
        {
            if (index == -1) index = Positions.Count;

            GameObject point = new GameObject($"Point-{index}");
            point.transform.parent = transform;

            if (position != default)
            {
                point.transform.position = position;
            }
            else
            {
                // calculate the end point based upon our index
                if (index != 0 && index != Positions.Count - 1)
                {
                    Vector3 prevPos = GetPositionAtIndex(index - 1);
                    Vector3 nextPos = GetPositionAtIndex(index + 1);

                    Vector3 pos = (prevPos + nextPos) / 2;
                    point.transform.position = pos;
                }
                else
                {
                    // we are at the start or the end, just use that point
                    Vector3 pos = GetPositionAtIndex(index);
                    point.transform.position = pos;
                }
            }

            if (index == -1)
            {
                Positions.Add(point);
            }
            else
            {
                Positions.Insert(index, point);
            }
        }

        public void RemovePoint(int index)
        {
            if (index == -1) index = Positions.Count - 1;
            if (index < 0 || index >= Positions.Count) return;

            GameObject point = Positions[index];
            Positions.RemoveAt(index);
            DestroyImmediate(point);

            // rename all the points, if they are prefixed with Point-#
            for (int i = 0; i < Positions.Count; i++)
            {
                GameObject cur = Positions[i];
                if (cur.name.StartsWith("Point-"))
                    cur.name = $"Point-{i}";
            }
        }
    }
}
