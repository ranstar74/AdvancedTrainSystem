using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using FusionLibrary.Memory.Paths;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.ComponentModel;

namespace AdvancedTrainSystem.Core
{
    /// <summary>
    /// Flags for <see cref="EntityPathMover"/>.
    /// </summary>
    [Flags]
    public enum PathMoverFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,
        /// <summary>
        /// Entity collision with world will be disabled.
        /// Useful for trains without actual wheels.
        /// <para>Collision with player will remain.</para>
        /// </summary>
        NoCollision = 1,
        /// <summary>
        /// Disables ability to steer vehicle.
        /// </summary>
        DisableSteering = 2,
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
        public Vector3 RotationOffset
        {
            get => _rotationOffset.ToDegrees();
            set => _rotationOffset = value.ToQuaternion();
        }

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
        /// <para>Equal to <see cref="Entity.Speed"/>.</para>
        /// </summary>
        public float Speed
        {
            get => Entity.Speed;
            set => Entity.Velocity = Entity.ForwardVector * value;
        }

        /// <summary>
        /// Speed at which Entity is moving along the Track, without taking direction into account.
        /// </summary>
        public float TrackSpeed
        {
            get => Direction ? Speed : Speed * -1;
            set => Speed = Direction ? value : value * -1;
        }

        /// <summary>
        /// Invoked on entity derail.
        /// </summary>
        public event EventHandler Derailed;

        /// <summary>
        /// Gets a value that indicates if this path mover was aborted.
        /// </summary>
        public bool IsAborted => _aborted;

        private bool _aborted;                      // Whether mover was aborted or not
        private bool _isAligning;                   // Whether entity is currently aligning with closest position on track
        private int _alignStartTime;                // Time when aligning started. Using for aligning timeout
        private int _alignFinishTime = -1;          // Time when aligning finished. Used to check if theres some time passed in derail update
        private int _currentNodeIndex;              // Index of current node on track. We store it because node doesnt store self index
        private int _nextNodeIndex;                 // Index of the next node on track
        private int _previousNodeIndex;             // Index of the previous node on track
        private float[] _suspensionCompressions;    // Compressions to use while aligning because without world collision suspension unfolds
        private float _distanceToGround;            // Distance to ground to use while aligning because world collision is disabled
        private float _speedBeforeAligning;         // Stored speed before aligning to be restored after
        private float _currentHeight;               // Interpolated Z between current and next node
        private float _distanceToCurrentNode;       // Distance from entity to current node
        private Vector3 _nodeDirection;             // Unit vector representing current -> next node direction
        private Vector3 _previousNodeDirection;     // Unit vector representing current -> previous node direction
        private Vector3 _closestNodeDirection;      // Direction of closest node (which is previousNodeDirection or nodeDirection)
        private Quaternion _rotationOffset;         // Rotation offset stored in quaternion

        private EntityPathMover(Entity entity, CTrainTrack track, PathMoverFlags flags, float zOffset, bool dir)
        {
            Entity = entity;
            Track = track;
            Flags = flags;
            Direction = dir;
            VerticalOffset = zOffset;

            if (Flags.HasFlag(PathMoverFlags.NoCollision))
            {
                TogglePhysics(false);
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
        public static EntityPathMover CreateOnClosestNode(Entity entity, PathMoverFlags flags = PathMoverFlags.None, float zOffset = 0.0f)
        {
            (int trackIndex, int nodeIndex, float _) = CTrainTrackCollection.Instance.GetClosestTrackNode(entity.Position);

            CTrainTrack track = CTrainTrackCollection.Instance[trackIndex];
            CTrainTrackNode currentNode = track[nodeIndex];
            CTrainTrackNode nextNode = track[GetNextNodeIndex(nodeIndex, true, track)];

            // Check if entity looks in next node direction
            Vector3 nodeDir = Vector3
                .Subtract(nextNode.Position, currentNode.Position)
                .Normalized;
            bool direction = Vector3.Dot(nodeDir, entity.ForwardVector) >= 0;

            EntityPathMover pathMover = new EntityPathMover(entity, track, flags, zOffset, direction);

            pathMover.MoveToNode(nodeIndex);
            pathMover.AlignWithCurrentNode();

            return pathMover;
        }

        /// <summary>
        /// Updates entity movement on path. Needs to be called every tick AFTER updating entity speed.
        /// </summary>
        public void Update()
        {
            if (_aborted)
            {
                throw new Exception("Path mover was aborted.");
            }

            //World.DrawLine(CurrentNode.Position + Vector3.WorldUp, NextNode.Position + Vector3.WorldUp, Color.Blue);
            //World.DrawLine(CurrentNode.Position + Vector3.WorldUp, PreviousNode.Position + Vector3.WorldUp, Color.Red);

            if(Flags.HasFlag(PathMoverFlags.NoCollision))
            {
                // Without collision we have to manually calculate Z position between current and next node
                UpdateZPosition();
            }

            Vector3 velocity = Vector3.Zero;
            // Align entity with closest position on track
            if (_isAligning)
            {
                UpdateAligning(ref velocity);
            }
            // Make entity move with node direction at specified speed
            UpdateVelocity(ref velocity);

            Entity.Velocity = velocity;

            // Smoothly align vehicle rotation with node
            UpdateRotation();

            // Check if we can move to next/previous node by comparing distances
            UpdateMoveToNextNode();

            // Check if theres conditions to derail entity
            UpdateDerail();
        }

        private void UpdateZPosition()
        {
            // Get closest point should also work for height but this requires less operations

            Vector3 interpolatedPos = CurrentNode.Position + _closestNodeDirection * _distanceToCurrentNode;

            _currentHeight = interpolatedPos.Z;

            //World.DrawLine(interpolatedPos, interpolatedPos + Vector3.WorldUp, Color.Green);
        }

        private void UpdateVelocity(ref Vector3 velocity)
        {
            // To check whether entity is moving forward or backwards
            float relativeVelocity = Entity.RelativeVelocity().Y;

            Vector3 destination;
            Vector3 hOffset = Vector3.WorldUp * VerticalOffset;
            if (relativeVelocity >= 0)
            {
                destination = Vector3.Subtract(NextNode.Position + hOffset, Entity.Position);
            }
            else
            {
                destination = Vector3.Subtract(Entity.Position, PreviousNode.Position + hOffset);
            }

            velocity += destination.Normalized * Speed;
            if (relativeVelocity < 0)
            {
                velocity *= -1;
            }

            if (!Flags.HasFlag(PathMoverFlags.NoCollision))
            {
                velocity.Z = Entity.Velocity.Z;
            }
            else
            {
                velocity.Z = (_currentHeight - Entity.Position.Z) / Game.LastFrameTime;
            }
        }

        private void UpdateAligning(ref Vector3 velocity)
        {
            Vector3 closestAlignPoint = Entity.Position.GetClosestPointOnFiniteLine(
                PreviousNode.Position, NextNode.Position);

            if (Flags.HasFlag(PathMoverFlags.NoCollision))
            {
                closestAlignPoint.Z += VerticalOffset;
            }
            else
            {
                closestAlignPoint.Z += _distanceToGround;
            }

            Vector3 headingToClosestPoint = closestAlignPoint - Entity.Position;

            // Keep only side velocity
            float headingDot = Math.Abs(headingToClosestPoint.Dot2d(Entity.RightVector));
            if (headingDot > 0.75f)
            {
                velocity += headingToClosestPoint / (Game.LastFrameTime * 4.5f);
            }

            // Force wheel compression, explained in AlignWithCurrentNode method
            if (Entity is Vehicle vehicle)
            {
                for (int i = 0; i < _suspensionCompressions.Length; i++)
                {
                    VehicleControl.SetWheelCompression(vehicle, i, _suspensionCompressions[i]);
                }
            }

            // If car is close enough and aligned with node direction or if aligning timed out
            float nodeDot = Vector3.Dot(Entity.ForwardVector, _nodeDirection);
            float dist = headingToClosestPoint.LengthSquared();
            if (Math.Abs(dist) <= 0.1f && nodeDot > 0.9995f || _alignStartTime > Game.GameTime + 500)
            {
                FinishAligning();
            }
        }

        private void UpdateRotation()
        {
            Quaternion rotationQ = _nodeDirection.LookRotation(Vector3.WorldUp);

            // A bit faster in aligning mode
            Entity.Quaternion = Quaternion
                .Slerp(Entity.Quaternion, rotationQ, Game.LastFrameTime * (_isAligning ? 6 : 1)) + _rotationOffset;

            if (Flags.HasFlag(PathMoverFlags.NoCollision))
            {
                // With no collision can will spin around Y axis
                Vector3 rotation = Entity.Rotation;
                rotation.Y = 0;

                Entity.Rotation = rotation;
            }
        }

        private void UpdateMoveToNextNode()
        {
            float currentNodeDist = Vector3.DistanceSquared(Entity.Position, CurrentNode.Position);
            float nextNodeDist = Vector3.DistanceSquared(Entity.Position, NextNode.Position);
            float prevNodeDist = Vector3.DistanceSquared(Entity.Position, PreviousNode.Position);

            _distanceToCurrentNode = (float)Math.Sqrt(currentNodeDist);
            _closestNodeDirection = nextNodeDist < prevNodeDist ? _nodeDirection : _previousNodeDirection;

            if (nextNodeDist < currentNodeDist)
            {
                MoveToNode(_nextNodeIndex);
            }
            else if (prevNodeDist < currentNodeDist)
            {
                MoveToNode(_previousNodeIndex);
            }

            if (Flags.HasFlag(PathMoverFlags.DisableSteering) && Game.Player.Character.CurrentVehicle == Entity)
            {
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 27, 59, true);
            }
        }

        private void UpdateDerail()
        {
            // Derail entity if its in air or wheels are too far from tracks
            if (!_isAligning && (Game.GameTime - _alignFinishTime > 500 && _alignFinishTime != -1))
            {
                float dot = Vector3.Dot(Entity.ForwardVector, _nodeDirection);
                bool isInAir = !Flags.HasFlag(PathMoverFlags.NoCollision) && Entity.IsInAir;
                if (isInAir || Math.Abs(dot) < 0.9f)
                {
                    Derail();
                }
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
            // Set it temporary or car may hit obstacles
            TogglePhysics(false);

            // Since we disable world collision, car suspension
            // will straight up just like in air, so will cause visible
            // 'flickering'. As solution for this we force
            // the same wheel compression ratio during alignment
            if (Entity is Vehicle vehicle)
            {
                _suspensionCompressions = VehicleControl.GetWheelCompressions(vehicle);
            }

            // Make point position more accurate by using actual vertical entity offset from ground
            if (!Flags.HasFlag(PathMoverFlags.NoCollision))
            {
                RaycastResult result = World.Raycast(Entity.Position, Vector3.WorldDown, 1.0f, IntersectFlags.Map);
                if (result.DidHit)
                {
                    _distanceToGround = Vector3.Distance(Entity.Position, result.HitPosition);
                }
            }

            _alignStartTime = Game.GameTime;
            _speedBeforeAligning = Speed;

            _isAligning = true;
        }

        /// <summary>
        /// Instantly moves entity on specified distance.
        /// </summary>
        /// <param name="distance">Distance to move entity on.</param>
        public void Move(float distance)
        {
            // We can't just apply velocity because we need to move 
            // entity on given distance relative to track
            float distanceLeft = distance;
            float distanceToNext = 0.0f;
            while (distanceLeft > distanceToNext)
            {
                distanceToNext = Entity.Position.DistanceTo(NextNode.Position);
                distanceLeft -= distanceToNext;

                WarpToNode(_nextNodeIndex);

                Script.Yield();
            }

            Entity.Position += _nodeDirection * distanceLeft;
            Entity.Quaternion = _nodeDirection.LookRotation(Vector3.WorldUp);
        }

        /// <summary>
        /// Aborts execution and detaches entity from track. Cannot be undone.
        /// </summary>
        public void Abort()
        {
            FinishAligning();
            TogglePhysics(true);

            _aborted = true;
        }

        /// <summary>
        /// Derails train, invoking <see cref="Derailed"/>. 
        /// If event wasn't canceled, <see cref="Abort"/> called.
        /// </summary>
        public void Derail()
        {
            CancelEventArgs e = new CancelEventArgs();

            Derailed?.Invoke(this, e);

            if (!e.Cancel)
            {
                Abort();
            }
        }

        private void FinishAligning()
        {
            if (!_isAligning)
            {
                return;
            }

            // Return world collision back
            if (!Flags.HasFlag(PathMoverFlags.NoCollision))
            {
                TogglePhysics(true);
            }

            // Restore speed as it was modified by aligning code
            // and we don't want that affect actual speed
            Speed = _speedBeforeAligning;

            _alignFinishTime = Game.GameTime;
            _isAligning = false;
        }

        private void TogglePhysics(bool on)
        {
            if (on)
            {
                Entity.IsCollisionEnabled = true;
            }
            else
            {
                Function.Call(Hash._DISABLE_VEHICLE_WORLD_COLLISION, Entity);
            }
        }

        private void UpdateNodeDirection()
        {
            Vector3 currentPos = CurrentNode.Position;
            Vector3 nextPos = NextNode.Position;
            Vector3 previousPos = PreviousNode.Position;

            if (Direction)
            {
                _nodeDirection = currentPos.GetDirectionTo(nextPos);
                _previousNodeDirection = currentPos.GetDirectionTo(previousPos);
            }
            else
            {
                _nodeDirection = previousPos.GetDirectionTo(currentPos);
                _previousNodeDirection = nextPos.GetDirectionTo(currentPos);
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
