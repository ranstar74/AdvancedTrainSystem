using FusionLibrary.Extensions;
using FusionLibrary.Memory.Paths;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Drawing;

namespace AdvancedTrainSystem.Core
{
    [Flags]
    public enum PathMoverFlags
    {
        None = 0,
        IgnoreWorld = 1,
        OverrideHeight = 2,
    }

    /// <summary>
    /// This class handles <see cref="Entity"/> movements along train path.
    /// </summary>
    public class EntityPathMover
    {
        public Entity Entity { get; }

        public Vector3 RotationOffset { get; set; } = Vector3.Zero;

        public bool Direction { get; }

        public float VerticalOffset { get; }

        public float NodePosition { get; private set; }

        public CTrainTrack Track { get; private set; }

        public CTrainTrackNode CurrentNode { get; private set; }

        public CTrainTrackNode NextNode { get; private set; }

        public PathMoverFlags Flags { get; set; }

        public float Speed { get; set; }

        public float TrackSpeed => Direction ? Speed : Speed * -1;

        private int _nextNodeIndex;
        private float _nodeLength;
        private Vector3 _nodeDirectionLength;
        private Vector3 _nodeDirection;
        private Vector3 _nodeRotation;
        private readonly int _moveDirection;

        public EntityPathMover(
            Entity entity, CTrainTrack track, PathMoverFlags flags, int nodeIndex = 0, bool direction = true, float verticalOffset = 0.0f)
        {
            Entity = entity;
            Track = track;
            Direction = direction;
            Flags = flags;
            VerticalOffset = verticalOffset;

            CurrentNode = track[nodeIndex];

            Entity.Position = CurrentNode.Position;

            _moveDirection = Direction ? 1 : -1;
            _nextNodeIndex = GetNextNodeIndex(nodeIndex);

            NextNode = track[_nextNodeIndex];

            UpdateNode();
        }

        public void Update()
        {
            if(Flags.HasFlag(PathMoverFlags.IgnoreWorld))
            {
                Function.Call(Hash._DISABLE_VEHICLE_WORLD_COLLISION, Entity);
            }

            // Calculate how much we've moved
            NodePosition += Speed * Game.LastFrameTime / _nodeLength;

            // Interpolate vehicle rotation
            Vector3 currentRotation = Entity.Rotation;
            Vector3 nextRotation = new Vector3(currentRotation.X, currentRotation.Y, _nodeRotation.Z);
            Entity.Rotation = Vector3.Lerp(currentRotation, nextRotation, Game.LastFrameTime * 6) + RotationOffset;

            // Move to next node
            if (NodePosition > 1.0f)
            {
                // Get difference that will move on next node
                NodePosition -= 1.0f;

                CurrentNode = NextNode;

                _nextNodeIndex = GetNextNodeIndex(_nextNodeIndex);
                NextNode = Track[_nextNodeIndex];

                UpdateNode();
            }

            // Apply movement which is node position (on map) + velocity * track move direction
            Vector3 trackVelocity = _nodeDirectionLength * NodePosition * _moveDirection;
            Vector3 desiredPosition = CurrentNode.Position + trackVelocity;

            World.DrawLine(desiredPosition, desiredPosition + Vector3.WorldUp, Color.Red);

            if(Flags.HasFlag(PathMoverFlags.OverrideHeight))
            {
                desiredPosition += Vector3.WorldUp * VerticalOffset;
            }

            // Apply resulting velocity on entity
            Vector3 entityVelocity = (desiredPosition - Entity.Position) / Game.LastFrameTime;

            if(!Flags.HasFlag(PathMoverFlags.OverrideHeight))
            {
                entityVelocity.Z = Entity.Velocity.Z;
            }

            Entity.Velocity = entityVelocity;
        }

        private void UpdateNode()
        {
            _nodeLength = Vector3.Distance(CurrentNode.Position, NextNode.Position);

            Vector3 currentPos = CurrentNode.Position;
            Vector3 nextPos = NextNode.Position;

            if (Direction)
            {
                _nodeDirection = currentPos.GetDirectionTo(nextPos);
            }
            else
            {
                _nodeDirection = nextPos.GetDirectionTo(currentPos);
            }

            _nodeRotation = _nodeDirection.DirectionToRotation(0.0f);
            _nodeDirectionLength = _nodeDirection * _nodeLength;
        }

        private int GetNextNodeIndex(int currentNodeIndex)
        {
            if (Direction)
            {
                return currentNodeIndex == Track.Length ? 0 : currentNodeIndex + 1;
            }

            return currentNodeIndex == 0 ? Track.Length : currentNodeIndex - 1;
        }
    }
}
