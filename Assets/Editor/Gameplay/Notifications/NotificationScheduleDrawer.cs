using UnityEngine;
using UnityEditor;
using LostInLeaves.Notifications;
using System.Collections.Generic;

namespace LostInLeavesEditor.Notifications
{
    public class NotificationScheduleDrawer
    {
        private NotificationScheduler.NotificationSchedule _schedule;
        private Dictionary<INotificationFrontend, bool> _showQueuedNotifications = new Dictionary<INotificationFrontend, bool>();

        public NotificationScheduleDrawer(NotificationScheduler.NotificationSchedule schedule)
        {
            _schedule = schedule;
        }

        public void Render()
        {
            // Heading
            GUILayout.Label("Notification Schedule", EditorStyles.boldLabel);

            // Add other controls as necessary
            DrawQueuedNotifications();
            DrawActiveNotifications();
        }

        private void DrawQueuedNotifications()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Queued Notifications", EditorStyles.boldLabel);

            IReadOnlyDictionary<INotificationFrontend, Queue<Notification>> queuedNotifications = _schedule.GetQueuedNotifications();

            int notificationCount = 0;
            foreach (KeyValuePair<INotificationFrontend, Queue<Notification>> pair in queuedNotifications)
            {
                notificationCount += pair.Value.Count;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.IntField("Frontends", queuedNotifications.Count);
            EditorGUILayout.IntField("Notifications", notificationCount);
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            foreach (KeyValuePair<INotificationFrontend, Queue<Notification>> pair in queuedNotifications)
            {
                EditorGUILayout.BeginVertical("Box");
                if (!_showQueuedNotifications.ContainsKey(pair.Key))
                {
                    _showQueuedNotifications.Add(pair.Key, true);
                }

                _showQueuedNotifications[pair.Key] = EditorGUILayout.Foldout(_showQueuedNotifications[pair.Key], pair.Key.ToString());

                if (_showQueuedNotifications[pair.Key] == false)
                {
                    EditorGUILayout.EndVertical();
                    continue;
                }

                EditorGUI.indentLevel++;
                // Show some information about the frontend
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Toggle("Multiple", pair.Key.AllowMultipleNotifications, GUILayout.ExpandWidth(false));
                EditorGUILayout.Toggle("Alone", pair.Key.MustBeAlone, GUILayout.ExpandWidth(false));
                EditorGUILayout.FloatField("Wait Time", pair.Key.WaitTime);
                EditorGUILayout.EndHorizontal();


                // showed queued notifications
                EditorGUILayout.LabelField("Queued Notifications", EditorStyles.boldLabel);
                foreach (Notification notification in pair.Value)
                {
                    EditorGUILayout.TextField(notification.Body);
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();
        }

        private void DrawActiveNotifications()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Active Notifications", EditorStyles.boldLabel);

            IReadOnlyDictionary<INotificationFrontend, List<Notification>> activeNotifications = _schedule.GetActiveNotifications();

            foreach (KeyValuePair<INotificationFrontend, List<Notification>> pair in activeNotifications)
            {
                EditorGUILayout.LabelField(pair.Key.ToString(), EditorStyles.boldLabel);

                foreach (Notification notification in pair.Value)
                {
                    EditorGUILayout.LabelField(notification.ToString());
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawDictionaryCollection(IReadOnlyDictionary<INotificationFrontend, ICollection<Notification>> dictionaryCollection)
        {
            foreach (KeyValuePair<INotificationFrontend, ICollection<Notification>> pair in dictionaryCollection)
            {
                EditorGUILayout.LabelField(pair.Key.ToString(), EditorStyles.boldLabel);

                foreach (Notification notification in pair.Value)
                {
                    EditorGUILayout.LabelField(notification.ToString());
                }
            }
        }
    }
}