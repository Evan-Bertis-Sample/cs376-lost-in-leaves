using System.Collections;
using System.Collections.Generic;
using LostInLeaves.Notifications.Frontend;
using UnityEngine;

namespace LostInLeaves.Notifications
{
    public static class NotificationBuilder
    {
        /// <summary>
        /// Builds a notification for a phone call
        /// </summary>
        /// <param name="contact"> Who is calling? </param>
        /// <param name="conversationDialoguePath"> The dialogue file path </param>
        /// <param name="phoneCallReactionPath"> The reaction file path (said when the player immediately gets the call) </param>
        /// <param name="pickup"> Whether or not the player picks up (determined by story) </param>
        /// <param name="pickupDelay"> How long the player should wait to pickup </param>
        /// <param name="reactionDelay"> How long the player should wait to react </param>
        /// <param name="startDialogueOnPickup"> Whether or not to start the dialogue when the player picks up (false means start immediately after ring) </param>
        /// <returns></returns>
        public static Notification BuildPhoneCallNotification(PhoneContact contact, string conversationDialoguePath, string phoneCallReactionPath = "",
                                                              bool pickup = true, float pickupDelay = 0f, float reactionDelay = 0f, float postCallDelay = 0f, string postCallConversationPath = "")
        {
            var notification = new Notification($"Incoming call from {contact.Name}")
                .AddProperty("contact", contact)
                .AddProperty("pickup", pickup)
                .AddProperty("pickupDelay", pickupDelay)
                .AddProperty("conversationDialoguePath", conversationDialoguePath)
                .AddProperty("reactionPath", phoneCallReactionPath)
                .AddProperty("reactionDelay", reactionDelay)
                .AddProperty("type", (int)PhoneRenderer.PhoneScreen.Call)
                .AddProperty("postCallDelay", postCallDelay)
                .AddProperty("postCallConversationPath", postCallConversationPath);
            return notification;
        }

        /// <summary>
        /// Builds a notification for a text message
        /// <param name="contact"> Who is texting? </param>
        /// <param name="message"> The text message </param>
        /// <param name="reactionDelay"> How long the player should wait to react </param>
        /// <param name="playerReactionPath"> The reaction file path (said when the player immediately gets the text after delay) </param> 
        /// </summary>
        public static Notification BuildTextMessageNotification(PhoneContact contact, string message, float reactionDelay, string playerReactionPath = "")
        {
            var notification = new Notification($"New message from {contact.Name}: {message}")
                .AddProperty("contact", contact)
                .AddProperty("message", message)
                .AddProperty("reactionPath", playerReactionPath)
                .AddProperty("reactionDelay", reactionDelay)
                .AddProperty("type", (int)PhoneRenderer.PhoneScreen.Text);
            return notification;
        }
    }
}
