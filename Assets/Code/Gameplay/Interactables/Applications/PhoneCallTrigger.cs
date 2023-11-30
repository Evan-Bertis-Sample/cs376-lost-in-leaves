using System.Collections;
using System.Collections.Generic;
using LostInLeaves.Notifications;
using UnityEditor;
using UnityEngine;
using CurlyUtility;
using CurlyCore;

namespace LostInLeaves.Interactables
{
    public class PhoneCallTrigger : InteractableObject
    {
        [Header("Notification Setup")]
        [SerializeField] private NotificationFrontendObject _notificationFrontend = null; // required always
        [GlobalDefault] private NotificationManager _notificationManager = null; // required always

        [Header("Phone Call Setup")]
        [SerializeField] private PhoneContact _contactToCall = null; // required always
        [SerializeField, @FilePath] private string _phoneCallReactionPath = "";
        [SerializeField, @FilePath] private string _conversationDialoguePath = ""; // required if pickup is true
        [SerializeField] private bool _pickup = true;
        [SerializeField] private float _pickupDelay = 0f;
        [SerializeField] private float _reactionDelay = 0f;

        private void Start()
        {
            EnsureValidConfiguration();
            DependencyInjector.InjectDependencies(this);
        }

        private bool EnsureValidConfiguration()
        {
            // ensure that we have a valid configuration
            if (_notificationFrontend == null)
            {
                Debug.LogError("PhoneCallTrigger: No notification frontend!");
                return false;
            }

            if (_contactToCall == null)
            {
                Debug.LogError("PhoneCallTrigger: No contact to call!");
                return false;
            }

            if (_pickup && string.IsNullOrEmpty(_conversationDialoguePath))
            {
                Debug.LogError("PhoneCallTrigger: No conversation dialogue path!");
                return false;
            }

            return true;
        }

        protected override void OnInteractStart(Interactor interactor)
        {
            if (!EnsureValidConfiguration()) return;
            // build the notification
            Notification notification = NotificationBuilder.BuildPhoneCallNotification(_contactToCall, _conversationDialoguePath, _phoneCallReactionPath, _pickup, _pickupDelay, _reactionDelay);
            // send the notification
            _notificationManager.PushNotification(notification, _notificationFrontend);
        }
    }
}
