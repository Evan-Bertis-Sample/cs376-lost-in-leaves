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
            DrawFrontendStates();
            DrawThreadStates();
        }

        private void DrawFrontendStates()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Frontend States", EditorStyles.boldLabel);
            List<INotificationFrontend> notificationFrontends = _scheduler.NotificationFrontends;
            foreach(INotificationFrontend frontend in notificationFrontends)
            {
                NotificationScheduler.NotificationFrontendState state = _scheduler.GetFrontendState(frontend);

                EditorGUILayout.BeginHorizontal("Box");
                EditorGUILayout.LabelField(frontend.ToString());
                GUILayout.FlexibleSpace();
                (GUIContent content, Color statusColor) = GetStateContent(state);
                float width = EditorStyles.label.CalcSize(content).x;
                GUI.color = statusColor;
                EditorGUILayout.LabelField(content, GUILayout.Width(width));
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawThreadStates()
        {
            EditorGUILayout.IntField("Threads Allocated", _scheduler.ThreadCount);
        }

        private (GUIContent, Color) GetStateContent(NotificationScheduler.NotificationFrontendState state)
        {
            string iconName = "";
            string statusText = "";
            Color statusColor = Color.white;

            switch (state)
            {
                case NotificationScheduler.NotificationFrontendState.Open:
                    iconName = "d_winbtn_mac_max";
                    statusText = "Open";
                    statusColor = Color.yellow;
                    break;
                case NotificationScheduler.NotificationFrontendState.Closed:
                    iconName = "d_winbtn_mac_close";
                    statusText = "Closed";
                    statusColor = Color.red;
                    break;
                case NotificationScheduler.NotificationFrontendState.Displaying:
                    iconName = "d_winbtn_mac_inact";
                    statusText = "Displaying";
                    statusColor = Color.green;
                    break;
            }

            // Create the guicontent
            Texture icon = EditorGUIUtility.IconContent(iconName).image;
            GUIContent content = new GUIContent(statusText, icon);

            return (content, statusColor);
        }
    }
}
