using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#if !Xbox
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endif
using ChiseDrive;
using ChiseDrive.Graphics;
using ChiseDrive.Cameras;
using ChiseDrive.Pathfinding;
using ChiseDrive.Units;

namespace ChiseDrive.World
{
    public class GameWorld
    {
        // ************************************************** PUBLIC METHODS //

        /// <summary>
        /// Returns the object pointer to the contents of the world location.
        /// </summary>
        /// <param name="worldLocation">The 3D Location (Z ignored)</param>
        /// <returns>Contents of the position (null if empty)</returns>
        public object GetContents(Vector3 worldLocation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the object pointer to the contents of the world location.
        /// </summary>
        /// <param name="worldLocation">The 2D World Location</param>
        /// <returns>Contents of the position (null if empty)</returns>
        public object GetContents(Vector2 worldLocation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an object underneath the screen position.
        /// </summary>
        /// <param name="screenLocation">The 2D Screen Location</param>
        /// <returns>Contents of the screen (null if empty)</returns>
        public object GetContentsFromScreen(Vector2 screenLocation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a path for the unit to follow.
        /// </summary>
        /// <param name="unit">Unit to move.</param>
        /// <param name="destination">Destination to move to.</param>
        /// <returns>A valid path, else null.</returns>
        public Path BuildPath(Unit unit, Vector3 destination)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// If true, then the world is visible/active.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }
        bool enabled = false;

        public static readonly Vector3 InvalidPosition = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        public Rectangle Dimensions { get; set; }

        /// <summary>
        /// Creates a GameWorld.
        /// </summary>
        /// <param name="game"></param>
        public GameWorld(GameWorldSettings settings)
        {

        }

        public static GameWorld Load(ChiseDriveGame game, string file)
        {
            GameWorld world = game.Content.Load<GameWorld>(file);
            world.Initialize(game);
            return world;
        }

        public void Close()
        {
            Visible = false;
        }

        List<IWorldComponent> components = new List<IWorldComponent>(50);
        public List<IWorldComponent> Components
        {
            get
            {
                return components;
            }
            set
            {
                components = value;
            }
        }
        public bool Visible
        {
            get
            {
                if (components.Count > 0)
                {
                    return components[0].Visible;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                foreach (IWorldComponent c in components)
                {
                    c.Visible = value;
                }
            }
        }

        Color ambient;
        public Color AmbientLight
        {
            get
            {
                return ambient;
            }
            set
            {
                ambient = value;
            }
        }
        public ChiseDriveGame Game { get; set; }

        public GameWorld()
        {
        }

        public void Initialize(ChiseDriveGame game)
        {
            Rectangle bounds = new Rectangle();
            if (components.Count > 0)
            {
                foreach (IWorldComponent c in components)
                {
                    c.Initialize(game);
                    c.SizeBounds(ref bounds);
                }
            }
            Dimensions = bounds;
            Visible = true;
            Game = game;
        }

        public Vector3 CorrectForBounds(Vector3 initial)
        {
            Vector3 newposition = InvalidPosition;
            foreach (IWorldComponent c in components)
            {
                Vector3 testposition = c.CorrectForBounds(initial);
                if (testposition == initial)
                {
                    return initial;
                }
                if (testposition != InvalidPosition)
                {
                    float distance1 = Vector3.Distance(testposition, initial);
                    float distance2 = Vector3.Distance(newposition, initial);

                    if (distance1 < distance2) newposition = testposition;
                }
            }

            return newposition;
        }

        public Vector3 CorrectForHeight(Vector3 initial)
        {
            foreach (IWorldComponent c in components)
                initial = c.CorrectForHeight(initial);

            return initial;
        }

        public void Update(Time elapsed)
        {
            if (Game == null) throw new Exception("GameWorld.Initialize has not been called yet.");
            foreach (IWorldComponent c in components)
            {
                c.Update(elapsed);
            }
        }

#if !Xbox
        public void Write(ContentWriter output)
        {
            output.Write(components.Count);
            foreach (IWorldComponent c in components)
            {
                output.Write(c.ToString());
                c.Write(output);
            }
        }
#endif
        public void Read(ContentReader input)
        {
            int count = input.ReadInt32();

            while (count > 0)
            {
                count--;
                string type = input.ReadString();

                switch (type)
                {
                    case "ChiseDrive.World.SceneObject":
                        SceneObject so = new SceneObject();
                        so.Read(input);
                        Components.Add(so);
                        break;
                    case "ChiseDrive.World.HeightmapMesh":
                        HeightmapMesh hm = new HeightmapMesh();
                        hm.Read(input);
                        Components.Add(hm);
                        break;
                    case "ChiseDrive.World.TextureSkybox":
                        TextureSkybox ts = new TextureSkybox();
                        ts.Read(input);
                        Components.Add(ts);
                        break;
                    case "ChiseDrive.World.SunLight":
                        SunLight sl = new SunLight();
                        sl.Read(input);
                        Components.Add(sl);
                        break;
                }
            }
        }
    }
}