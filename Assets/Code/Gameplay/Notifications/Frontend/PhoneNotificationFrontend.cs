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

        // shake options
        [field: Header("Phone Shake Options")]
        [field: SerializeField] private float _phoneShakeStrength = 1f;
        [field: SerializeField] private int _phoneShakeVibrato = 10;
        [field: SerializeField] private float _phoneShakeRandomness = 90f;

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

            Debug.Log("PhoneNotificationFrontend: Beginning notification stream");
        }

        public override async Task DisplayNotification(Notification notification)
        {
            // display the notification
            Debug.Log($"PhoneNotificationFrontend: Displaying notification: {notification}");
            _phoneInstance.RenderNotification(notification);
            // move the phone to the show position

            await MovePhone(_phoneShowPosition);
            PhoneRenderer.PhoneScreen screenType = notification.GetProperty<PhoneRenderer.PhoneScreen>("type");
            Debug.Log($"PhoneNotificationFrontend: Displaying notification on phone screen: {screenType}");
            await HandlePhoneScreen(screenType, notification);

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

        private async Task VibratePhone(float duration)
        {
            // vibrate the rect transform
            if (_phoneMotionTween != null)
            {
                _phoneMotionTween.Kill();
            }

            // shake the phone for the duration
            _phoneMotionTween = _phoneRectTransform.DOShakeAnchorPos(duration, _phoneShakeStrength, _phoneShakeVibrato, _phoneShakeRandomness);
            await _phoneMotionTween.AsyncWaitForCompletion();
        }

        private async Task HandlePhoneScreen(PhoneRenderer.PhoneScreen screenType, Notification notification)
        {
            switch (screenType)
            {
                case PhoneRenderer.PhoneScreen.Call:
                    await HandleCallScreen(notification);
                    break;
                case PhoneRenderer.PhoneScreen.Text:
                    break;
            }
        }

        private async Task HandleCallScreen(Notification notification)
        {
            float pickupDelay = notification.GetProperty<float>("pickupDelay");
            Debug.Log($"PhoneNotificationFrontend: Pickup delay is {pickupDelay}");
            // visualize the phone ringing
            await VibratePhone(pickupDelay);


        }

    }
}
