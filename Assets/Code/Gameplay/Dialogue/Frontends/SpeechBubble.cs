using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CurlyUtility;
using TMPro;

namespace LostInLeaves.Components
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class SpeechBubble : MonoBehaviour
    {
        [Header("Bubble Settings")]
        [SerializeField] private Vector3 _bubbleScale;
        [SerializeField] private Vector2 _textPadding = new Vector2(0.25f, 0f);
        [SerializeField] private Vector2 _minimumSize = new Vector2(1f, 1f);
        [SerializeField] private Vector2 _maximumSize = new Vector2(5f, 5f);
        [SerializeField] private int _bevelSegments = 3;
        [SerializeField] private float _bubbleRaidus = 0.5f;
        [SerializeField] private float _bubbleInterpolation = 0.5f;

        [Header("Tail Settings")]
        [SerializeField] private float _tailWidth = 0.5f;
        [SerializeField] private Vector3 _maxTailOffset = new Vector3(0.5f, 0.5f, 0);

        [Header("Text References")]
        public TextMeshPro TextMesh;
        private GameObject _textMeshObject;
        private RectTransform _textMeshTransform;

        private List<Vector3> _vertices = new List<Vector3>();
        private List<int> _triangles = new List<int>();
        private Vector3 _tailTargetPosition;
        private Mesh _bubbleMesh;
        private MeshFilter _mf;
        private Transform _desireGoalTransform; // desired placement of the speech bubble
        private Vector3 _desiredGoalOffset;
        private Vector3 _desiredPosition;

        private void Start()
        {
            GetReferences();
            _desiredPosition = transform.position;
        }

        private void Update()
        {
            _desiredPosition = Vector3.Lerp(transform.position, _desireGoalTransform.position + _desiredGoalOffset, _bubbleInterpolation);
            transform.position = _desiredPosition;
        }

        private void GetReferences()
        {
            _mf = GetComponent<MeshFilter>();
            _textMeshTransform = TextMesh.GetComponent<RectTransform>();
            _textMeshObject = TextMesh.gameObject;
        }

        public void Render(Vector3 target)
        {
            if (_mf == null) GetReferences();
            _tailTargetPosition = target;
            GenerateBubble();
        }

        public void SetTailTarget(Vector3 target)
        {
            _tailTargetPosition = target;
        }

        public void SetBubblePosition(Transform transform, Vector3 offset = new Vector3())
        {
            _desireGoalTransform = transform;
            _desiredGoalOffset = offset;
        }

        public void SetText(string text)
        {
            if (_mf == null) GetReferences();
            TextMesh.text = text;
            TextMesh.ForceMeshUpdate();
            GenerateBubble();
        }

        public void OnDestroy()
        {
            Destroy(_textMeshObject);
        }

        #region Bubble Generation
        private void GenerateBubble()
        {
            SetScale();
            GenerateVertices();
            GenerateTriangles();
            GenerateMesh();
        }

        private void SetScale()
        {
            Vector2 minSizeCanvas = _minimumSize;
            Vector2 maxSizeCanvas = _maximumSize;
            Vector2 scale = minSizeCanvas;
            for (int i = 0; i < 2; i++) //Done multiple times to avoid "bouncing" while scaling
            {
                scale = ClampVector(TextMesh.GetPreferredValues(), minSizeCanvas, maxSizeCanvas);
                TextMesh.ForceMeshUpdate();
                _textMeshTransform.sizeDelta = scale;
            }
            Vector3 textScale = scale;
            _bubbleScale = textScale + (Vector3)(_textPadding * 2);
        }

        private void GenerateVertices()
        {
            //The first four points will be the "box" of the speech bubble
            _vertices = new List<Vector3>();
            float radX = _bubbleScale.x / 2f;
            float radY = _bubbleScale.y / 2f;
            _vertices.AddRange(new Vector3[]
                {
                new Vector3( radX,  radY),
                new Vector3( radX, -radY),
                new Vector3(-radX, -radY),
                new Vector3(-radX,  radY),
                }
            );

            List<Vector3> topRight = new List<Vector3>();
            List<Vector3> botRight = new List<Vector3>();
            List<Vector3> botLeft = new List<Vector3>();
            List<Vector3> topLeft = new List<Vector3>();

            //Generate the sides of the bubble
            for (int i = 1; i <= _bevelSegments; i++)
            {
                float t = (i * Mathf.PI) / (_bevelSegments * 2);
                float xOffset = Mathf.Sin(t) * _bubbleRaidus;
                float yOffset = (1 - Mathf.Cos(t)) * _bubbleRaidus;

                topRight.Add(_vertices[0] + new Vector3(xOffset, -yOffset));
                botRight.Add(_vertices[1] + new Vector3(xOffset, yOffset));
                botLeft.Add(_vertices[2] + new Vector3(-xOffset, yOffset));
                topLeft.Add(_vertices[3] + new Vector3(-xOffset, -yOffset));
            }

            _vertices.AddRange(topRight);
            _vertices.AddRange(botRight);
            _vertices.AddRange(botLeft);
            _vertices.AddRange(topLeft);
        }

        private void GenerateTriangles()
        {
            if (_vertices == null) return;
            if (_vertices.Count < 3) return;

            //"Box" of the speech bubble
            _triangles = new List<int>();
            _triangles.AddRange(new int[] { 0, 1, 3, 3, 1, 2 });

            //The sides of the speech bubble
            //Right side
            for (int i = 0; i < _bevelSegments; i++)
            {
                if (i == 0)
                {
                    _triangles.AddRange(new int[] { 0, 4, 1 });
                    continue;
                }

                _triangles.AddRange(new int[]
                    {
                    i + 3,
                    i + 4,
                    1
                    }
                );

            }
            for (int i = _bevelSegments; i < _bevelSegments * 2; i++)
            {
                if (i == _bevelSegments)
                {
                    _triangles.AddRange(new int[]
                        {
                        1,
                        _bevelSegments + 3,
                        _bevelSegments * 2 + 3
                        }
                    );
                    continue;
                }
                _triangles.AddRange(new int[]
                    {
                    1,
                    i + 4,
                    i + 3,
                    }
                );
            }

            //Left Side
            for (int i = _bevelSegments * 2; i < _bevelSegments * 3; i++)
            {
                if (i == _bevelSegments * 2)
                {
                    _triangles.AddRange(new int[] { 2, _bevelSegments * 2 + 4, 3 });
                    continue;
                }

                _triangles.AddRange(new int[]
                    {
                    3,
                    i + 3,
                    i + 4,
                    }
                );

            }

            for (int i = _bevelSegments * 3; i < _bevelSegments * 4; i++)
            {
                if (i == _bevelSegments * 3)
                {
                    _triangles.AddRange(new int[]
                        {
                        3,
                        _bevelSegments * 3 + 3,
                        _bevelSegments * 4 + 3
                        }
                    );
                    continue;
                }
                _triangles.AddRange(new int[]
                    {
                    i + 3,
                    3,
                    i + 4,
                    }
                );
            }
        }

        private Mesh GenerateTail(Vector3 target)
        {
            //Find the closest side on the rectangle
            //We assume the non constant x or y value of the side is the coordinating x or y value of the target point. Clamp this coordinating value to the bounds of the rectangle
            //Top, right, bot, left
            float halfWidth = _bubbleRaidus + _bubbleScale.x / 2; //The bevel expands the rectangle a little bit
            float halfHeight = _bubbleScale.y / 2;

            //target += targetOffset;
            Vector3[] sides = new Vector3[]
            {
            new Vector3(Mathf.Clamp(target.x, transform.position.x - halfWidth, transform.position.x + halfWidth), transform.position.y + halfHeight),
            new Vector3(transform.position.x + halfWidth, Mathf.Clamp(target.y, transform.position.y - halfHeight + _bubbleRaidus, transform.position.y + halfHeight - _bubbleRaidus)),
            new Vector3(Mathf.Clamp(target.x, transform.position.x - halfWidth, transform.position.x + halfWidth), transform.position.y - halfHeight),
            new Vector3(transform.position.x - halfWidth, Mathf.Clamp(target.y, transform.position.y - halfHeight + _bubbleRaidus, transform.position.y + halfHeight - _bubbleRaidus)),
            };

            //Now test the difference between all those
            float smallestDistance = float.MaxValue;
            int side = 0;
            for (int i = 0; i < 4; i++)
            {
                float distance = (sides[i] - target).magnitude;
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    side = i;
                }
            }

            int[] sideVertices =
            {
            3, 0,
            1, 0,
            2, 1,
            2, 3
        };

            //Use the side index to find the verticies using the look up table
            //These points are in local scale
            Vector3 minBound = _vertices[sideVertices[side * 2]];
            Vector3 maxBound = _vertices[sideVertices[side * 2 + 1]];
            //Correct points if the line is vertical, in order to account for the bevel
            if (side % 2 != 0)
            {
                minBound = minBound + new Vector3(_bubbleRaidus * ((minBound.x < 0) ? -1 : 1), _bubbleRaidus);
                maxBound = maxBound + new Vector3(_bubbleRaidus * ((maxBound.x < 0) ? -1 : 1), -_bubbleRaidus);
            }

            //Calculate where to place the points on the triangle
            //Even sides are the horizontal, odds are vertical
            //The points on the speech bubble
            Vector3 minTailPoint = (side % 2 == 0) ?
                new Vector3(target.x - _tailWidth / 2 - transform.position.x, minBound.y) :
                new Vector3(minBound.x, target.y - _tailWidth / 2 - transform.position.y);

            Vector3 maxTailPoint = (side % 2 == 0) ?
                new Vector3(target.x + _tailWidth / 2 - transform.position.x, maxBound.y) :
                new Vector3(maxBound.x, target.y + _tailWidth / 2 - transform.position.y);

            //Correct points to be on bubble
            float minT = InverseLerp(minBound, maxBound, minTailPoint);
            float maxT = InverseLerp(minBound, maxBound, maxTailPoint);
            if (minT <= 0)
            {
                //Correct max point
                minTailPoint = minBound;
                float newMaxT = _tailWidth / ((maxBound - minBound).magnitude);
                Mathf.Clamp(newMaxT, 0, 1);
                maxTailPoint = Vector3.Lerp(minBound, maxBound, newMaxT);
            }
            else if (maxT >= 1)
            {
                //Correct min point
                maxTailPoint = maxBound;
                float newMinT = 1 - (_tailWidth / ((maxBound - minBound).magnitude));
                Mathf.Clamp(newMinT, 0, 1);
                minTailPoint = Vector3.Lerp(minBound, maxBound, newMinT);
            }

            //The end of the tail
            Vector3 tailCenter = Vector3.Lerp(minTailPoint, maxTailPoint, 0.5f);
            Vector3 offset = new Vector3(
                ((target.x - transform.position.x) < 0) ? -_maxTailOffset.x : _maxTailOffset.x,
                ((target.y - transform.position.y) < 0) ? -_maxTailOffset.y : _maxTailOffset.y);

            Vector3 targetToTail = target - transform.position - tailCenter;

            Vector3 endTailPoint = tailCenter + new Vector3(
                (Mathf.Abs(targetToTail.x) < Mathf.Abs(offset.x)) ? targetToTail.x : offset.x,
                (Mathf.Abs(targetToTail.y) < Mathf.Abs(offset.y) ? targetToTail.y : offset.y)).normalized * offset.magnitude;


            //Order the three points of the triangle to be in clockwise order relative to the camera - hardcoded
            Vector3[] tailVertices;
            if (side == 0 || side == 3)
            {
                tailVertices = new Vector3[] { maxTailPoint, minTailPoint, endTailPoint };
            }
            else
            {
                tailVertices = new Vector3[] { minTailPoint, maxTailPoint, endTailPoint };
            }

            Mesh tailMesh = new Mesh
            {
                vertices = tailVertices,
                triangles = new int[] { 0, 1, 2 }
            };
            return tailMesh;
        }

        private void GenerateMesh()
        {
            if (_vertices == null) return;
            if (_vertices.Count < 3) return;
            if (_triangles == null) return;
            if (_triangles.Count < 3) return;

            _bubbleMesh = new Mesh
            {
                name = "Speech Bubble",
                vertices = _vertices.ToArray(),
                triangles = _triangles.ToArray()
            };

            Mesh tailMesh = GenerateTail(_tailTargetPosition);
            _bubbleMesh = MeshUtility.BasicCombineMeshes(new Mesh[] { _bubbleMesh, tailMesh });
            _bubbleMesh.RecalculateNormals();
            _mf.mesh = _bubbleMesh;
        }
        #endregion

        #region Utilities
        private float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
        }

        private Vector3 ClampVector(Vector3 value, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Mathf.Clamp(value.x, min.x, max.x),
                Mathf.Clamp(value.y, min.y, max.y),
                Mathf.Clamp(value.z, min.z, max.z));
        }
        #endregion
    }
}