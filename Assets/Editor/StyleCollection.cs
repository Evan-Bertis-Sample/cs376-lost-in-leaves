using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CurlyEditor
{
    public sealed class StyleCollection
    {
        public static float StandardSpace = 5f;
        public static float DoubleSpace => StandardSpace * 2;

        public static GUIStyle Button = "Button";
        public static GUIStyle ButtonDown
        {
            get
            {
                GUIStyle temp = new GUIStyle(Button);
                temp.normal.background = temp.active.background;
                return temp;
            }
        }
    }
}