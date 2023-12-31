using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LostInLeaves.Notifications;
using System.Threading.Tasks;
using DG.Tweening;
using LostInLeaves.Dialogue;
using CurlyCore.CurlyApp;

namespace LostInLeaves.Notifications.Frontend
{
    [RequireComponent(typeof(DialogueEmitter))]
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

        private void BeginPhoneShake()
        {
            // vibrate the rect transform
            if (_phoneMotionTween != null)
            {
                _phoneMotionTween.Kill();
            }

            // shake the phone rect transform forever, and repeat when finished and with delay
            _phoneMotionTween = _phoneRectTransform.DOShakeAnchorPos(1f, _phoneShakeStrength, _phoneShakeVibrato, _phoneShakeRandomness, true);
            _phoneMotionTween.SetLoops(-1, LoopType.Restart);
        }

        private void EndPhoneShake()
        {
            if (_phoneMotionTween != null)
            {
                _phoneMotionTween.Kill();
                // reset the position of the phone
                _phoneRectTransform.anchoredPosition = _phoneShowPosition;
            }
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
            // visualize the phone ringing
            string reactionPath = notification.GetProperty<string>("reactionPath");
            float reactionDelay = notification.GetProperty<float>("reactionDelay");

            float timeForReaction = await PhoneReaction(reactionPath, reactionDelay);

            float pickupDelay = notification.GetProperty<float>("pickupDelay");
            Debug.Log($"PhoneNotificationFrontend: Pickup delay is {pickupDelay}");

            float timeToWait = pickupDelay - timeForReaction;
            Debug.Log($"PhoneNotificationFrontend: Waiting {timeToWait} seconds to pickup");
            
            if (timeToWait > 0)
            {
                await Task.Delay((int)(timeToWait * 1000));
            }

            bool pickup = notification.GetProperty<bool>("pickup");
            if (pickup)
            {
                _phoneInstance.AcceptCall();
                DialogueEmitter player = DialogueRunner.GetDialogueEmitter("Player");
                string conversationDialoguePath = notification.GetProperty<string>("conversationDialoguePath");
                await DialogueRunner.RunDialogue(player, conversationDialoguePath, false);
            }
            else
            {
                _phoneInstance.DeclineCall();
            }

            float postCallDelay = notification.GetProperty<float>("postCallDelay");
            string postCallConversationPath = notification.GetProperty<string>("postCallConversationPath");

            if (postCallDelay > 0)
            {
                await Task.Delay((int)(postCallDelay * 1000));
            }

            if (!string.IsNullOrEmpty(postCallConversationPath))
            {
                DialogueEmitter player = DialogueRunner.GetDialogueEmitter("Player");
                await DialogueRunner.RunDialogue(player, postCallConversationPath, false);
            }
        }

        private async Task<float> PhoneReaction(string reactionPath, float reactionDelay)
        {
            Debug.Log($"PhoneNotificationFrontend: Reacting to call");
            if (!string.IsNullOrEmpty(reactionPath))
            {
                float start = Time.time;
                Debug.Log($"PhoneNotificationFrontend: Playing reaction: {reactionPath}");
                DialogueEmitter playerEmitter = DialogueRunner.GetDialogueEmitter("Player");
                // await the the dialogue coroutine
                BeginPhoneShake();
                await Task.Delay((int)(reactionDelay * 1000));
                await DialogueRunner.RunDialogue(playerEmitter, reactionPath, false);
                EndPhoneShake();

                float end = Time.time;
                Debug.Log($"PhoneNotificationFrontend: Finished reaction: {reactionPath}");
                return end - start; // return the time it took to play the reaction
            }

            return 0;
        }
        
    }
}
