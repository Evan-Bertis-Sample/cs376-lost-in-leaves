using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

using CurlyCore.CurlyApp;
using CurlyUtility;

namespace CurlyCore.Debugging
{
    [CreateAssetMenu(menuName = "Curly/Core/Logger", fileName = "Logger")]
    public class GroupLogger : RuntimeScriptableObject
    {
        [field: Header("File Logging Configuration")]
        [field: SerializeField] public bool LogToFile { get; private set; }
        [field: SerializeField] public int MaxLogCount { get; private set; } = 5;
        [field: SerializeField, DirectoryPath(true)] public string LoggingPath { get; private set; }

        [field: Header("Logging Configuration")]
        [field: SerializeField, FilePath] public string LoggingEnumPath { get; private set; }
        [field: SerializeField] public List<LoggingGroup> LoggingGroups { get; private set; }

        private bool _loggedThisSession;
        [SerializeField, TextArea] private string _logContents;

        private const string _LAST_LOG_FILE = "last_session.txt";
        private const string _LOG_META = "log_meta.txt";

        [SerializeField, HideInInspector] public List<LoggingGroup> Groups = new List<LoggingGroup>();

        // Required groups that must be in the enum for Curly internals -- do not delete
        private readonly LoggingGroup[] _REQUIRED_GROUPS = new LoggingGroup[]
        {
            new LoggingGroup { GroupName = "App", LoggingColor = new Color(0.9716981f, 0.3162602f, 0.3162602f ), EnableLogging = true },
            new LoggingGroup { GroupName = "Gameplay", LoggingColor = new Color(0.7203155f, 0.4963955f, 0.9150943f ), EnableLogging = true },
        };

        public override void OnBoot(App app, Scene scene)
        {
            _loggedThisSession = false;
            Groups = GetGroups();
#if UNITY_EDITOR
            if (LogToFile) SetupLogContents();
#endif
        }

        public void Log(LoggingGroupID type, object message, LogType logType = LogType.Log)
        {
#if UNITY_EDITOR
            _loggedThisSession = true;
            LoggingGroup group = Groups[(int)type];

            if (group.EnableLogging == false) return;

            string logColor = ColorUtility.ToHtmlStringRGB(group.LoggingColor);
            string prefix = $"<b><color=#{logColor}>{group.GroupName}: </color></b>";

            Debug.unityLogger.Log(logType, prefix + message.ToString());

            _logContents += $"{group.GroupName} ({DateTime.Now.ToString("HH:mm:ss")}): {message.ToString()}\n";
#else

            return;

#endif
        }

        public List<LoggingGroup> GetGroups()
        {
            // Concatenate LoggingGroups with _REQUIRED_GROUPS, then remove duplicates by GroupName (case-insensitive)
            List<LoggingGroup> combinedGroups = LoggingGroups
                .Union(_REQUIRED_GROUPS, new LoggingGroupNameComparer())
                .ToList();

            return combinedGroups;
        }

        private class LoggingGroupNameComparer : IEqualityComparer<LoggingGroup>
        {
            public bool Equals(LoggingGroup x, LoggingGroup y)
            {
                return string.Equals(x.GroupName, y.GroupName, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(LoggingGroup obj)
            {
                return obj.GroupName.ToUpperInvariant().GetHashCode();
            }
        }

        public override void OnQuit(App app, Scene scene)
        {
            Log(LoggingGroupID.APP, "Finishing Logging Session");
#if UNITY_EDITOR
            if (LogToFile == false || _loggedThisSession == false) return;

            string lastLogPath = $"{LoggingPath}/{_LAST_LOG_FILE}";
            if (File.Exists(lastLogPath))
            {
                // Move contents from old last log to new last log
                string oldContents = File.ReadAllText(lastLogPath);
                FileInfo fileInfo = new FileInfo(lastLogPath);
                string timeStamp = fileInfo.LastWriteTime.ToString();
                timeStamp = timeStamp.Replace(":", "-");
                timeStamp = timeStamp.Replace(" ", "_");
                timeStamp = timeStamp.Replace(@"\", "-");
                timeStamp = timeStamp.Replace("/", "-");
                string newPath = $"{LoggingPath}/prev_session_{timeStamp}.txt";

                Queue<string> pastLogs = GetLogMeta();

                if (pastLogs.Count >= MaxLogCount)
                {
                    // We need to delete a file
                    string delete = pastLogs.Dequeue();
                    if (File.Exists(delete))
                    {
                        Log(LoggingGroupID.APP, $"Deleting Log File '{delete}'");
                        File.Delete(delete);
                    }
                }

                /// Add this data to the pastlogs
                pastLogs.Enqueue(newPath);
                // Serialize this in our metadata
                SaveMetadata(pastLogs);

                Log(LoggingGroupID.APP, "Moving old logging contents to new file");
                using (File.Create(newPath)) { }
                File.WriteAllText(newPath, oldContents);
            }
            else
            {
                // We need to create a file
                using (File.Create(lastLogPath)) { }
            }

            File.WriteAllText(lastLogPath, _logContents);
#endif
        }

        private void SetupLogContents()
        {
            DateTime time = DateTime.Now;
            _logContents = $"LOG SESSION ({time})\n";
            _logContents += string.Concat(System.Linq.Enumerable.Repeat("-", 40));
            _logContents += "\n";
        }

        private Queue<string> GetLogMeta()
        {
            string metaPath = $"{LoggingPath}/{_LOG_META}";
            if (!File.Exists(metaPath)) return new Queue<string>();

            IEnumerable<string> metadata = File.ReadLines(metaPath);

            Queue<string> queuemeta = new Queue<string>();
            foreach (string path in metadata) queuemeta.Enqueue(path);
            return queuemeta;
        }

        private void SaveMetadata(Queue<string> data)
        {
            string metaPath = $"{LoggingPath}/{_LOG_META}";
            string metadata = "";

            foreach (string path in data)
            {
                metadata += path + "\n";
            }

            File.WriteAllText(metaPath, metadata);
        }
    }

}