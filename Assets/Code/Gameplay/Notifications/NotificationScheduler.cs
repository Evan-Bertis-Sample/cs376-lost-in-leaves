using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace LostInLeaves.Notifications
{
    /// <summary>
    /// A class that manages the scheduling and display of notifications
    /// It tries to display notification in the order they are received, but will not display a notification if the previous one has not been dismissed
    /// It also tries to ensure that NotificationFrontends stay "open" for the duration of a series of notifications
    /// </summary>
    public class NotificationScheduler
    {
        internal class NotificationSchedule
        {
            private Dictionary<INotificationFrontend, Queue<Notification>> _queuedNotificationsByFrontend = new Dictionary<INotificationFrontend, Queue<Notification>>();
            private Dictionary<INotificationFrontend, List<Notification>> _activeNotificationsByFrontend = new Dictionary<INotificationFrontend, List<Notification>>();

            /// <summary>
            /// Queues a notification to be displayed by the given frontend
            /// </summary>
            /// <param name="notification"> The notification to queue </param>
            /// <param name="frontend"> The frontend that this notifiation uses </param>
            public void QueueNotification(Notification notification, INotificationFrontend frontend)
            {
                if (!_queuedNotificationsByFrontend.ContainsKey(frontend))
                {
                    _queuedNotificationsByFrontend.Add(frontend, new Queue<Notification>());
                }
                _queuedNotificationsByFrontend[frontend].Enqueue(notification);
            }

            public List<(Notification, INotificationFrontend)> GetNext()
            {
                List<(Notification, INotificationFrontend)> notificationTuples = PeekNextSet();

                if (notificationTuples.Count == 0)
                {
                    return notificationTuples;
                }

                // Okay, so now we should promote the queued notifications to active notifications
                foreach (var (notification, frontend) in notificationTuples)
                {
                    DeployNotification(frontend);
                }

                return notificationTuples;
            }

            public List<(Notification, INotificationFrontend)> PeekNextSet()
            {
                // Check the active notifications first, and if there are any that must be alone, return none
                foreach (var (frontend, notificationList) in _activeNotificationsByFrontend)
                {
                    if (frontend.MustBeAlone && notificationList.Count > 0)
                    {
                        return new List<(Notification, INotificationFrontend)>();
                    }
                }   

                // there are no active notification that must be alone, check if there is a frontend with queued notifications that must be alone
                foreach (var (frontend, notificationQueue) in _queuedNotificationsByFrontend)
                {
                    if (frontend.MustBeAlone && notificationQueue.Count > 0)
                    {
                        // grab the notifications from the queue
                        List<Notification> notifications = GetQueuedNotifications(frontend);

                        // return tuples of the notifications and the frontend
                        List<(Notification, INotificationFrontend)> notificationTuples = new List<(Notification, INotificationFrontend)>();
                        foreach (var notification in notifications)
                        {
                            notificationTuples.Add((notification, frontend));
                        }

                        return notificationTuples;
                    }
                }

                // there are no active or queued notifications that must be alone, check if there are any active notifications
                // and return all of them in the form of tuples
                List<(Notification, INotificationFrontend)> activeNotificationTuples = new List<(Notification, INotificationFrontend)>();

                foreach (var (frontend, notificationList) in _activeNotificationsByFrontend)
                {
                    List<Notification> availableNotifications = GetQueuedNotifications(frontend);

                    foreach (var notification in availableNotifications)
                    {
                        activeNotificationTuples.Add((notification, frontend));
                    }
                }

                return activeNotificationTuples;
            }

            private List<Notification> GetQueuedNotifications(INotificationFrontend frontend)
            {
                List<Notification> notifications = new List<Notification>();

                if (!_queuedNotificationsByFrontend.ContainsKey(frontend) || _queuedNotificationsByFrontend[frontend].Count == 0)
                {
                    return notifications;
                }

                if (frontend.AllowMultipleNotifications)
                {
                    // Grab all the queued notifications for this frontend
                    notifications.AddRange(_queuedNotificationsByFrontend[frontend]);
                }
                else
                {
                    // Grab the first queued notification for this frontend
                    notifications.Add(_queuedNotificationsByFrontend[frontend].Peek());
                }

                return notifications;
            }

            public Notification PeekNextNotification(INotificationFrontend frontend)
            {
                if (!_queuedNotificationsByFrontend.ContainsKey(frontend))
                {
                    return null;
                }
                return _queuedNotificationsByFrontend[frontend].Peek();
            }

            public Notification DeployNotification(INotificationFrontend frontend)
            {
                if (!_queuedNotificationsByFrontend.ContainsKey(frontend))
                {
                    return null;
                }

                if (!_activeNotificationsByFrontend.ContainsKey(frontend))
                {
                    _activeNotificationsByFrontend.Add(frontend, new List<Notification>());
                }

                Notification notification = _queuedNotificationsByFrontend[frontend].Dequeue();
                _activeNotificationsByFrontend[frontend].Add(notification);

                return notification;
            }

            public void CloseActiveNotification(INotificationFrontend frontend, Notification notification)
            {
                if (!_activeNotificationsByFrontend.ContainsKey(frontend))
                {
                    return;
                }
                _activeNotificationsByFrontend[frontend].Remove(notification);
            }

            public void RemoveNotification(INotificationFrontend frontend)
            {
                if (!_queuedNotificationsByFrontend.ContainsKey(frontend))
                {
                    return;
                }
                _queuedNotificationsByFrontend[frontend].Dequeue();
            }

            public Queue<Notification> GetQueuedNotificationsForFrontend(INotificationFrontend frontend)
            {
                if (!_queuedNotificationsByFrontend.ContainsKey(frontend))
                {
                    return new Queue<Notification>();
                }
                return _queuedNotificationsByFrontend[frontend];
            }

            public List<Notification> GetActiveNotificationsForFrontend(INotificationFrontend frontend)
            {
                if (!_activeNotificationsByFrontend.ContainsKey(frontend))
                {
                    return new List<Notification>();
                }
                return _activeNotificationsByFrontend[frontend];
            }

            public List<Notification> GetAllNotificationsForFrontend(INotificationFrontend frontend)
            {
                List<Notification> notifications = new List<Notification>();
                notifications.AddRange(GetQueuedNotificationsForFrontend(frontend));
                notifications.AddRange(GetActiveNotificationsForFrontend(frontend));
                return notifications;
            }

            public bool HasQueuedNotificationsForFrontend(INotificationFrontend frontend)
            {
                return GetQueuedNotificationsForFrontend(frontend).Count > 0;
            }

        }

        private NotificationSchedule _notificationSchedule = new NotificationSchedule();

        public void PushNotification(Notification notification, INotificationFrontend frontend)
        {
            _notificationSchedule.QueueNotification(notification, frontend);
        }

        public void DisplayNotifications()
        {

        }
    }
}