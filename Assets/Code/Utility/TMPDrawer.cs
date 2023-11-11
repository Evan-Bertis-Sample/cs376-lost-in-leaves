using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace CurlyUtility
{
    [CreateAssetMenu(menuName = "TMP Drawer")]
    public class TMPDrawer : ScriptableObject
    {
        public TMP_FontAsset font;
        public float fontSize;
        public FontStyles style;
        public bool autoSize;
        public Color vertexColor;
        public WrapMode wrapMode;
        public HorizontalAlignmentOptions horizontalAlignment;
        public VerticalAlignmentOptions verticalAlignment;
        public TextOverflowModes overflow;

        public void ApplySettings(TextMeshProUGUI tm)
        {
            tm.font = font;
            tm.UpdateFontAsset();
            tm.fontSize = fontSize;
            tm.fontStyle = style;
            tm.enableAutoSizing = autoSize;
            tm.color = vertexColor;
            tm.overflowMode = overflow;
            tm.horizontalAlignment = horizontalAlignment;
            tm.verticalAlignment = verticalAlignment;
        }
    }
}
