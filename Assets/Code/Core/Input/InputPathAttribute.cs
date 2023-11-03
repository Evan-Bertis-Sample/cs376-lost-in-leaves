using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace CurlyCore.Input
{
    public class InputPathAttribute : PropertyAttribute
    {
        public InputManager Manager;

        public InputPathAttribute()
        {
            Manager = GlobalDefaultStorage.GetDefault(typeof(InputManager)) as InputManager;
        }
    }
}
