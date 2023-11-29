using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInLeaves.Notifications
{
    public class Notification 
    {
        public string Title { get; set; } // The title of the notification
        public string Body { get; set; } // The text of the notification
        public float Delay { get; set; } = 0f; // How long to wait before showing the notification
        public float Duration { get; set; } = 5f; // How long to show the notification
        // public bool RequireManualDismissal {get; set;} = false; // Whether the notification has to be dismissed by the user

        public Dictionary<string, object> NotificationData { get; set; } = new Dictionary<string, object>(); // Any additional data you want to pass to the notification

        public Notification(string body)
        {
            Body = body;
        }

        public Notification AddProperty(string key, object value)
        {
            NotificationData.Add(key, value);
            return this;
        }

        public Notification AddProperties(Dictionary<string, object> properties)
        {
            foreach (var property in properties)
            {
                NotificationData.Add(property.Key, property.Value);
            }
            return this;
        }

        public T GetProperty<T>(string key)
        {
            try
            {
                return (T)NotificationData[key];
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError($"Key {key} not found in notification data");
                return default(T);
            }
            catch (InvalidCastException)
            {
                Debug.LogError($"Key {key} is not of type {typeof(T)}");
                return default(T);
            }
        }
    }
}
