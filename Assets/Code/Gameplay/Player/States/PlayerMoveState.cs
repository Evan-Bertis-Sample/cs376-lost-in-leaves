using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlayerContext = LostInLeaves.Player.PlayerController.PlayerContext;

namespace LostInLeaves.Player
{
    [CreateAssetMenu(menuName = "Lost In Leaves/States/Player/Move")]
    public class PlayerMoveState : PlayerState
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private AnimationCurve _accelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float _accelerationTime = 0.5f;

        private float _accelerationTimer = 0f;

        public override void OnStateEnter()
        {
            _accelerationTimer = 0f;
        }

        public override void OnLogic(PlayerContext context)
        {
            float t = Mathf.InverseLerp(0f, _accelerationTime, _accelerationTimer);
            t = Mathf.Clamp01(t);

            float acceleration = _accelerationCurve.Evaluate(t);
            _accelerationTimer += Time.deltaTime;
            float moveSpeed = _moveSpeed * acceleration;

            Vector3 movement = new Vector3(context.MovementInput.x, 0f, context.MovementInput.y).normalized * moveSpeed;
            context.Rigidbody.velocity = new Vector3(movement.x, context.Rigidbody.velocity.y, movement.z);
        }
    }
}
