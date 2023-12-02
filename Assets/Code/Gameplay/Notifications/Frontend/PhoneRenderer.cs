using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LostInLeaves.Notifications.Frontend
{
    public class PhoneRenderer : MonoBehaviour
    {
        public enum PhoneScreen { Call, Text }

        [System.Serializable]
        internal class PhoneScreenBase
        {
            public GameObject RootObject;
        }

        // Screens
        [System.Serializable]
        internal class PhoneCallReferences : PhoneScreenBase
        {
            public Image ContactImage;
            public TextMeshProUGUI ContactName;
            public TextMeshProUGUI ContactNumber;
        }

        [System.Serializable]
        internal class PhoneTextReferences : PhoneScreenBase
        {
            public Image ContactImage;
            public TextMeshProUGUI ContactName;
            public TextMeshProUGUI ContactNumber;
            public TextMeshProUGUI MessageBody;
        }

        [SerializeField] private PhoneCallReferences _phoneCallScreenObjects;
        [SerializeField] private PhoneTextReferences _phoneTextScreenObjects;

        private List<PhoneScreenBase> _phoneScreens = new List<PhoneScreenBase>();

        private void Awake()
        {
            // allows us to iterate through all screens
            _phoneScreens.Add(_phoneCallScreenObjects);
            _phoneScreens.Add(_phoneTextScreenObjects);
        }

        public void RenderNotification(Notification notification)
        {
            PhoneScreen screen = notification.GetProperty<PhoneScreen>("type");
            switch (screen)
            {
                case PhoneScreen.Call:
                    RenderCallNotification(notification);
                    break;
                case PhoneScreen.Text:
                    RenderTextNotification(notification);
                    break;
            }
        }

        private void RenderCallNotification(Notification notification)
        {
            PhoneContact contact = notification.GetProperty<PhoneContact>("contact");
            _phoneCallScreenObjects.ContactImage.sprite = contact.ContactPhoto;
            _phoneCallScreenObjects.ContactName.text = contact.Name;
            _phoneCallScreenObjects.ContactNumber.text = contact.PhoneNumber;

            EnableScreen(PhoneScreen.Call);
        }

        private void RenderTextNotification(Notification notification)
        {
            PhoneContact contact = notification.GetProperty<PhoneContact>("contact");
            _phoneTextScreenObjects.ContactImage.sprite = contact.ContactPhoto;
            _phoneTextScreenObjects.ContactName.text = contact.Name;
            _phoneTextScreenObjects.ContactNumber.text = contact.PhoneNumber;
            // _phoneTextScreenObjects.MessageBody.text = notification.GetProperty<string>("message");

            EnableScreen(PhoneScreen.Text);
        }

        private void EnableScreen(PhoneScreen screen)
        {
            foreach (PhoneScreenBase phoneScreen in _phoneScreens)
            {
                phoneScreen?.RootObject?.SetActive(false);
            }

            _phoneScreens[(int)screen].RootObject.SetActive(true);
        }
    }
}
