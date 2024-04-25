using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ChiseDrive.Units;
using ChiseDrive.Pathfinding;

namespace ChiseDrive.Motion
{
    public class SimpleMoveTo : Locomotion
    {
        public SimpleMoveTo(Units.Unit owner)
            : base(owner)
        {
        }

        public override Locomotion Clone(Units.UnitEX owner)
        {
            return new SimpleMoveTo(follow);
        }

        public override void MoveWithin(Vector3 destination, float distance)
        {
            base.MoveWithin(destination, distance);

            Path.BuildPath(follow.Game.GameBoard, follow,
                follow.Game.GameBoard.WorldToNode(follow.Position),
                follow.Game.GameBoard.WorldToNode(destination));

            destination = GameBoard.NodeToScreen3(Path.LastStep);
        }

        public override void Update(Time elapsed)
        {
            if (Physics != null)
            {
                Debug.Metrics.OpenMetric("Locomotion.FetchDest");
                Vector3 nextdest = destination;
                if (Path.CurrentStep != Node.Invalid)
                {
                    nextdest = GameBoard.NodeToScreen3(Path.CurrentStep);

                    IBounding contents = GameBoard.GetContents(nextdest);
                    if (contents != null && contents != follow)
                    {
                        Path.BuildPath(follow.Game.GameBoard, follow,
                            GameBoard.WorldToNode(follow.Position),
                            GameBoard.WorldToNode(destination));

                        if (Path.CurrentStep == Node.Invalid)
                        {
                            destination = follow.Position;
                            InCollision = true;
                        }
                    }
                }
                else
                {
                    destination = follow.Position;
                }

                nextdest.Z = follow.Position.Z;
                Debug.Metrics.CloseMetric("Locomotion.FetchDest");

                Debug.Metrics.OpenMetric("Locomotion.Physics");
                if (Vector3.Distance(follow.Position, nextdest) > SettleDistance)
                {
                    Vector3 delta = nextdest - follow.Position;
                    delta = Helper.Normalize(delta);
                    delta *= AccelerationRate;
                    Physics.Push(delta);

                    facing = nextdest;
                }
                else if (Path.CurrentStep != Node.Invalid)
                {
                    Path.Increment();
                }

                if (facing != Vector3.Zero)
                {
                    Vector3 delta = facing - follow.Position;
                    float rdelta = (float)Math.Atan2((double)delta.Y, (double)delta.X);
                    rdelta += (float)(Math.PI / 2);
                    follow.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Backward, rdelta);
                }
                
                Vector3 testposition = Physics.RunPhysics(elapsed, follow.Position);
                Vector2 location = new Vector2(testposition.X, testposition.Y);
                Debug.Metrics.CloseMetric("Locomotion.Physics");

                Debug.Metrics.OpenMetric("Locomotion.PathRebuild");
                //List<Unit> intersect = follow.Game.GetContents(location, follow.CollisionRadius, ChiseDriveGame.Contents.AllIntersects);

                follow.Position = testposition;
                /*
                IBounding contents = follow.Game.GameBoard.GetContents(testposition);

                if (contents != follow && contents != null)
                //if (intersect.Count > 1)
                //                if (intersect.Count < 2) follow.Position = testposition;
                //                else
                {
                    // There was a collision so try to build a new path
                    path.BuildPath(follow.Game.GameBoard, follow,
                        follow.Game.GameBoard.WorldToNode(follow.Position),
                        follow.Game.GameBoard.WorldToNode(destination));

                    if (path.CurrentStep == Node.Invalid)
                    {
                        destination = follow.Position;
                    }

                    InCollision = true;
                }
                else
                {
                    follow.Position = testposition;
                }*/

                Debug.Metrics.CloseMetric("Locomotion.PathRebuild");
            }

            follow.HeightCorrect();
        }
    }
}