using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CurlyUtility
{
    public static class UIUtility
    {
        public static GameObject SpawnTextBoxUI(Vector3 worldPosition, TMPDrawer drawer, Canvas canvas, out TextMeshProUGUI tmproUI, out RectTransform rectTransform, string text = "")
        {
            Debug.Log("spawning text box");
            GameObject textObject = new GameObject("Textbox");
            rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.position = WorldToCanvasSpace(worldPosition);
            rectTransform.SetParent(canvas.transform);
            tmproUI = textObject.AddComponent<TextMeshProUGUI>();
            drawer.ApplySettings(tmproUI);
            tmproUI.text = text;
            return textObject;
        }

        public static Button SpawnButton(Vector3 canvasPosition, Vector2 scale, TMPDrawer drawer, string buttonText, Sprite buttonSprite, Canvas canvas = null)
        {
            GameObject newButton = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
            Image buttonImage = newButton.GetComponent<Image>();
            TextMeshProUGUI buttonTextbox = newButton.GetComponentInChildren<TextMeshProUGUI>();

            buttonImage.sprite = buttonSprite;
            buttonImage.SetNativeSize();
            drawer.ApplySettings(buttonTextbox);
            buttonTextbox.text = buttonText;
            RectTransform buttonTransform = newButton.GetComponent<RectTransform>();
            buttonTransform.parent = canvas.transform;
            buttonTransform.anchoredPosition = canvasPosition;
            buttonTransform.sizeDelta *= scale;
            return newButton.GetComponent<Button>();
        }

        public static Vector3 WorldToCanvasSpace(Vector3 position)
        {
            return Camera.main.WorldToScreenPoint(position);
        }

        public static Vector3 CanvasToWorldSpace(Vector3 position)
        {
            return Camera.main.ScreenToViewportPoint(position);
        }
    }
}

