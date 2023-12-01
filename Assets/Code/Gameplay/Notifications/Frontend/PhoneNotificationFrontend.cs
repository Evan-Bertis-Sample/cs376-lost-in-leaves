using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LostInLeaves.Notifications;
using System.Threading.Tasks;
using DG.Tweening;

namespace LostInLeaves.Notifications.Frontend
{
    [CreateAssetMenu(menuName = "Lost In Leaves/Notifications/Frontend/Phone Notification Frontend", order = 0, fileName = "phone-notification-frontend")]
    public class PhoneNotificationFrontend : NotificationFrontendObject
    {
        [field: Header("Phone Options")]
        [field: SerializeField] private PhoneRenderer _phonePrefab;
        [field: SerializeField] private Vector2 _phoneSpawnPosition; // hidden off screen
        [field: SerializeField] private Vector2 _phoneHidePosition; // slightly off screen
        [field: SerializeField] private Vector2 _phoneShowPosition; // on screen when phone is active
        [field: SerializeField] private AnimationCurve _phoneAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [field: SerializeField] private float _phoneAnimationSpeed = 1f;

        private Canvas _rootCanvas;
        private PhoneRenderer _phoneInstance;
        private RectTransform _phoneRectTransform;
        private Tween _phoneMotionTween;

        public override async Task BeginNotificationStream()
        {
            if (_phoneInstance == null)
            {
                SpawnPhone();
            }
            // move the phone to the hide position
            await MovePhone(_phoneHidePosition);
        }

        public override async Task DisplayNotification(Notification notification)
        {
            // move the phone to the show position
            await MovePhone(_phoneShowPosition);

            // display the notification
            _phoneInstance.RenderNotification(notification);

            // wait for the notification to be dismissed
            float notificationDuration = notification.Duration;
            await Task.Delay((int)(notificationDuration * 1000));
        }

        public override async Task EndNotificationStream()
        {
            // move the phone to the hide position
            await MovePhone(_phoneHidePosition);
        }

        private void SpawnPhone()
        {
            // find the canvas tagged with GameHUD
            if (_rootCanvas == null)
            {
                _rootCanvas = GameObject.FindGameObjectWithTag("GameHUD").GetComponent<Canvas>();
            }

            if (_rootCanvas == null)
            {
                Debug.LogError("PhoneNotificationFrontend: Could not find GameHUD canvas");
                return;
            }

            Debug.Log("PhoneNotificationFrontend: Spawning phone");
            _phoneInstance = Instantiate(_phonePrefab);
            _phoneRectTransform = _phoneInstance.GetComponent<RectTransform>();
            _phoneRectTransform.SetParent(_rootCanvas.transform, false);

            if (_phoneRectTransform == null)
            {
                Debug.LogError("PhoneNotificationFrontend: Could not find RectTransform on phone instance");
                return;
            }

            _phoneRectTransform.anchoredPosition = _phoneSpawnPosition;
        }

        private async Task MovePhone(Vector2 targetPosition)
        {
            if (_phoneMotionTween != null)
            {
                _phoneMotionTween.Kill();
            }

            float distance = Vector2.Distance(_phoneRectTransform.anchoredPosition, targetPosition);
            float duration = distance / _phoneAnimationSpeed;

            _phoneMotionTween = _phoneRectTransform.DOAnchorPos(targetPosition, duration).SetEase(_phoneAnimationCurve);
            await _phoneMotionTween.AsyncWaitForCompletion();
        }
    }
}
