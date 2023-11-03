using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlayerContext = LostInLeaves.Player.PlayerController.PlayerContext;
namespace LostInLeaves.Player
{
    [CreateAssetMenu(menuName = "Lost In Leaves/States/Player/Idle")]
    public class PlayerIdleState : PlayerState
    {
        [SerializeField] private float _bobSpeed = 1f;
        [SerializeField] private float _bobHeight = 1f;

        private float _bobTimer = 0f;

        public override void OnStateEnter()
        {
            _bobTimer = 0f;
        }

        public override void OnLogic(PlayerContext context)
        {
            _bobTimer += Time.deltaTime * _bobSpeed;
            // context.Rigidbody.position = new Vector3(context.Rigidbody.position.x, Mathf.Sin(_bobTimer) * _bobHeight, context.Rigidbody.position.z);
        }
    }
}
