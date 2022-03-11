using FusionLibrary.Extensions;
using FusionLibrary.Memory.Paths;
using GTA;
using GTA.Math;
using GTA.Native;
using System;

namespace AdvancedTrainSystem.Core
{
    /// <summary>
    /// Flags for <see cref="EntityPathMover"/>.
    /// </summary>
    [Flags]
    public enum PathMoverFlags
    {
        None = 0,
        NoCollision = 1,
        DisableSteering = 2
    }

    /// <summary>
    /// This class handles <see cref="Entity"/> movements along train path.
    /// </summary>
    public class EntityPathMover
    {
        /// <summary>
        /// Entity that being moved on path. This property is read-only.
        /// </summary>
        public Entity Entity { get; }

        /// <summary>
        /// Additional entity rotation.
        /// </summary>
        public Vector3 RotationOffset { get; set; } = Vector3.Zero;

        /// <summary>
        /// Direction of the Entity on path.
        /// If set to True, Entity will go from start to end,
        /// with False Entity will move in opposite direciton, from end to start.
        /// </summary>
        public bool Direction { get; }

        /// <summary>
        /// Vertical Entity offset above ground.
        /// </summary>
        public float VerticalOffset { get; }

        /// <summary>
        /// Track on which Entity is moving.
        /// </summary>
        public CTrainTrack Track { get; private set; }

        /// <summary>
        /// Current node of the Track.
        /// </summary>
        public CTrainTrackNode CurrentNode { get; private set; }

        /// <summary>
        /// Next node of the Track with taking direction into account.
        /// </summary>
        public CTrainTrackNode NextNode { get; private set; }

        /// <summary>
        /// Previous node of the Track with taking direction into account.
        /// </summary>
        public CTrainTrackNode PreviousNode { get; private set; }

        /// <summary>
        /// Flags of this <see cref="EntityPathMover"/>.
        /// </summary>
        public PathMoverFlags Flags { get; }

        /// <summary>
        /// Speed at which Entity is moving along the Track, relative to direction.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Speed at which Entity is moving along the Track, without taking direction into account.
        /// </summary>
        public float TrackSpeed
        {
            get => Direction ? Speed : Speed * -1;
            set => Speed = Direction ? value : value * -1;
        }

        private bool _isAligning;
        private int _currentNodeIndex;
        private int _nextNodeIndex;
        private int _previousNodeIndex;
        private Vector3 _nodeDirection;

        private EntityPathMover(Entity entity, CTrainTrack track, PathMoverFlags flags, float zOffset, bool dir)
        {
            Entity = entity;
            Track = track;
            Flags = flags;
            Direction = dir;
            VerticalOffset = zOffset;

            if (Flags.HasFlag(PathMoverFlags.NoCollision))
            {
                Function.Call(Hash._DISABLE_VEHICLE_WORLD_COLLISION, Entity);

                // To prevent from falling under map
                Entity.HasGravity = false;
            }
        }

        /// <summary>
        /// Creates a new <see cref="EntityPathMover"/> instance, warping entity to given point on given track.
        /// </summary>
        /// <param name="entity">Entity that will be moved on path.</param>
        /// <param name="track">Track on which Entity will move on</param>
        /// <param name="flags">Flags to use.</param>
        /// <param name="zOffset">Vertical offset of entity above ground.</param>
        /// <param name="direction">Direction of entity on the Track.</param>
        /// <param name="nodeIndex">Track node index which will be used as start position.</param>
        public static EntityPathMover CreateOnNode(Entity entity, CTrainTrack track, PathMoverFlags flags, float zOffset = 0.0f, bool direction = true, int nodeIndex = 0)
        {
            EntityPathMover pathMover = new EntityPathMover(entity, track, flags, zOffset, direction);

            pathMover.WarpToNode(nodeIndex);

            return pathMover;
        }

        /// <summary>
        /// Creates a new <see cref="EntityPathMover"/> instance, aligning entity with closest point on given track.
        /// </summary>
        /// <param name="entity">Entity that will be moved on path.</param>
        /// <param name="flags">Flags to use.</param>
        /// <param name="zOffset">Vertical offset of entity above ground.</param>
        public static EntityPathMover CreateOnClosestNode(Entity entity, PathMoverFlags flags, float zOffset = 0.0f)
        {
            (int trackIndex,int nodeIndex, float _) = CTrainTrackCollection.Instance.GetClosestTrackNode(entity.Position);

            CTrainTrack track = CTrainTrackCollection.Instance[trackIndex];

            // Check if entity looks in next node direction
            Vector3 nodeDir = Vector3
                .Subtract(track[GetNextNodeIndex(nodeIndex, true, track)].Position, track[nodeIndex].Position)
                .Normalized;
            bool direction = Vector3.Dot(nodeDir, entity.ForwardVector) >= 0;

            EntityPathMover pathMover = new EntityPathMover(entity, track, flags, zOffset, direction);

            pathMover.MoveToNode(nodeIndex);
            pathMover.AlignWithCurrentNode();

            return pathMover;
        }

        /// <summary>
        /// Updates entity movement on path. Needs to be called every tick.
        /// </summary>
        public void Update()
        {
            // Make entity move with node direction at specified speed
            Vector3 nextPos = NextNode.Position;
            nextPos.Z += VerticalOffset;

            Vector3 velocity = Vector3.Subtract(nextPos, Entity.Position).Normalized * Speed;

            // Align entity with closest position on track
            if(_isAligning)
            {
                float distToNode = VectorExtensions.DistanceToLine2D(
                    CurrentNode.Position, NextNode.Position, Entity.Position);

                float distToNodeNext = VectorExtensions.DistanceToLine2D(
                    CurrentNode.Position, NextNode.Position, Entity.Position + Entity.RightVector * distToNode);

                if (distToNode < distToNodeNext)
                {
                    distToNode *= -1;
                }

                velocity += Entity.RightVector * distToNode / (Game.LastFrameTime * 5);

                if (distToNode < 0.05f)
                {
                    _isAligning = false;
                }
            }

            Entity.Velocity = velocity;

            // Smoothly align vehicle rotation with node, and a bit faster in aligning mode
            Quaternion rotation = _nodeDirection.LookRotation(Vector3.WorldUp);
            Entity.Quaternion = Quaternion.Slerp(Entity.Quaternion, rotation, Game.LastFrameTime * (_isAligning ? 4 : 2));

            // Check if we can move to next/previous node by comparing distances
            float currentNodeDist = Vector3.DistanceSquared2D(Entity.Position, CurrentNode.Position);
            float nextNodeDist = Vector3.DistanceSquared2D(Entity.Position, NextNode.Position);
            float prevNodeDist = Vector3.DistanceSquared2D(Entity.Position, PreviousNode.Position);

            if (nextNodeDist < currentNodeDist)
            {
                MoveToNode(_nextNodeIndex);
            }
            else if (prevNodeDist < currentNodeDist)
            {
                MoveToNode(_currentNodeIndex);
            }

            if(Flags.HasFlag(PathMoverFlags.DisableSteering) && Game.Player.Character.CurrentVehicle is Entity)
            {
                // TODO: Figure out blocking control
            }
        }

        /// <summary>
        /// Warps entity to given node index.
        /// </summary>
        /// <param name="nodeIndex">Index of the Track node.</param>
        public void WarpToNode(int nodeIndex)
        {
            MoveToNode(nodeIndex);

            Entity.Position = CurrentNode.Position;
            Entity.Quaternion = _nodeDirection.LookRotation(Vector3.WorldUp);
        }

        /// <summary>
        /// Sets node as current node, without warping entity.
        /// </summary>
        /// <param name="nodeIndex">Index of the Track node.</param>
        public void MoveToNode(int nodeIndex)
        {
            _currentNodeIndex = nodeIndex;
            _nextNodeIndex = GetNextNodeIndex(nodeIndex, Direction, Track);
            _previousNodeIndex = GetPreviousNodeIndex(nodeIndex, Direction, Track);

            CurrentNode = Track[nodeIndex];
            NextNode = Track[_nextNodeIndex];
            PreviousNode = Track[_previousNodeIndex];

            UpdateNodeDirection();
        }

        /// <summary>
        /// Aligns entity with path, only moving it side wise.
        /// </summary>
        public void AlignWithCurrentNode()
        {
            _isAligning = true;
        }

        private void UpdateNodeDirection()
        {
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
        }

        private static int GetNextNodeIndex(int currentNodeIndex, bool direction, CTrainTrack track)
        {
            if (direction)
            {
                return currentNodeIndex == track.Length ? 0 : currentNodeIndex + 1;
            }

            return currentNodeIndex == 0 ? track.Length : currentNodeIndex - 1;
        }

        private static int GetPreviousNodeIndex(int currentNodeIndex, bool direction, CTrainTrack track)
        {
            return GetNextNodeIndex(currentNodeIndex, !direction, track);
        }
    }
}
