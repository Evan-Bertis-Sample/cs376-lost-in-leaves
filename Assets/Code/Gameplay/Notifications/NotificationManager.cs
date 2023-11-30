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
        public NotificationScheduler Scheduler;
        [SerializeField] private bool _debug;
        
        private CoroutineRunner _coroutineRunner => App.Instance.CoroutineRunner;
        private static bool _shouldListen = true;

        public override void OnBoot(App app, Scene startingScene)
        {
            Scheduler = new NotificationScheduler();
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
                Scheduler.HandleNotifications();
                yield return null;
            }
        }

        public void PushNotification(Notification notification, INotificationFrontend frontend)
        {
            if (_debug) Debug.Log($"NotificationManager: Pushing notification: {notification.Body}");

            Scheduler.PushNotification(notification, frontend);
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
