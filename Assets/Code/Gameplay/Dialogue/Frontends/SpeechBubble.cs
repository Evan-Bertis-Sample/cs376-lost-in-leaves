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
        private Mesh _bubbleMesh;
        private MeshFilter _mf;
        private Canvas _canvas;

        [Header("Bubble Settings")]
        public Vector3 bubbleScale;
        public Vector2 textPadding = new Vector2(0.25f, 0f);
        public Vector2 minimumSize = new Vector2(1f, 1f);
        public Vector2 maximumSize = new Vector2(5f, 5f);
        public int bevelSegments = 3;
        public float bubbleRadius = 0.5f;

        public float tailWidth = 0.5f;
        public Vector3 maxTailOffset = new Vector3(0.5f, 0.5f, 0);
        public Vector2 scaleAdjustment = new Vector2(100f, 110f);

        public Vector3 target;

        [Header("Text Settings")]
        public TMPDrawer fontSettings;
        public GameObject textMeshObject;
        public RectTransform textMeshTransform;
        public TextMeshProUGUI textMesh;
        private Canvas canvas;


        private List<Vector3> vertices = new List<Vector3>();
        private List<int> triangles = new List<int>();

        public void Start()
        {
            Init();
        }

        public void Init()
        {
            if (_mf == null) _mf = GetComponent<MeshFilter>();
            if (textMesh == null) textMesh = GetComponent<TextMeshProUGUI>();
            textMeshObject = null;
        }

        public void SetCanvas(Canvas c)
        {
            canvas = c;
        }

        public void Render(Vector3 target)
        {
            this.target = target;
            GenerateBubble();
        }

        public void SetText(string text)
        {
            if (textMeshObject == null)
            {
                textMeshObject = UIUtility.SpawnTextBoxUI(transform.position, fontSettings, _canvas, out textMesh, out textMeshTransform, text, canvas);
                textMesh.ForceMeshUpdate();
                return;
            }
            textMeshTransform.position = UIUtility.WorldToCanvasSpace(transform.position);
            textMesh.text = text;
            textMesh.ForceMeshUpdate();
        }

        public void OnDestroy()
        {
            Destroy(textMeshObject);
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
            float canvasScale = textMesh.canvas.referencePixelsPerUnit * 4;
            Vector2 minSizeCanvas = minimumSize * canvasScale;
            Vector2 maxSizeCanvas = maximumSize * canvasScale;
            Vector2 scale = minSizeCanvas;
            for (int i = 0; i < 2; i++) //Done multiple times to avoid "bouncing" while scaling
            {
                scale = ClampVector(textMesh.GetPreferredValues(), minSizeCanvas, maxSizeCanvas);
                textMesh.ForceMeshUpdate();
                textMeshTransform.sizeDelta = scale;
            }
            Vector3 textScale = scale / canvasScale;
            bubbleScale = textScale + (Vector3)(textPadding * 2);
        }

        private void GenerateVertices()
        {
            //The first four points will be the "box" of the speech bubble
            vertices = new List<Vector3>();
            float radX = bubbleScale.x / 2f;
            float radY = bubbleScale.y / 2f;
            vertices.AddRange(new Vector3[]
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
            for (int i = 1; i <= bevelSegments; i++)
            {
                float t = (i * Mathf.PI) / (bevelSegments * 2);
                float xOffset = Mathf.Sin(t) * bubbleRadius;
                float yOffset = (1 - Mathf.Cos(t)) * bubbleRadius;

                topRight.Add(vertices[0] + new Vector3(xOffset, -yOffset));
                botRight.Add(vertices[1] + new Vector3(xOffset, yOffset));
                botLeft.Add(vertices[2] + new Vector3(-xOffset, yOffset));
                topLeft.Add(vertices[3] + new Vector3(-xOffset, -yOffset));
            }

            vertices.AddRange(topRight);
            vertices.AddRange(botRight);
            vertices.AddRange(botLeft);
            vertices.AddRange(topLeft);
        }

        private void GenerateTriangles()
        {
            if (vertices == null) return;
            if (vertices.Count < 3) return;

            //"Box" of the speech bubble
            triangles = new List<int>();
            triangles.AddRange(new int[] { 0, 1, 3, 3, 1, 2 });

            //The sides of the speech bubble
            //Right side
            for (int i = 0; i < bevelSegments; i++)
            {
                if (i == 0)
                {
                    triangles.AddRange(new int[] { 0, 4, 1 });
                    continue;
                }

                triangles.AddRange(new int[]
                    {
                    i + 3,
                    i + 4,
                    1
                    }
                );

            }
            for (int i = bevelSegments; i < bevelSegments * 2; i++)
            {
                if (i == bevelSegments)
                {
                    triangles.AddRange(new int[]
                        {
                        1,
                        bevelSegments + 3,
                        bevelSegments * 2 + 3
                        }
                    );
                    continue;
                }
                triangles.AddRange(new int[]
                    {
                    1,
                    i + 4,
                    i + 3,
                    }
                );
            }

            //Left Side
            for (int i = bevelSegments * 2; i < bevelSegments * 3; i++)
            {
                if (i == bevelSegments * 2)
                {
                    triangles.AddRange(new int[] { 2, bevelSegments * 2 + 4, 3 });
                    continue;
                }

                triangles.AddRange(new int[]
                    {
                    3,
                    i + 3,
                    i + 4,
                    }
                );

            }

            for (int i = bevelSegments * 3; i < bevelSegments * 4; i++)
            {
                if (i == bevelSegments * 3)
                {
                    triangles.AddRange(new int[]
                        {
                        3,
                        bevelSegments * 3 + 3,
                        bevelSegments * 4 + 3
                        }
                    );
                    continue;
                }
                triangles.AddRange(new int[]
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
            float halfWidth = bubbleRadius + bubbleScale.x / 2; //The bevel expands the rectangle a little bit
            float halfHeight = bubbleScale.y / 2;

            //target += targetOffset;
            Vector3[] sides = new Vector3[]
            {
            new Vector3(Mathf.Clamp(target.x, transform.position.x - halfWidth, transform.position.x + halfWidth), transform.position.y + halfHeight),
            new Vector3(transform.position.x + halfWidth, Mathf.Clamp(target.y, transform.position.y - halfHeight + bubbleRadius, transform.position.y + halfHeight - bubbleRadius)),
            new Vector3(Mathf.Clamp(target.x, transform.position.x - halfWidth, transform.position.x + halfWidth), transform.position.y - halfHeight),
            new Vector3(transform.position.x - halfWidth, Mathf.Clamp(target.y, transform.position.y - halfHeight + bubbleRadius, transform.position.y + halfHeight - bubbleRadius)),
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
            Vector3 minBound = vertices[sideVertices[side * 2]];
            Vector3 maxBound = vertices[sideVertices[side * 2 + 1]];
            //Correct points if the line is vertical, in order to account for the bevel
            if (side % 2 != 0)
            {
                minBound = minBound + new Vector3(bubbleRadius * ((minBound.x < 0) ? -1 : 1), bubbleRadius);
                maxBound = maxBound + new Vector3(bubbleRadius * ((maxBound.x < 0) ? -1 : 1), -bubbleRadius);
            }

            //Calculate where to place the points on the triangle
            //Even sides are the horizontal, odds are vertical
            //The points on the speech bubble
            Vector3 minTailPoint = (side % 2 == 0) ?
                new Vector3(target.x - tailWidth / 2 - transform.position.x, minBound.y) :
                new Vector3(minBound.x, target.y - tailWidth / 2 - transform.position.y);

            Vector3 maxTailPoint = (side % 2 == 0) ?
                new Vector3(target.x + tailWidth / 2 - transform.position.x, maxBound.y) :
                new Vector3(maxBound.x, target.y + tailWidth / 2 - transform.position.y);

            //Correct points to be on bubble
            float minT = InverseLerp(minBound, maxBound, minTailPoint);
            float maxT = InverseLerp(minBound, maxBound, maxTailPoint);
            if (minT <= 0)
            {
                //Correct max point
                minTailPoint = minBound;
                float newMaxT = tailWidth / ((maxBound - minBound).magnitude);
                Mathf.Clamp(newMaxT, 0, 1);
                maxTailPoint = Vector3.Lerp(minBound, maxBound, newMaxT);
            }
            else if (maxT >= 1)
            {
                //Correct min point
                maxTailPoint = maxBound;
                float newMinT = 1 - (tailWidth / ((maxBound - minBound).magnitude));
                Mathf.Clamp(newMinT, 0, 1);
                minTailPoint = Vector3.Lerp(minBound, maxBound, newMinT);
            }

            //The end of the tail
            Vector3 tailCenter = Vector3.Lerp(minTailPoint, maxTailPoint, 0.5f);
            Vector3 offset = new Vector3(
                ((target.x - transform.position.x) < 0) ? -maxTailOffset.x : maxTailOffset.x,
                ((target.y - transform.position.y) < 0) ? -maxTailOffset.y : maxTailOffset.y);

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

            Mesh tailMesh = new Mesh();
            tailMesh.vertices = tailVertices;
            tailMesh.triangles = new int[] { 0, 1, 2 };
            return tailMesh;
        }

        private void GenerateMesh()
        {
            if (vertices == null) return;
            if (vertices.Count < 3) return;
            if (triangles == null) return;
            if (triangles.Count < 3) return;

            _bubbleMesh = new Mesh();
            _bubbleMesh.name = "Speech Bubble";
            _bubbleMesh.vertices = vertices.ToArray();
            _bubbleMesh.triangles = triangles.ToArray();

            Mesh tailMesh = GenerateTail(target);
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