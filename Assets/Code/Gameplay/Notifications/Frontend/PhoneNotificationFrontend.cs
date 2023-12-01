using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LostInLeaves.Notifications;
using System.Threading.Tasks;

namespace LostInLeaves.Notifications.Frontend
{
    [CreateAssetMenu(menuName = "Lost In Leaves/Notifications/Frontend/Phone Notification Frontend", order = 0, fileName = "phone-notification-frontend")]
    public class PhoneNotificationFrontend : NotificationFrontendObject
    {
        [SerializeField] private GameObject _phoneNotificationPrefab;
        
        public override async Task BeginNotificationStream()
        {
            
        }

        public override async Task DisplayNotification(Notification notification)
        {

        }
    }
}
