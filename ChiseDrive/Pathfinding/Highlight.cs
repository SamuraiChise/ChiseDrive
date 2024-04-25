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


namespace ChiseDrive.Pathfinding
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Highlight : Microsoft.Xna.Framework.DrawableGameComponent
    {
        SpriteBatch mysprites;
        Texture2D HighlightTexture;
        Rectangle HighlightRectangle = new Rectangle();
        List<Node> Nodes = new List<Node>(GameBoard.MaxEntries);
        GameBoard board;

        public Highlight(Game game)
            : base(game)
        {
            DrawOrder = ChiseDrive.DrawOrder.Path;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            mysprites = new SpriteBatch(Game.GraphicsDevice);
            HighlightTexture = Game.Content.Load<Texture2D>("Textures/hexhighlight");
            HighlightRectangle.X = 0;
            HighlightRectangle.Y = 0;
            HighlightRectangle.Width = HighlightTexture.Width;
            HighlightRectangle.Height = HighlightTexture.Height;
            base.Initialize();
        }

        void BuildMoveNodeRecursive(Node parent, int range)
        {
            if (range <= 0) return;

            for (int i = 0; i < 6; i++)
            {
                Node node = new Node(parent, (Node.Step)i);
                if (board.GetContents(node) == null)
                {
                    if (!Nodes.Contains(node)) Nodes.Add(node);
                    BuildMoveNodeRecursive(node, range - 1);
                }
            }
        }

        public void BuildMoveZone(GameBoard board, Node first, int range)
        {
            this.board = board;

            Nodes.Clear();
            BuildMoveNodeRecursive(first, range);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (board != null)
            {
                mysprites.Begin();

                Color drawcolor = Color.Yellow;
                drawcolor.A = 100;

                foreach (Node n in Nodes)
                {
                    Rectangle drawrect = board.GetDrawRect(n);
                    mysprites.Draw(HighlightTexture, drawrect, drawcolor);
                }

                mysprites.End();
            }
            base.Draw(gameTime);
        }
    }
}