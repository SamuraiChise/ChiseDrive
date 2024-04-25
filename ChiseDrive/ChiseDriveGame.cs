using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using ChiseDrive.Cameras;
using ChiseDrive.Debug;
using ChiseDrive.Graphics;
using ChiseDrive.Input;
using ChiseDrive.Menu;
using ChiseDrive.Motion;
using ChiseDrive.Particles;
using ChiseDrive.Pathfinding;
using ChiseDrive.Physics;
using ChiseDrive.Storage;
using ChiseDrive.Units;
using ChiseDrive.World;

namespace ChiseDrive
{
    public class ChiseDriveGame : Microsoft.Xna.Framework.Game
    {
        #region Debug
#if Debug
        public struct GameDebugSetup
        {
            public bool ShowFramerate;
            public bool ShowMetrics;
            public bool ShowTitleSafe;
        };
        public GameDebugSetup DebugSettings = new GameDebugSetup();

        Texture2D SafeZoneTexture;
#endif
        #endregion

        public static bool Force2D { get; set; }
        String title;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameWorld world;
        Camera camera;
        Settings settings;
        MenuDevice menu;
        PostProcessComponent postprocess;
        GameState gamestate;
        Time timeSkew = Time.FromFrames(1f);

        GameBoard board = null;

        List<Player> players = new List<Player>(16);
        List<Unit> units = new List<Unit>(300);
        List<UnitEX> unitsEX = new List<UnitEX>(100);
        List<IDrawableEffect3D> drawableEffects = new List<IDrawableEffect3D>(30);

        AudioEngine audioengine;
        AudioListener audiolistener;
        SoundBank soundbank;
        WaveBank wavebank;

        public AudioEngine AudioEngine { get { return audioengine; } set { audioengine = value; } }
        public WaveBank WaveBank { get { return wavebank; } set { wavebank = value; } }
        public SoundBank SoundBank { get { return soundbank; } set { soundbank = value; } }
        public AudioListener AudioListener { get { return audiolistener; } set { audiolistener = value; } }

        public string Title { get { return title; } }


        public Time TimeSkew { get { return timeSkew; } set { timeSkew = value; } }

        public GameWorld World
        {
            get
            {
                return world;
            }
            set
            {
                world = value;
            }
        }
        public Camera Camera
        {
            get
            {
                return camera;
            }
            set
            {
                camera = value;
            }
        }
        public PostProcessSettings PostProcessSettings
        {
            set
            {
                if (value == null && postprocess != null)
                {
                    postprocess.Dispose();
                    postprocess = null;
                }
                if (value != null && postprocess == null)
                {
                    postprocess = new PostProcessComponent(this);
                }
                if (postprocess != null)
                {
                    postprocess.Settings = value;
                }
            }
        }
        public MenuDevice Menu
        {
            get
            {
                return menu;
            }
        }
        public SpriteBatch SpriteBatch
        {
            get
            {
                return spriteBatch;
            }
        }
        public Settings Settings
        {
            get
            {
                return settings;
            }
            set
            {
                settings = value;
            }
        }
        public GameState GameState
        {
            get
            {
                return gamestate;
            }
            set
            {
                GameState previous = gamestate;
                gamestate = value;
                if (gamestate == GameState.PauseGame)
                {
                    ControllerVibration.PauseAll();
                }
                if (previous == GameState.PauseGame
                    && gamestate == GameState.RunGame)
                {
                    ControllerVibration.ResumeAll();
                }
                if (gamestate == GameState.EndGame
                    || gamestate == GameState.LoadGame)
                {
                    ControllerVibration.ResetAll();
                }
            }
        }
        public List<Player> Players
        {
            get
            {
                return players;
            }
        }
        public List<Unit> Units
        {
            get
            {
                return units;
            }
        }
        public List<UnitEX> UnitsEX
        {
            get
            {
                return unitsEX;
            }
        }
        public List<IDrawableEffect3D> DrawableEffects
        {
            get
            {
                return drawableEffects;
            }
        }
        public float CollisionAccuracy
        {
            get
            {
                if (board != null) return board.Accuracy;
                else return 1f;
            }
        }
        public GameBoard GameBoard
        {
            get
            {
                return board;
            }
            set
            {
                if (board != null)
                    Components.Remove(board);
                board = value;
                if (board != null)
                    Components.Add(board);
            }
        }

        public ChiseDriveGame(string title)
        {
            this.title = title;
            this.settings = new Settings(title);
            graphics = new GraphicsDeviceManager(this);
            gamestate = GameState.Initialize;
        }

        public ChiseDriveGame(string title, Settings settings)
        {
            this.title = title;
            this.settings = settings;
            graphics = new GraphicsDeviceManager(this);
            gamestate = GameState.Initialize;
        }

        /// <summary>
        /// </summary>
        protected override void Initialize()
        {
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = settings.PreferredWidth;
            graphics.PreferredBackBufferHeight = settings.PreferredHeight;
            graphics.PreferMultiSampling = settings.PreferredMultiSample == 1 ? false : true;
            graphics.PreparingDeviceSettings +=
                new EventHandler<PreparingDeviceSettingsEventArgs>(
                    PrepareGraphicsDeviceSettings);

            graphics.ApplyChanges();

            spriteBatch = new SpriteBatch(GraphicsDevice);

            #region Debug
#if Debug
            Debug.DebugText.Initialize(Content.Load<SpriteFont>("Fonts/Debug"), spriteBatch);
            Debug.DebugText.Active = true;
            SafeZoneTexture = Content.Load<Texture2D>("Textures/white");
#endif
            #endregion

//#if !WINDOWS
            Components.Add(new GamerServicesComponent(this));
//#endif

            if (settings.SimulateTrialMode)
            {
                Guide.SimulateTrialMode = true;
            }

            menu = new MenuDevice(this);

            ParticleSystem.Game = this;

            audiolistener = new AudioListener();

            UnitEvent.Game = this;

            Time.InitializeStrings();

            base.Initialize();
        }

        void PrepareGraphicsDeviceSettings(object sender,
            PreparingDeviceSettingsEventArgs e)
        {
            PresentationParameters pp = e.GraphicsDeviceInformation.PresentationParameters;

#if XBOX
            pp.MultiSampleQuality = 0;
            pp.MultiSampleType = MultiSampleType.FourSamples;
#else
            int quality = 0;
            GraphicsAdapter adapter = e.GraphicsDeviceInformation.Adapter;
            SurfaceFormat format = adapter.CurrentDisplayMode.Format;
            // Check for 4xAA
            if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format,
                false, MultiSampleType.FourSamples, out quality))
            {
                // even if a greater quality is returned, we only want quality 0
                pp.MultiSampleQuality = 0;
                pp.MultiSampleType =
                    MultiSampleType.FourSamples;
            }
            // Check for 2xAA
            else if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, 
                format, false, MultiSampleType.TwoSamples, out quality))
            {
                // even if a greater quality is returned, we only want quality 0
                pp.MultiSampleQuality = 0;
                pp.MultiSampleType =
                    MultiSampleType.TwoSamples;
            }
#endif
        }

        /// <summary>
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            camera = new Camera(GraphicsDevice);
        }

        /// <summary>
        /// </summary>
        protected virtual void UnloadContent()
        {
        }

        public virtual void AnnounceEvent(UnitEvent e)
        {
            foreach (Player p in Players)
            {
                p.RecieveEvent(e);
            }
        }

        protected virtual void ProcessMenuInput(Instruction instruction)
        {

        }

        void CheckMenuInput(Time elapsed)
        {
            InstructionStack action = menu.GetWaitingInstructions();

            while (!action.IsEmpty)
            {
                ProcessMenuInput(action.Pop());
            }
        }

        public List<T> FindAll<T>()
        {
            List<T> returnlist = new List<T>();

            foreach (object o in Units)
            {
                if (o is T) returnlist.Add((T)o);
            }

            return returnlist;
        }

        public enum Contents
        {
            AllIntersects,
            ClosestToCenter,
        };

        public List<Unit> GetContents(Vector2 location, float radius, Contents style)
        {
            List<Unit> retvalue = new List<Unit>();

            foreach (Unit u in Units)
            {
                Vector2 unitlocation = new Vector2(u.Position.X, u.Position.Y);
                float distance = Vector2.Distance(unitlocation, location);
                distance -= u.CollisionRadius;

                if (distance < radius) retvalue.Add(u);
            }

            if (style == Contents.ClosestToCenter)
            {
                float closest = radius * 2f;
                Unit lastUnit = null;

                foreach (Unit u in retvalue)
                {
                    Vector2 unitlocation = new Vector2(u.Position.X, u.Position.Y);
                    float distance = Vector2.Distance(unitlocation, location);

                    if (distance < closest)
                    {
                        closest = distance;
                        lastUnit = u;
                    }
                }

                retvalue.Clear();

                if (lastUnit != null) retvalue.Add(lastUnit);
            }

            return retvalue;
        }

        protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Time elapsed = Time.FromGameTime(gameTime);

            #region Performance Debug
#if Debug
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) Exit();
            if (GamePad.GetState(PlayerIndex.Two).Buttons.Back == ButtonState.Pressed) Exit();

            DateTime update = new DateTime();
            if (DebugSettings.ShowMetrics || DebugSettings.ShowFramerate)
            {
                update = DateTime.Now;
                DebugText.Update(elapsed);
                UpdatePerformance(elapsed);
            }
#endif
            #endregion
            
            CheckMenuInput(elapsed);
            ControllerVibration.UpdateAll(elapsed);
            
            switch (gamestate)
            {
                case GameState.Initialize: UpdateInitialize(elapsed); break;
                case GameState.RunMenu: UpdateMenu(elapsed); break;
                case GameState.LoadGame: UpdateLoad(elapsed); break;
                case GameState.RunGame: UpdateGame(elapsed * timeSkew); break;
                case GameState.PauseGame: UpdatePause(elapsed); break;
                case GameState.EndGame: UpdateEndgame(elapsed); break;
            }
            
            audiolistener.Position = Camera.Position;
            audiolistener.Up = Camera.Up;
            audiolistener.Forward = Camera.Forward;
            audiolistener.Velocity = Camera.Velocity;

            if (AudioEngine != null) AudioEngine.Update();
            
            #region Performance Debug
#if Debug
            if (DebugSettings.ShowMetrics || DebugSettings.ShowFramerate)
            {
                UpdateFramesSinceCalc++;
                DebugText.CloseWrite();
                TotalTime += DateTime.Now - update;
            }
#endif
            #endregion
            
            base.Update(gameTime);
        }

        protected virtual void UpdateInitialize(Time elapsed)
        {
            if (!Sprite3D.Initialized)
                Sprite3D.Initialize(new XnaReference(Content, GraphicsDevice, SpriteBatch));
        }

        protected virtual void UpdateMenu(Time elapsed)
        {
            Camera.Update(elapsed);
            if (World != null) World.Update(elapsed);
            foreach (Unit unit in Units)
            {
                if (unit.Active) unit.Update(elapsed);
            }
            foreach (UnitEX unit in UnitsEX)
            {
                if (unit.Active) unit.Update(elapsed);
            }
            LitObject.UpdateAll(elapsed, Camera);
        }

        protected virtual void UpdateLoad(Time elapsed)
        {
        }

        protected virtual void UpdateGame(Time elapsed)
        {
            Metrics.OpenMetric("Update.Players");
            foreach (Player player in Players)
            {
                player.Update(elapsed);
            }
            Metrics.CloseMetric("Update.Players");

            Metrics.OpenMetric("Update.Camera");
            Camera.Update(elapsed);
            Metrics.CloseMetric("Update.Camera");

            Metrics.OpenMetric("Update.World");
            if (World != null) World.Update(elapsed);
            Metrics.CloseMetric("Update.World");


            Metrics.OpenMetric("Update.LitObject");
            LitObject.UpdateAll(elapsed, Camera);
            Metrics.CloseMetric("Update.LitObject");
            
            Metrics.OpenMetric("Update.Units");
            foreach (Unit unit in Units)
            {
                if (unit.Active) unit.Update(elapsed);
            }
            foreach (UnitEX unit in UnitsEX)
            {
                if (unit.Active) unit.Update(elapsed);
            }
            Metrics.CloseMetric("Update.Units");

            Metrics.OpenMetric("Update.Bullets");
            Bullet.UpdateAll(elapsed);
            Bullet.ResolveCollisions(this, elapsed);
            Laser.UpdateAll(elapsed);
            Laser.ResolveCollisions(this, elapsed);
            Metrics.CloseMetric("Update.Bullets");

            Metrics.OpenMetric("Update.Particles");
            ParticleSystem.UpdateAll(elapsed);
            Metrics.CloseMetric("Update.Particles");

            Metrics.OpenMetric("Update.Explosions");
            Explosion.UpdateAll(elapsed);
            Metrics.CloseMetric("Update.Explosions");
            
        }

        protected virtual void UpdatePause(Time elapsed)
        {
        }

        /// <summary>
        /// Close the current world, shut down all the units
        /// and everything else that is required to end a level.
        /// </summary>
        /// <param name="elapsed"></param>
        protected virtual void UpdateEndgame(Time elapsed)
        {
            if (World != null) World.Close();

            ControllerVibration.ResetAll();

            foreach (UnitEX unit in UnitsEX)
            {
                unit.Body.Visible = false;
                unit.Status = UnitEX.UnitStatus.None;
            }
        }


        protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            #region Performance Debug
#if Debug
            DateTime draw = new DateTime();
            if (DebugSettings.ShowMetrics || DebugSettings.ShowFramerate)
            {
                draw = DateTime.Now;
                DebugText.OpenWrite();
            }
#endif
            #endregion

            GraphicsDevice.Clear(Color.White);
            Camera.GenerateCameraMatrixes();
            
            switch (gamestate)
            {
                case GameState.Initialize: DrawInitialize(); break;
                case GameState.RunMenu: DrawMenu(); break;
                case GameState.LoadGame: DrawLoad(); break;
                case GameState.RunGame: DrawGame(); break;
                case GameState.PauseGame: DrawPause(); break;
                case GameState.EndGame: DrawEndgame(); break;
            }
            
            // Clear the changed flags for the next pass
            PointLight.ClearAllChanged();

#if Debug
            if (DebugSettings.ShowTitleSafe &&
                SafeZoneTexture != null) 
                DrawSafeZone(SafeZoneTexture);
#endif
            
            base.Draw(gameTime);
            
            #region Performance Debug
#if Debug
            if (DebugSettings.ShowMetrics || DebugSettings.ShowFramerate)
            {
                DrawFramesSinceCalc++;
                DebugText.Draw();
                TotalTime += DateTime.Now - draw;
            }
#endif
            #endregion
        }

        protected virtual void DrawInitialize()
        {
            Drawable2D.DrawAll(SpriteBatch);
        }

        protected virtual void DrawMenu()
        {
            DrawGame();
        }

        protected virtual void DrawLoad()
        {
        }

        protected virtual void DrawGame()
        {
            if (postprocess != null && postprocess.RenderNormals)
            {
                postprocess.SetNormalTargets();
                DrawGamePass(Camera, Visibility.Normal);
            }
            if (postprocess != null) postprocess.SetRenderTargets();
            
            DrawGamePass(Camera, Visibility.Opaque);

            Metrics.OpenMetric("Draw.ApplyPostProcess");
            if (postprocess != null) postprocess.ApplyPostProcess();
            Metrics.CloseMetric("Draw.ApplyPostProcess");
            
            Metrics.OpenMetric("Draw.Players");
            foreach (Player player in Players)
            {
                player.Draw();
            }
            Metrics.CloseMetric("Draw.Players");
           
            Metrics.OpenMetric("Draw.Drawable2D");
            Drawable2D.DrawAll(SpriteBatch);
            Metrics.CloseMetric("Draw.Drawable2D");
        }

        protected virtual void DrawPause()
        {
            DrawGame();
        }

        protected virtual void DrawEndgame()
        {
            Drawable2D.DrawAll(SpriteBatch);
        }

        protected void DrawGamePass(Camera camera, Visibility visiblity)
        {
            Metrics.OpenMetric("DrawGamePass");
            Metrics.OpenMetric("Draw.Skybox");
            SkyboxPane.DrawAll(GraphicsDevice, camera, visiblity);
            Metrics.CloseMetric("Draw.Skybox");

            Metrics.OpenMetric("Draw.3DPrimitive");
            DrawablePrimitive3D.DrawAll(GraphicsDevice, camera, visiblity);
            Metrics.CloseMetric("Draw.3DPrimitive"); 
            
            Metrics.OpenMetric("Draw.LitObject");
            LitObject.DrawAll(GraphicsDevice, camera, visiblity);
            Metrics.CloseMetric("Draw.LitObject");

            #region TODO: Change SpaceDebris to be LitObjects (or UnlitObjects)
            //Metrics.OpenMetric("Draw.Debris");
            //SpaceDebris.DrawAll(device, camera);
            //Metrics.CloseMetric("Draw.Debris");
            #endregion
            
            Metrics.OpenMetric("Draw.Bullets");
            Bullet.DrawAll(GraphicsDevice, camera);
            Laser.DrawAll(GraphicsDevice, camera);
            Metrics.CloseMetric("Draw.Bullets");

            Metrics.OpenMetric("Draw.ParticleSystem");
            ParticleSystem.DrawAll(GraphicsDevice, camera);
            Metrics.CloseMetric("Draw.ParticleSystem");

            Metrics.OpenMetric("Draw.PointSprites");
            PointSpriteD.DrawAll(GraphicsDevice, camera);
            Metrics.CloseMetric("Draw.PointSprites");

            Metrics.OpenMetric("Draw.IDrawableEffect3D");
            foreach (IDrawableEffect3D effect in drawableEffects) effect.Draw();
            Metrics.CloseMetric("Draw.IDrawableEffect3D");
            Metrics.CloseMetric("DrawGamePass");
        }

        #region Debug
#if Debug
        public void DrawSafeZone(Texture2D texture)
        {
            Rectangle titlesafe = GraphicsDevice.Viewport.TitleSafeArea;
            Rectangle imagesafe = titlesafe;
            imagesafe.Width += imagesafe.X;
            imagesafe.X /= 2;
            imagesafe.Height += imagesafe.Y;
            imagesafe.Y /= 2;
            //titlesafe = imagesafe;

            spriteBatch.Begin();

            Rectangle top = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, titlesafe.Y);
            spriteBatch.Draw(texture, top, Color.White);

            Rectangle left = new Rectangle(0, titlesafe.Y, titlesafe.X, titlesafe.Height);
            spriteBatch.Draw(texture, left, Color.White);

            Rectangle right = new Rectangle(titlesafe.X + titlesafe.Width, titlesafe.Y, GraphicsDevice.Viewport.Width - titlesafe.X - titlesafe.Width, titlesafe.Height);
            spriteBatch.Draw(texture, right, Color.White);

            Rectangle bottom = new Rectangle(0, titlesafe.Y + titlesafe.Height, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height - titlesafe.Height - titlesafe.Y);
            spriteBatch.Draw(texture, bottom, Color.White);

            spriteBatch.End();
        }

        DateTime LastTimeCalc = new DateTime();
        TimeSpan TotalTime = TimeSpan.Zero;
        TimeSpan TotalLast = TimeSpan.Zero;
        int UpdateFramesSinceCalc = 0;
        int DrawFramesSinceCalc = 0;
        int UpdatesPerSecond = 0;
        int DrawsPerSecond = 0;
        bool RunDrawMetrics = true;
        bool RunUpdateMetrics = false;
        List<Metric> Performance = null;

        int AveragePolyCount = 0;

        void UpdatePerformance(Time elapsed)
        {
            if (DebugSettings.ShowMetrics) Metrics.Active = true;
            else Metrics.Active = false;

            DateTime checkfps = DateTime.Now;
            TimeSpan spanfps = checkfps - LastTimeCalc;

            if (spanfps.TotalSeconds > 1)
            {
                int temp = DrawableMesh.GetPolygonCount();
                AveragePolyCount = DrawFramesSinceCalc > 0 ? temp / DrawFramesSinceCalc : 0;

                LastTimeCalc = checkfps;
                UpdatesPerSecond = (int)((double)UpdateFramesSinceCalc / spanfps.TotalSeconds);
                DrawsPerSecond = (int)((double)DrawFramesSinceCalc / spanfps.TotalSeconds);

                UpdateFramesSinceCalc = 0;
                DrawFramesSinceCalc = 0;

                Performance = Metrics.TopOffenders(5, Metrics.TotalTime);
                TotalLast = TotalTime;
            }

            if (DebugText.Initialized && DebugSettings.ShowFramerate)
            {
                if (UpdatesPerSecond < 30 || DrawsPerSecond < 30)
                {
                    DebugText.Write("Running Slow!  Updates Per Second: " + UpdatesPerSecond +
                        " Draws Per Second: " + DrawsPerSecond +
                        " Using CPU: " + TotalLast.TotalMilliseconds * 100 / 1000 + "%");
                }
                else
                {
                    DebugText.Write("Running Good.  Updates Per Second: " + UpdatesPerSecond +
                        " Draws Per Second: " + DrawsPerSecond +
                        " Using CPU: " + TotalLast.TotalMilliseconds * 100 / 1000 + "%");
                }
            }

            if (DebugSettings.ShowMetrics)
            {
                if (Performance != null)
                {
                    if (TotalTime != TimeSpan.Zero)
                    {
                        foreach (Metric m in Performance)
                        {
                            if (DebugText.Initialized)
                            {
                                DebugText.Write(m.Name + ": Total CPU: "
                                    + (int)(m.TotalTime.TotalMilliseconds * 100 / 1000) + "%"
                                    + " Ticks/Call (" + (int)m.AverageTimeTicks + ")"
                                    + " Calls/Second (" + m.TotalCalls + ")");
                            }
                        }
                    }
                }
            }
            if (DebugSettings.ShowMetrics)
            {
                DebugText.Write("Average Lights Per Render: " + LitObject.AverageLightsPerRender + " Average Polygons Per Render: " + AveragePolyCount);
            }

            if (spanfps.TotalSeconds > 1)
            {
                Metrics.ResetAll();
                TotalTime = TimeSpan.Zero;
            }
        }
#endif
        #endregion
    }
}