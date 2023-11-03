using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using CurlyCore.Debugging;

namespace CurlyEditor.Core
{
    [CustomEditor(typeof(GroupLogger))]
    public class LoggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GroupLogger logger = target as GroupLogger;
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(logger.LoggingEnumPath);
            if (asset == null) return;
            if (GUILayout.Button("Generate Logging Groups")) GenerateLoggerGroups(logger, logger.LoggingEnumPath);
        }

        private void GenerateLoggerGroups(GroupLogger logger, string path)
        {
            List<LoggingGroup> groups = logger.GetGroups();
            logger.Groups = groups;

            string enumValues = "";
            for (int i = 0; i < groups.Count; i++)
            {
                LoggingGroup group = groups[i];
                string value = "\t\t";
                value += group.GroupName.ToUpper();
                if (i != groups.Count - 1) value += ",\n";
                else value += "\n";

                enumValues += value;
            }

            string content =
                "namespace CurlyCore.Debugging\n" +
                "{\n" +
                    "\tpublic enum LoggingGroupID\n" +
                    "\t{\n" +
                        $"{enumValues}" +
                    "\t}\n" +
                "}";

            File.WriteAllText(path, content);
            AssetDatabase.Refresh();
        }
    }
}