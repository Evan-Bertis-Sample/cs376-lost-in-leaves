using System.Collections;
using System.Collections.Generic;
using LostInLeaves.Monolift;
using UnityEngine;
using UnityEditor;

namespace LostInLeavesEditor.Inspectors
{
    [CustomEditor(typeof(MonoliftTrack))]
    public class MonoliftPathEditor : Editor
    {
        private MonoliftTrack _track;

        // state and settings
        private static float _time = 0;
        private static float _ballGizmoSize = 0.5f;
        private static Color _ballGizmoColor = Color.red;
        private static Color _timeGizmoColor = Color.green;
        private static Color _trackGizmoColor = Color.blue;

        private int _addIndex = -1;
        private Vector3 _addPosition = Vector3.zero;

        private int _removeIndex = -1;

        private void OnEnable()
        {
            _track = target as MonoliftTrack;
        }

        public override void OnInspectorGUI()
        {
            if (_track == null)
                _track = target as MonoliftTrack;

            base.OnInspectorGUI();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Track Editor", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            // draw our optons
            _time = EditorGUILayout.Slider("Time", _time, 0, 1);
            _ballGizmoSize = EditorGUILayout.FloatField("Ball Gizmo Size", _ballGizmoSize);
            _ballGizmoColor = EditorGUILayout.ColorField("Ball Gizmo Color", _ballGizmoColor);

            EditorGUI.BeginChangeCheck();
            _timeGizmoColor = EditorGUILayout.ColorField("Time Gizmo Color", _timeGizmoColor);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
            _trackGizmoColor = EditorGUILayout.ColorField("Track Gizmo Color", _trackGizmoColor);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Edit", EditorStyles.boldLabel);

            _addPosition = EditorGUILayout.Vector3Field("Add Position", _addPosition);
            EditorGUILayout.BeginHorizontal();
            _addIndex = EditorGUILayout.IntField("Add At", _addIndex, GUILayout.ExpandWidth(false));

            if (GUILayout.Button("Add Point"))
            {
                Undo.RecordObject(_track, "Add Point");
                EditorUtility.SetDirty(_track);
                _track.AddPoint(_addIndex, _addPosition);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            _removeIndex = EditorGUILayout.IntField("Remove At", _removeIndex, GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Remove Point"))
            {
                Undo.RecordObject(_track, "Remove Point");
                EditorUtility.SetDirty(_track);
                _track.RemovePoint(_addIndex);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        private void OnSceneGUI()
        {
            if (_track == null)
                _track = target as MonoliftTrack;

            List<GameObject> points = _track.Positions;
            for (int i = 0; i < points.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                Handles.color = _ballGizmoColor;
                Handles.SphereHandleCap(0, points[i].transform.position, Quaternion.identity, _ballGizmoSize, EventType.Repaint);
                Vector3 newPoint = Handles.PositionHandle(points[i].transform.position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_track, "Move Point");
                    EditorUtility.SetDirty(_track);
                    _track.SetPoint(i, newPoint);
                }
            }

            // draw the track
            Handles.color = _trackGizmoColor;
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 cur = points[i].transform.position;
                Vector3 next = points[i + 1].transform.position;
                Handles.DrawLine(cur, next);
            }

            if (_track.Loop)
            {
                Vector3 cur = points[points.Count - 1].transform.position;
                Vector3 next = points[0].transform.position;
                Handles.DrawLine(cur, next);
            }

            // draw the time
            Handles.color = _timeGizmoColor;
            Vector3 pos = _track.GetPosition(_time);
            Handles.SphereHandleCap(0, pos, Quaternion.identity, _ballGizmoSize, EventType.Repaint);
        }
    }
}
