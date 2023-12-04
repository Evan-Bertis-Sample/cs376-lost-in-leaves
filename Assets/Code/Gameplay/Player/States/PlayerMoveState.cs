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
        [SerializeField] private float _maxHeightDifference = 3f;
        [SerializeField] private float _waterLevel = 3f;

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

            Vector3 newVelocity = context.Rigidbody.velocity + movement;
            if (CanMove(context, newVelocity) == false)
            {
                context.Rigidbody.velocity = new Vector3(0f, context.Rigidbody.velocity.y, 0f);
                MoveTowardsPathOfLeastResistance(context, movement);
            }

            else context.Rigidbody.velocity = new Vector3(movement.x, context.Rigidbody.velocity.y, movement.z);
        }

        private bool CanMove(PlayerContext context, Vector3 velocity)
        {
            // Grab the terrain and make sure where we will end up isn't too steep
            Terrain terrain = Terrain.activeTerrain;
            Vector3 position = context.Rigidbody.position;
            Vector3 newPosition = position + velocity * Time.fixedDeltaTime;

            float currentHeight = terrain.SampleHeight(position);
            float newHeight = terrain.SampleHeight(newPosition);

            float heightDifference = newHeight - currentHeight;

            if (currentHeight <= _waterLevel) return true; // let them get out of the water, even though we shouldn't be able to get to it

            // If the height difference is too great, we can't move
            if (Mathf.Abs(heightDifference) > _maxHeightDifference)
            {
                return false;
            }

            // Similarly, we can't move if there will be water there
            if (newHeight < _waterLevel)
            {
                return false;
            }

            return true;
        }

        private void MoveTowardsPathOfLeastResistance(PlayerContext context, Vector3 desiredVelocity)
        {
            // called when we can't move because of a height difference or water
            // try to move in the direction in which the height difference is the smallest, or that there is no water
            // also make sure that it's kinda in the direction of the desired velocity

            Terrain terrain = Terrain.activeTerrain;
            Vector3 position = context.Rigidbody.position;

            float currentHeight = terrain.SampleHeight(position);

            Vector3 bestDirection = Vector3.zero;
            float smallestHeightDifference = float.MaxValue;
            float smallestChangeInDirection = float.MaxValue;

            // check all 8 directions around the player
            for (int i = 0; i < 8; i++)
            {
                float theta = (i / (float)8) * Mathf.PI * 2;
                Vector3 offset = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta));

                Vector3 newPosition = position + offset;
                float newHeight = terrain.SampleHeight(newPosition);

                if (newHeight < _waterLevel) continue; // don't move into the water

                float heightDifference = newHeight - currentHeight;

                if (Mathf.Abs(heightDifference) > _maxHeightDifference) continue; // don't move into a place that's too steep

                float changeInDirection = (desiredVelocity - offset).magnitude;


                if (Mathf.Abs(heightDifference) < smallestHeightDifference && changeInDirection < smallestChangeInDirection)
                {
                    smallestHeightDifference = Mathf.Abs(heightDifference);
                    bestDirection = offset;
                    smallestChangeInDirection = changeInDirection;
                }
            }


            context.Rigidbody.velocity = new Vector3(bestDirection.x, context.Rigidbody.velocity.y, bestDirection.z);
        }

    }
}
