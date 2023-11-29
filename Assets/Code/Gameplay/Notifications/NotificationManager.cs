using System.Collections;
using System.Collections.Generic;
using CurlyCore;
using CurlyCore.CurlyApp;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace LostInLeaves.Notifications
{
    [CreateAssetMenu(fileName = "NotificationManager", menuName = "Lost In Leaves/Notifications/Notification Manager")]
    public class NotificationManager : RuntimeScriptableObject
    {
        private CoroutineRunner _coroutineRunner => App.Instance.CoroutineRunner;

        private static NotificationScheduler _scheduler;
        private static bool _shouldListen = true;

        public override void OnBoot(App app, Scene startingScene)
        {
            _scheduler = new NotificationScheduler();
            _coroutineRunner.StartCoroutine(Listen());
        }

        public override void OnQuit(App app, Scene scene)
        {
            Stop();
        }

        // Emulates update()
        private IEnumerator Listen()
        {
            while (Application.isPlaying && _shouldListen)
            {
                Debug.Log("Listening for notifications");
                _scheduler.HandleNotifications();
                yield return null;
            }
        }

        public void PushNotification(Notification notification, INotificationFrontend frontend)
        {
            _scheduler.PushNotification(notification, frontend);
        }

        public void Start()
        {
            _shouldListen = true;
            _coroutineRunner.StartCoroutine(Listen());
        }

        public void Stop()
        {
            _shouldListen = false;
        }
    }
}
