using UnityEditor;
using LostInLeaves.Notifications;
using System.Collections.Generic;
using CurlyCore;
using CurlyEditor;

namespace LostInLeavesEditor.Notifications
{
    [CustomEditor(typeof(NotificationManager))]
    public class NotificationManagerEditor : Editor
    {
        private Dictionary<NotificationManager, NotificationSchedulerDrawer> _schedulerDrawers = new Dictionary<NotificationManager, NotificationSchedulerDrawer>();

        // force redraw
        public override bool RequiresConstantRepaint()
        {
            return true;
        }
        
        public override void OnInspectorGUI()
        {
            NotificationManager manager = target as NotificationManager;

            if (manager.Scheduler == null)
            {
                DrawUnableToDraw();
                return;
            }

            if (_schedulerDrawers.ContainsKey(manager) == false)
            {
                _schedulerDrawers.Add(manager, new NotificationSchedulerDrawer(manager.Scheduler));
            }

            _schedulerDrawers[manager].Render();

            EditorGUILayout.Space(StyleCollection.DoubleSpace);

            EditorGUILayout.BeginVertical("Box");
            DrawManagerEditor();
            EditorGUILayout.EndVertical();

        }

        private void DrawUnableToDraw()
        {
            EditorGUILayout.LabelField("Unable to draw NotificationManager, please enter playmode", EditorStyles.boldLabel);
        }

        private void DrawManagerEditor()
        {
            base.OnInspectorGUI();
        }
    }
}
