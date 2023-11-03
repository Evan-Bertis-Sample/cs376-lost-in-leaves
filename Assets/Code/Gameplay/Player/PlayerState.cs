using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CurlyUtility.DSA;
using PlayerContext = LostInLeaves.Player.PlayerController.PlayerContext;

namespace LostInLeaves.Player
{
    public class PlayerState : ScriptableObject, IState<PlayerContext> 
    {
        public virtual void OnStateEnter() {}
        public virtual void OnLogic(PlayerContext context) {}
        public virtual bool IsReady() {return true; }
        public virtual void OnStateExit() {}
    }

}
