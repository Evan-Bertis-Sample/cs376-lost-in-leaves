using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LostInLeaves.Notifications.Frontend
{
    [CreateAssetMenu(menuName = "Lost In Leaves/Notifications/Frontend/Debug Notification Frontend", order = 0, fileName = "debug-notification-frontend")]
    public class DebugNotificationFrontend : NotificationFrontendObject
    {
        [SerializeField] private bool _logNotifications = true;

        public override async Task BeginNotificationStream()
        {
            if (_logNotifications) Debug.Log("DebugNotificationFrontend: BeginNotificationStream");
        }

        public override async Task DisplayNotification(Notification notification)
        {
            if (_logNotifications) Debug.Log($"DebugNotificationFrontend: {notification.Body}");
        }

        public override async Task EndNotificationStream()
        {
            if (_logNotifications) Debug.Log("DebugNotificationFrontend: EndNotificationStream");
        }
    }
}
