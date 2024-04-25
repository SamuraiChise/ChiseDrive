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
    public class GameBoard : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public int Width = 50;
        public int Height = 50;
        Vector2 Offset;
        public static int MaxEntries;
        int SpanX = 48;
        int SpanY = 54;
        int OddY;

        float accuracy;
        public float Accuracy
        {
            get
            {
                return accuracy;
            }
        }

        ChiseDriveGame Game;
        IBounding[,] Contents = null;

        #region Debug
#if DebugPathfinding
        Texture2D HexTile;
        Sprite3D[,] TileSprites = null;
#endif
        #endregion

        public GameBoard(ChiseDriveGame game)
            : base(game)
        {
            this.Game = game;
            this.DrawOrder = ChiseDrive.DrawOrder.Board;
            this.UpdateOrder = ChiseDrive.UpdateOrder.Board;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize(Rectangle dimensions, float accuracy)
        {
            Width = (int)((float)dimensions.Width / accuracy);
            Height = (int)((float)dimensions.Height / accuracy);

            this.accuracy = accuracy;

            SpanX = (int)accuracy;
            SpanY = (int)accuracy;
#if Hexagons
            OddY = SpanY / 2;
#else
            OddY = 0;
#endif
            Offset = new Vector2((float)dimensions.X, (float)dimensions.Y);

            MaxEntries = Width * Height;

            Contents = new IBounding[Width, Height];

            #region Debug
#if DebugPathfinding
#if Hexagons
            HexTile = Game.Content.Load<Texture2D>("Textures/hextile");
#else
            HexTile = Game.Content.Load<Texture2D>("Textures/squaretile");
#endif
            
            TileSprites = new Sprite3D[Width, Height];
            for (int w = 0; w < Width; w++)
            {
                for (int h = 0; h < Height; h++)
                {
                    Node n = new Node(w, h);
                    Vector2 screen = NodeToScreen(n);

                    Vector3 world = new Vector3(screen, 15f);
                    world = Game.World.CorrectForHeight(world);

                    TileSprites[w, h] = new Sprite3D(
                        world,
                        Vector3.Up, Vector3.Left, accuracy * 0.6f, new Color(Color.Gold, 100), 
                        new AnimatedTexture(HexTile));
                    TileSprites[w, h].Visible = false;
                }
            }
#endif
            #endregion

            Path.InitializeData();

            base.Initialize();
        }

        public IBounding GetContents(Node location)
        {
            if (location.X < 0 || location.Y < 0) return null;
            return Contents[location.X, location.Y];
        }

        public IBounding GetContents(Vector3 location)
        {
            return GetContents(WorldToNode(location));
        }

        public bool Touches(Node n, Vector3 test, float distance)
        {
            Vector2 world = NodeToScreen(n);
            Vector2 test2 = new Vector2(test.X, test.Y);

            float d = Vector2.Distance(world, test2);
            d -= distance + SpanX / 2f;
            if (d > 0) return false;
            return true;
        }

        public bool TryPlaceObject(Vector2 location, IBounding obj)
        {
            return false;
        }

        public bool TryPlaceObject(Node location, IBounding obj, ref Node[] previous)
        {
            foreach (Node n in previous)
            {
                if (n != Node.Invalid)
                {
                    Contents[n.X, n.Y] = null;

                    #region Debug
#if DebugPathfinding
                    TileSprites[n.X, n.Y].Visible = false;
#endif
                    #endregion
                }
            }

            for (int i = 0; i < previous.Length; i++)
            {
                previous[i] = Node.Invalid;
            }

            bool placed = false;

            Vector3 min = obj.BoundingBox.Min;
            Vector3 max = obj.BoundingBox.Max;

            Node initial = WorldToNode(min);
            Node final = WorldToNode(max);

            int key = 0;

            for (int w = initial.X; w <= final.X && w < Width; w++)
            {
                for (int h = initial.Y; h <= final.Y && h < Height; h++)
                {
                    if (key < previous.Length)
                    {
                        if (Contents[w, h] == null
                            && Touches(new Node(w, h), obj.BoundingSphere.Center, obj.BoundingSphere.Radius))
                        {
                            #region Debug
#if DebugPathfinding
                            TileSprites[w, h].Visible = true;
#endif
                            #endregion

                            Contents[w, h] = obj;
                            placed = true;
                            previous[key] = new Node(w, h);
                            key++;
                        }
                    }
                }
            }

            return placed;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public bool IsNodeSafe(ref Node node)
        {
            return node.X >= 0 && node.X < Width
                && node.Y >= 0 && node.Y < Height;
        }

        public void MakeNodeSafe(ref Node node)
        {
            if (node.X < 0) node.X = 0;
            else if (node.X >= Width) node.X = Width - 1;

            if (node.Y < 0) node.Y = 0;
            else if (node.Y >= Height) node.Y = Height - 1;
        }

        public Node WorldToNode(Vector3 position)
        {
            int x = (int)((position.X - Offset.X) / (float)SpanX);
            int y = (int)(((position.Y - Offset.Y) - (x % 2 * OddY)) / (float)SpanY);

            Node node = new Node(x, y);
            MakeNodeSafe(ref node);
            return node;
        }

        public Vector2 NodeToScreen(Node node)
        {
            Vector2 test = new Vector2();
            test.X = Offset.X + (node.X * SpanX);
            test.Y = Offset.Y + (node.Y * SpanY - (node.X % 2 * OddY));
            test.X += SpanX / 2f;
            test.Y += SpanY / 2f;
            return test;
        }
        public Vector3 NodeToScreen3(Node node)
        {
            return new Vector3(NodeToScreen(node), 0f);
        }

        public Rectangle GetDrawRect(Node node)
        {
            Rectangle drawrect = new Rectangle();
            drawrect.X = node.X * SpanX;
            drawrect.Y = node.Y * SpanY - (node.X % 2 * OddY);
            drawrect.Width = SpanX;
            drawrect.Height = SpanY;
            return drawrect;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}