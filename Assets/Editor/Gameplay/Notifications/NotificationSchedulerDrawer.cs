using System.Collections;
using System.Collections.Generic;
using LostInLeaves.Notifications;
using LostInLeavesEditor.Notifications;
using UnityEditor;
using UnityEngine;
using CurlyEditor;

namespace CurlyCore
{
    public class NotificationSchedulerDrawer : MonoBehaviour
    {
        private NotificationScheduler _scheduler;
        private NotificationScheduleDrawer _scheduleDrawer;

        public NotificationSchedulerDrawer(NotificationScheduler scheduler)
        {
            _scheduler = scheduler;
            _scheduleDrawer = new NotificationScheduleDrawer(_scheduler.Schedule);
        }

        public void Render()
        {
            // Heading
            EditorGUILayout.BeginVertical("Box");
            _scheduleDrawer.Render();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(StyleCollection.DoubleSpace);

            // Heading
            EditorGUILayout.BeginVertical("Box");
            DrawSchedulerFields();
            EditorGUILayout.EndVertical();

            // Add other controls as necessary
        }

        private void DrawSchedulerFields()
        {
            EditorGUILayout.LabelField("Scheduler", EditorStyles.boldLabel);
        }
    }
}
