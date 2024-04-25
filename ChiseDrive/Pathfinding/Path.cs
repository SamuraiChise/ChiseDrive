using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using ChiseDrive.Graphics;


namespace ChiseDrive.Pathfinding
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Path : Microsoft.Xna.Framework.DrawableGameComponent
    {
        const int MaxPath = 50;
        List<Node> PathList = new List<Node>(MaxPath);
        
        int currentStep;

        #region Debug
#if DebugPathfinding
        Texture2D PathTexture;
        Rectangle PathRectangle;
        List<Sprite3D> PathSprites = new List<Sprite3D>(30);
#endif
        #endregion

        ChiseDriveGame Game;

        public Node CurrentStep
        {
            get
            {
                if (currentStep >= PathList.Count) return Node.Invalid;
                return PathList[currentStep];
            }
        }
        public Node NextStep
        {
            get
            {
                currentStep++;
                return CurrentStep;
            }
        }
        public Node LastStep
        {
            get
            {
                if (PathList.Count <= 0) return Node.Invalid;
                return PathList[PathList.Count - 1];
            }
        }

        GameBoard board = null;
        IBounding follow = null;

        public Path(ChiseDriveGame game)
            : base(game)
        {
            this.Game = game;
            DrawOrder = ChiseDrive.DrawOrder.Path;
            PathList = new List<Node>(GameBoard.MaxEntries);
            Initialize();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            #region Debug
#if DebugPathfinding
            PathTexture = Game.Content.Load<Texture2D>("Textures/hexhilight");
            PathRectangle = new Rectangle();
            PathRectangle.Width = 10;
            PathRectangle.Height = 58;
            PathRectangle.X = 0;
            PathRectangle.Y = 0;
            for (int i = 0; i < PathSprites.Capacity; i++)
            {
                PathSprites.Add(new Sprite3D(Vector3.Zero, Vector3.Up, Vector3.Left, 1f, new Color(Color.Red, 100),
                    new AnimatedTexture(PathTexture)));
            }
#endif
            #endregion
        }

        PathNode endNode = null;

        static List<PathNode> OpenList;
        static List<PathNode> CloseList;
        static List<Node> NearestList;
        static PathNode[] ScratchList;
        static int scratchindex = 0;
        static void ClearScratchData() { scratchindex = 0; }
        static PathNode NextScratchNode
        {
            get
            {
                scratchindex++;
                if (scratchindex >= ScratchList.Length) return null;
                return ScratchList[scratchindex - 1];
            }
        }
        static PathNode NewPathNode(Node initial, Node destination, PathNode parent)
        {
            PathNode pn = NextScratchNode;
            if (pn == null) return null;

            pn.Reinitialize(initial, destination, parent);
            return pn;
        }
        
        public static void InitializeData()
        {
            OpenList = new List<PathNode>(GameBoard.MaxEntries);
            CloseList = new List<PathNode>(GameBoard.MaxEntries);
            NearestList = new List<Node>(MaxPath);
            ScratchList = new PathNode[GameBoard.MaxEntries];

            for (int i = 0; i < ScratchList.Length; i++)
            {
                ScratchList[i] = new PathNode(Node.Invalid, Node.Invalid, null);
            }
        }

        public void Increment()
        {
            currentStep++;
        }

        public void BuildPath(GameBoard board, IBounding follow, Node initial, Node destination)
        {
            ChiseDrive.Debug.Metrics.OpenMetric("Path.BuildPath");
            if (board == null) throw new NullReferenceException();
            if (ScratchList.Length == 0) throw new Exception("Call Path.InitializeData after setting up the GameBoard.");
            this.board = board;

            ClearScratchData();

            if (initial == destination) return;
            this.follow = follow;

            endNode = null;

            Node closest = FindClosestVacant(destination, initial);
            if (closest != Node.Invalid)
            {
                float initialCost = initial.EstimateCost(destination);
                float closestCost = closest.EstimateCost(destination);

                if (initialCost <= closestCost)
                {
                    // There are no shorter paths so return an empty path!
                    PathList.Add(Node.Invalid);
                    return;
                }

                // Go to the closest available square
                destination = closest;
            }

            PathList.Clear();
            OpenList.Clear();
            CloseList.Clear();
            ClearScratchData();

            PathNode start = NewPathNode(initial, destination, null);

            // Since the start is likely full (of whoever wants this path)
            OpenList.Add(start);

            int i = 0;

            // For every GridNode, add it to the list, expand it's kids, put it on the closed list.
            // Then find the next node with the best heuristic and repeat.
            while (i < MaxPath && endNode == null)
            {
                PathNode n = FindBestNode();
                if (n != null)
                {
                    ExpandList(n, destination);
                    OpenList.Remove(n);
                    CloseList.Add(n);
                }
                i++;
            }

            // If the distance to path was too long to find a destination,
            // find the closest path we've reached so far
            if (endNode == null)
            {
                // We didn't find the end, so settle for our best
                endNode = FindBestNode();
            }

            PathNode path = endNode;

            while (path != null)
            {
                PathList.Add(path.Node);
                path = path.Parent;
            }

            PathList.Reverse();

            currentStep = 1;

            ChiseDrive.Debug.Metrics.CloseMetric("Path.BuildPath");
            #region Debug
#if DebugPathfinding
            string pathstr = "New Path: ";
            foreach (Node n in PathList)
            {
                pathstr += n;
            }
            Debug.DebugText.Write(pathstr, Time.FromFrames(60f));

            for (int j = 0; j < PathSprites.Count; j++)
            {
                if (j < PathList.Count)
                {
                    PathSprites[j].Visible = true;
                    PathSprites[j].Position = new Vector3(board.NodeToScreen(PathList[j]), 15f);
                    PathSprites[j].Position = Game.World.CorrectForHeight(PathSprites[j].Position);
                }
                else
                {
                    PathSprites[j].Visible = false;
                }
            }
#endif
            #endregion
        }

        private Node FindClosestVacant(Node destination, Node initial)
        {
            PathNode nearest = NewPathNode(destination, initial, null);
            IBounding test = board.GetContents(nearest.Node);

            NearestList.Clear();

            // Simple test to see if the destination itself is vacant
            if (test == null || test == follow) return destination;

            PathNode best = PathNode.Empty;

            int depth = 1;
            while (best == PathNode.Empty && depth < 6)
            {
                ExpandNodeRangeRecursive(destination, depth);

                foreach (Node n in NearestList)
                {
                    test = board.GetContents(n);
                    if (test == null)
                    {
                        PathNode pn = NewPathNode(n, initial, null);
                        if (pn == null) return best.Node;
                        if (pn < best)
                            best = pn;
                    }
                }
                depth++;
            }

            return best.Node;
        }

        void ExpandNodeRangeRecursive(Node parent, int range)
        {
            if (range <= 0) return;

            for (int i = 0; i < (int)Node.Step.Count; i++)
            {
                Node node = new Node(parent, (Node.Step)i);
                if (Game.GameBoard.IsNodeSafe(ref node))
                {
                    if (!NearestList.Contains(node)) NearestList.Add(node);
                    ExpandNodeRangeRecursive(node, range - 1);
                }
            }
        }

        /// <summary>
        /// Finds the Path's best guess at the shortest node.
        /// </summary>
        /// <returns></returns>
        private PathNode FindBestNode()
        {
            if (OpenList.Count < 1) return null;

            PathNode best = PathNode.Empty;

            foreach (PathNode n in OpenList)
            {
                if (n <= best)
                {
                    best = n;
                }
            }

            return best;
        }
        
        /// <summary>
        /// Expands the list to include the children of the current testNode.
        /// </summary>
        /// <param name="testNode"></param>
        /// <param name="endNode"></param>
        private void ExpandList(PathNode testNode, Node destination)
        {
            for (int i = 0; i < (int)Node.Step.Count; i++)
            {
                Node n = new Node(testNode.Node, (Node.Step)i);
                if (Game.GameBoard.IsNodeSafe(ref n))
                {
                    PathNode pn = NewPathNode(n, destination, testNode);

                    if (pn == null) return;
                    if (pn.Node == destination)
                    {
                        // If we found the end, just return
                        endNode = pn;
                        return;
                    }

                    TryAdd(pn);
                }
            }
        }
        
        /// <summary>
        /// Tries to add a node to the search tree.
        /// </summary>
        /// <param name="testNode">The node to test.</param>
        /// <returns>True if the node was added, false if the node was not added.</returns>
        private bool TryAdd(PathNode testNode)
        {
            if (!IsClosed(testNode))
            {
                // Check to see if this test node already exists in the open list
                PathNode open = IsOpen(testNode);
                if (open != PathNode.Empty)
                {
                    // And if it does, and this is a better way to get to that node, forget the old route!
                    if (open.DistanceTraveled > testNode.DistanceTraveled)
                    {
                        OpenList.Remove(open);
                        OpenList.Add(testNode);
                        return true;
                    }
                }
                else
                {
                    // This node isn't in the open list, so add it
                    OpenList.Add(testNode);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if this current node is either occupied or closed.
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        private bool IsClosed(PathNode testNode)
        {
            IBounding contents = board.GetContents(testNode.Node);
            // Closed if it's occupied
            if (contents != follow && contents != null) return true;

            foreach (PathNode pn in CloseList)
            {
                if (testNode.Equals(pn))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if this node is open and can be expanded.
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        private PathNode IsOpen(PathNode testNode)
        {
            foreach (PathNode pn in OpenList)
            {
                if (testNode.Equals(pn))
                {
                    return pn;
                }
            }
            return PathNode.Empty;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}