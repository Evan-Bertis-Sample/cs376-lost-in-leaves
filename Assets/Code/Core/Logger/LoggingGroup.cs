using UnityEngine;

namespace CurlyCore.Debugging
{
    [System.Serializable]
    public struct LoggingGroup
    {
        public string GroupName;
        public Color LoggingColor;
        public bool EnableLogging;

        public LoggingGroup(string groupName, Color loggingColor, bool enableLogging = true)
        {
            GroupName = groupName;
            LoggingColor = loggingColor;
            EnableLogging = enableLogging;
        }
    }
}