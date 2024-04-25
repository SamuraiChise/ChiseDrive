using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.Pathfinding
{
    public struct Node
    {
#if Hexagons
        public enum Step
        {
            Up, UpRight, DownRight, Down, DownLeft, UpLeft,
        };
#else
        public enum Step
        {
            Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft, Count,
        };
#endif

        public int X;
        public int Y;

        public static Node Invalid
        {
            get
            {
                return new Node(-1, -1); 
            }
        }

        public Node(int x, int y) { X = x; Y = y; }
        public Node(Node initial, Step direction) 
        {
            int dx = 0;
            int dy = 0;

#if Hexagons
            switch (direction)
            {
                case Step.Up: dx = 0; dy = -1; break;
                case Step.Down: dx = 0; dy = 1; break;
                case Step.UpRight: dx = 1; dy = (initial.X % 2 == 0) ? 0 : -1; break;
                case Step.DownRight: dx = 1; dy = (initial.X % 2 == 0) ? 1 : 0; break;
                case Step.UpLeft: dx = -1; dy = (initial.X % 2 == 0) ? 0 : -1; break;
                case Step.DownLeft: dx = -1; dy = (initial.X % 2 == 0) ? 1 : 0; break;
                default: dx = 0; dy = 0; break;
            }
#else
            switch (direction)
            {
                case Step.Up: dx = 0; dy = -1; break;
                case Step.UpRight: dx = 1; dy = -1; break;
                case Step.Right: dx = 1; dy = 0; break;
                case Step.DownRight: dx = 1; dy = 1; break;
                case Step.Down: dx = 0; dy = 1; break;
                case Step.DownLeft: dx = -1; dy = 1; break;
                case Step.Left: dx = -1; dy = 0; break;
                case Step.UpLeft: dx = -1; dy = -1; break;
                default: dx = 0; dy = 0; break;
            }
#endif

            X = initial.X + dx;
            Y = initial.Y + dy;
        }

        public float EstimateCost(Node rhs)
        {
            if (this == Node.Invalid || rhs == Node.Invalid) return float.PositiveInfinity;

            float distx = (float)(X - rhs.X);
            float disty = (float)(Y - rhs.Y);

            distx = Math.Abs(distx);
            disty = Math.Abs(disty);

            float cost = 0f;

#if Hexagons
            cost = (distx + disty);
#else
            cost = (float)Math.Sqrt(distx * distx + disty * disty);
#endif

            return cost;
        }

        public static bool operator ==(Node lhs, Node rhs)
        {
            return lhs.X == rhs.X && lhs.Y == rhs.Y;
        }
        public static bool operator !=(Node lhs, Node rhs)
        {
            return lhs.X != rhs.X || lhs.Y != rhs.Y;
        }

        public override string ToString()
        {
            if (this == Node.Invalid) return "[Invalid]";
            else return "[" + X + "," + Y + "]";
        }
    }

    /// <summary>
    /// This class exists because it's impossible to have a struct that will
    /// hold a parent of the same type as the struct, since that will create
    /// an infinitely big struct.
    /// </summary>
    public class PathNode
    {
        Node index;
        PathNode parent;
        public int X { get { return index.X; } }
        public int Y { get { return index.Y; } }

        public float DistanceRemaining;
        public float DistanceTraveled;
        float EstimateCost { get { return DistanceTraveled + (DistanceRemaining * 2f); } }

        static PathNode empty = new PathNode(Node.Invalid, Node.Invalid, null);
        public static PathNode Empty { get { return empty; } }

        public Node Node { get { return index; } }
        public PathNode Parent { get { return parent; } }

        public PathNode(Node index, Node destination, PathNode parent)
        {
            Reinitialize(index, destination, parent);
        }

        public PathNode Reinitialize(Node index, Node destination, PathNode parent)
        {
            this.index = index;
            this.parent = parent;

            if (parent == null) DistanceTraveled = 0;
            else DistanceTraveled = parent.DistanceTraveled + parent.Node.EstimateCost(index);

            DistanceRemaining = index.EstimateCost(destination);

            return this;
        }

        public void SetEmpty()
        {
            this.index = Node.Invalid;
            this.parent = null;
        }

        public bool IsEmpty { get { return index == Node.Invalid; } }

        public override string ToString()
        {
            if (this == PathNode.Empty) return "[Empty]";
            return "[T: " + DistanceTraveled + " R: " + DistanceRemaining + "]";
        }

        public static bool operator <(PathNode lhs, PathNode rhs)
        {
            return lhs.EstimateCost < rhs.EstimateCost;
        }

        public static bool operator >(PathNode lhs, PathNode rhs)
        {
            return lhs.EstimateCost > rhs.EstimateCost;
        }
        public static bool operator <=(PathNode lhs, PathNode rhs)
        {
            return lhs.EstimateCost <= rhs.EstimateCost;
        }

        public static bool operator >=(PathNode lhs, PathNode rhs)
        {
            return lhs.EstimateCost >= rhs.EstimateCost;
        }
    }
}
