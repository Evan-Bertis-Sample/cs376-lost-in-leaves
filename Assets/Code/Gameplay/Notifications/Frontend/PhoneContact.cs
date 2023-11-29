using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInLeaves.Notifications
{
    [CreateAssetMenu(fileName = "PhoneContact", menuName = "Lost In Leaves/Notifications/Phone Contact", order = 0)]
    public class PhoneContact : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public Sprite ContactPhoto { get; private set; }
    }
}
