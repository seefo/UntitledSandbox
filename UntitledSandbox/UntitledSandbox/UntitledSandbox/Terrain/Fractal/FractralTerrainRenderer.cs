﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UntitledSandbox.PlayerData;
using UntitledSandbox.Common.Utils;
using UntitledSandbox.Terrain.Fractal;

namespace UntitledSandbox.Terrain.Fractal
{
	public class FractralTerrainRenderer : Renderer
	{
		private readonly bool DRAW_BOUNDING_BOXES = false;

		public const string EFFECT = "effects/Series4Effects";
		public const string TEXTURE = "textures/grass";
		public const float AMBIENT_LIGHT = 0.2f;
		public static Vector3 LIGHT_DIRECTION
		{
			get { return new Vector3(1f, 2, 1f); }
		}

		private int TileSize { get; set; }
		private VertexDeclaration TerrainVertexDeclaration { get; set; }

		private Tile[,] Tiles;

		List<VertexPositionColor[]> boxes = new List<VertexPositionColor[]>();

		short[] bBoxIndices = {
		        0, 1, 1, 2, 2, 3, 3, 0, // Front edges
		        4, 5, 5, 6, 6, 7, 7, 4, // Back edges
		        0, 4, 1, 5, 2, 6, 3, 7 // Side edges connecting front and back
		};

		public FractralTerrainRenderer()
		{
		}

		public override void Load()
		{
			this.ContentManager.Load<Texture2D>(TEXTURE);
			this.ContentManager.Load<Effect>(EFFECT);

			this.TileSize = 129;
			Random random = new Random();
			float heightScale = 200f;
			float roughness = 0.995f;
			int numTiles = 10;

			this.Tiles = new Tile[numTiles, numTiles];

			for (int i = 0; i < numTiles; i++)
			{
				for (int j = 0; j < numTiles; j++)
				{
					//heightScale -= random.Next(250);

					this.Tiles[i, j] = new Tile(this.TileSize, i, j);
					this.Tiles[i, j].Seed = random.Next();
					this.Tiles[i, j].HeightScale = heightScale;
					this.Tiles[i, j].Roughness = roughness;

					this.Tiles[i, j].SeedMap(
						TryFetchTile(i, j + 1),
						TryFetchTile(i + 1, j),
						TryFetchTile(i, j - 1),
						TryFetchTile(i - 1, j)
					);

					this.Tiles[i, j].GenerateMap();

					//heightScale = 300;
				}
			}

			foreach (Tile tile in this.Tiles)
				tile.LoadVertices();

			if (DRAW_BOUNDING_BOXES)
			{
				foreach (Tile tile in this.Tiles)
				{
					BoundingBox box = tile.Bounds;
					Vector3[] corners = box.GetCorners();
					VertexPositionColor[] primitiveList = new VertexPositionColor[corners.Length];

					// Assign the 8 box vertices
					for (int i = 0; i < corners.Length; i++)
					{
						primitiveList[i] = new VertexPositionColor(corners[i], Color.White);
					}

					boxes.Add(primitiveList);
				}
			}
		}

		private Tile TryFetchTile(int i, int j)
		{
			try
			{
				return this.Tiles[i, j];
			}
			catch (IndexOutOfRangeException)
			{
				return null;
			}
		}

		public override void Draw()
		{
			RasterizerState state = new RasterizerState();
			state.FillMode = Controls.IsWire ? FillMode.WireFrame : FillMode.Solid;
			state.CullMode = CullMode.CullClockwiseFace;
			this.GraphicsDevice.RasterizerState = state;

			Effect effect = this.ContentManager.Get<Effect>(EFFECT);
			effect.CurrentTechnique = effect.Techniques["Textured"];
			effect.Parameters["xTexture"].SetValue(this.ContentManager.Get<Texture2D>(TEXTURE));

			effect.Parameters["xView"].SetValue(this.Player.Camera.ViewMatrix);
			effect.Parameters["xProjection"].SetValue(this.Player.Camera.ProjectionMatrix);

			effect.Parameters["xEnableLighting"].SetValue(true);
			effect.Parameters["xAmbient"].SetValue(AMBIENT_LIGHT);
			effect.Parameters["xLightDirection"].SetValue(LIGHT_DIRECTION);

			//BasicEffect effect = new BasicEffect(this.GraphicsDevice);
			//effect.LightingEnabled = true;
			//effect.AmbientLightColor = new Vector3(AMBIENT_LIGHT);
			//effect.DirectionalLight0.Enabled = true;
			//effect.DirectionalLight0.Direction = LIGHT_DIRECTION;
			//effect.FogEnabled = false;
			//effect.FogStart = 300f;
			//effect.FogEnd = 1000f;
			//effect.FogColor = Color.Black.ToVector3();
			//effect.TextureEnabled = true;
			//effect.Texture = Game.Instance.ContentManager.Get<Texture2D>(TEXTURE);
			//effect.Projection = this.Player.Camera.ProjectionMatrix;
			//effect.View = this.Player.Camera.ViewMatrix;

			int tiles = 0;
			foreach (Tile tile in this.Tiles)
			{
				if (tile.IsInView)
				{
					tiles++;
					effect.Parameters["xWorld"].SetValue(tile.TranslationMatrix);
					//effect.World = tile.TranslationMatrix;
					
					foreach (EffectPass pass in effect.CurrentTechnique.Passes)
					{
						pass.Apply();
						tile.Draw(this.GraphicsDevice);
					}
				}
			}

			Game.Instance.Window.Title = tiles.ToString();

			if (DRAW_BOUNDING_BOXES)
			{
				BasicEffect boxEffect = new BasicEffect(this.GraphicsDevice);
				boxEffect.World = Matrix.Identity;
				boxEffect.View = this.Player.Camera.ViewMatrix;
				boxEffect.Projection = this.Player.Camera.ProjectionMatrix;
				boxEffect.TextureEnabled = false;

				foreach (VertexPositionColor[] primitiveList in this.boxes)
				{
					// Draw the box with a LineList
					foreach (EffectPass pass in boxEffect.CurrentTechnique.Passes)
					{
						pass.Apply();
						GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, primitiveList, 0, 8, bBoxIndices, 0, 12);
					}
				}
			}
		}
	}
}