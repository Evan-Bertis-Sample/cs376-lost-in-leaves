using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInLeaves.Clickables
{
    public interface IClickable
    {
        public void OnHover();
        public void OnUnhover();
        public void OnSelect();
        public void OnDeselect();
    }
}
