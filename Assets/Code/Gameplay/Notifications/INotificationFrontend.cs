using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LostInLeaves.Notifications
{
    public interface INotificationFrontend
    {
        bool AllowMultipleNotifications { get; set; } // Whether to allow multiple notifications to be displayed at once
        bool MustBeAlone { get; set; } // Whether this notification frontend should be shown alone, even if other notifications are queued
        
        /// <summary>
        /// Called at the start of a series of notifications
        /// Use this to set up any UI elements that will be used for notifications
        /// </summary>
        /// <returns></returns>
        Task BeginNotificationStream();

        /// <summary>
        /// Called whenever a notification should be displayed -- determined by the scheduler
        /// Use this to display the notification
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        Task DisplayNotification(Notification notification);

        /// <summary>
        /// Called at the end of a series of notifications
        /// Use this to clean up any UI elements that were used for notifications
        /// </summary>
        Task EndNotificationStream();
    }

    public abstract class NotificationFrontendObject : ScriptableObject, INotificationFrontend
    {
        public bool AllowMultipleNotifications { get; set; } = false;
        public bool MustBeAlone { get; set; } = false;
        
        public virtual async Task BeginNotificationStream() {}
        public virtual async Task DisplayNotification(Notification notification) {}
        public virtual async Task EndNotificationStream() {}
    }
}
