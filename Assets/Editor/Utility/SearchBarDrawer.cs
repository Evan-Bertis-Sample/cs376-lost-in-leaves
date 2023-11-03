using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CurlyEditor.Utility
{
    public abstract class SearchBarDrawer : PropertyDrawer
    {
        protected float _standardPropertyHeight => EditorGUIUtility.singleLineHeight;
        protected float _standardSpace = 6f;
        protected string _propertyValue = "";

        private bool _initialized = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!_initialized)
            {
                _propertyValue = property.stringValue;
                _initialized = true;
            }

            float buttonWidth = _standardPropertyHeight * 2f;
            float buttonX = position.xMax - buttonWidth;
            Rect buttonPosition = new Rect(buttonX, position.y, buttonWidth, _standardPropertyHeight);

            Rect propertyPosition = new Rect(position.x, position.y, position.width - buttonWidth - _standardSpace, _standardPropertyHeight);
            property.stringValue = _propertyValue;
            EditorGUI.PropertyField(propertyPosition, property, new GUIContent(property.displayName));

            if (GUI.Button(buttonPosition, new GUIContent(EditorGUIUtility.IconContent("Search On Icon"))))
            {
                ButtonClicked(buttonPosition);
            }
        }

        protected abstract void ButtonClicked(Rect buttonPosition);
    }
}