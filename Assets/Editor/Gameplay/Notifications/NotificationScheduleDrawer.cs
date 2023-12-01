using UnityEngine;
using UnityEditor;
using LostInLeaves.Notifications;
using System.Collections.Generic;
using System.Linq;

namespace LostInLeavesEditor.Notifications
{
    public class NotificationScheduleDrawer
    {
        private NotificationScheduler.NotificationSchedule _schedule;

        // Stores the status of each tab
        private Dictionary<INotificationFrontend, bool> _showQueuedNotifications = new Dictionary<INotificationFrontend, bool>();
        private Dictionary<INotificationFrontend, bool> _showActiveNotifications = new Dictionary<INotificationFrontend, bool>();
        private Dictionary<INotificationFrontend, bool> _showNextSet = new Dictionary<INotificationFrontend, bool>();

        public NotificationScheduleDrawer(NotificationScheduler.NotificationSchedule schedule)
        {
            _schedule = schedule;
        }

        public void Render()
        {
            // Heading
            GUILayout.Label("Notification Schedule", EditorStyles.boldLabel);

            // Add other controls as necessary
            DrawNotificationGroup("Queued Notifications", _showQueuedNotifications, _schedule.GetQueuedNotifications());
            DrawNotificationGroup("Active Notifications", _showActiveNotifications, _schedule.GetActiveNotifications());

            // Get the next set and convert it into a dictionary of lists
            List<(Notification, INotificationFrontend)> nextSet = _schedule.PeekNextSet();
            Dictionary<INotificationFrontend, List<Notification>> nextSetDictionary = new Dictionary<INotificationFrontend, List<Notification>>();

            foreach ((Notification, INotificationFrontend) pair in nextSet)
            {
                if (nextSetDictionary.ContainsKey(pair.Item2) == false)
                {
                    nextSetDictionary.Add(pair.Item2, new List<Notification>());
                }

                nextSetDictionary[pair.Item2].Add(pair.Item1);
            }

            DrawNotificationGroup("Next Set", _showNextSet, nextSetDictionary);
        }

        private void DrawNotificationGroup<T>(string title, Dictionary<INotificationFrontend, bool> tabStatus, Dictionary<INotificationFrontend, T> notifications) where T : IEnumerable<Notification>
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            DrawQueuedNotifications(notifications, tabStatus);

            EditorGUILayout.EndVertical();

        }

        private void DrawQueuedNotifications<T>(IReadOnlyDictionary<INotificationFrontend, T> queuedNotifications, Dictionary<INotificationFrontend, bool> tabStatus) where T : IEnumerable<Notification>
        {
            int notificationCount = 0;
            foreach (KeyValuePair<INotificationFrontend, T> pair in queuedNotifications)
            {
                notificationCount += pair.Value.Count();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.IntField("Frontends", queuedNotifications.Count);
            EditorGUILayout.IntField("Notifications", notificationCount);
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            foreach (KeyValuePair<INotificationFrontend, T> pair in queuedNotifications)
            {
                EditorGUILayout.BeginVertical("Box");

                if (tabStatus.ContainsKey(pair.Key) == false)
                {
                    tabStatus.Add(pair.Key, true);
                }

                tabStatus[pair.Key] = EditorGUILayout.Foldout(tabStatus[pair.Key], pair.Key.ToString());

                if (tabStatus[pair.Key] == false)
                {
                    EditorGUILayout.EndVertical();
                    continue;
                }

                EditorGUI.indentLevel++;
                // Show some information about the frontend
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Toggle("Multiple", pair.Key.AllowMultipleNotifications);
                EditorGUILayout.Space();
                EditorGUILayout.Toggle("Alone", pair.Key.MustBeAlone);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.FloatField("Wait Time", pair.Key.WaitTime);

                // showed queued notifications
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Queued Notifications", EditorStyles.boldLabel);
                foreach (Notification notification in pair.Value)
                {
                    EditorGUILayout.TextField(notification.Body);
                }
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
            EditorGUI.indentLevel--;
        }
    }
}