using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using UntitledSandbox.Common;
using UntitledSandbox.PlayerData;
using UntitledSandbox.Managers;
using UntitledSandbox.Terrain;
using UntitledSandbox.Terrain.Renderers;

namespace UntitledSandbox
{
	public class Game : Microsoft.Xna.Framework.Game
	{
		public static Game Instance { get; private set; }

		public SpriteBatch SpriteBatch { get; private set; }
		public ContentManager ContentManager { get; private set; }
		public UIManager UIManager { get; private set; }

		public GraphicsDeviceManager Graphics { get; set; }
		public Player Player { get; set; }

		public Renderer SkyRenderer { get; set; }
		public Renderer TerrainRenderer { get; set; }

		public Game()
		{
			Instance = this;

			this.Graphics = new GraphicsDeviceManager(this);
			this.Graphics.PreferMultiSampling = true;

			this.SkyRenderer = new SkySphereRenderer();

			// Something needs to be done about the scaling
			//this.TerrainRenderer = new NoiseTerrainRenderer();
			this.TerrainRenderer = new BlockTerrainRenderer();

			Content.RootDirectory = "Content";
		}

		// TODO Look into GameServiceContainer
		protected override void Initialize()
		{
			// [StartupLogic] Enable Window Resizing
			this.Window.AllowUserResizing = true;
			this.Window.ClientSizeChanged += new EventHandler<EventArgs>(this.Window_ClientSizeChanged);

			// [StartupLogic] Initialize Managers
			this.ContentManager = new ContentManager();
			this.UIManager = new UIManager();

			// [StartupLogic] Graphic Devices & Settings
			this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);

			// [StartupLogic] Create Player & Update
			Mouse.SetPosition(this.GraphicsDevice.Viewport.Width / 2, this.GraphicsDevice.Viewport.Height / 2);
			this.Player = new Player();

			this.Player.Camera.ViewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, -100), Vector3.Zero, Vector3.Up);
			this.Player.Camera.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.3f, 1000.0f);

			// [XNA]
			base.Initialize();
		}

		protected override void LoadContent()
		{
			this.SkyRenderer.Load();
			this.TerrainRenderer.Load();
		}

		protected override void UnloadContent()
		{
			// [[ExitLogic] Could save here in the future
		}

		protected override void Update(GameTime gameTime)
		{
			// [GameLogic] Global Controls
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) this.Exit();
			if (Keyboard.GetState().IsKeyDown(Keys.Space)) this.Exit();

			// [GameLogic] Player Controls
			this.Player.Controls.ProcessInput((float) gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f);
			
			// [GameLogic]
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			this.GraphicsDevice.Clear(Color.Transparent);
			//this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Transparent, 1.0f, 0);

			this.SkyRenderer.Draw();
			this.TerrainRenderer.Draw();

			SpriteBatch.Begin();
			UIManager.drawWindows();
			SpriteBatch.End();

			base.Draw(gameTime);
		}

		void Window_ClientSizeChanged(object sender, EventArgs e)
		{
			this.Player.UpdateMouseState();
		}

		public static void CenterMouse()
		{
			Mouse.SetPosition(Instance.GraphicsDevice.Viewport.Width / 2, Instance.GraphicsDevice.Viewport.Height / 2);
		}
	}
}
