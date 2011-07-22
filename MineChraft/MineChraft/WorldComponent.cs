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
using Chraft.World;
using Chraft.Net;


namespace MineChraft
{
	public class WorldComponent : DrawableGameComponent
	{
		private Client Parent;
		public ChunkSet Chunks { get; set; }
		private BasicEffect CurrentEffect;
		private List<Block> Blocks = new List<Block>();
		private bool NeedsUpdate = true;

		public WorldComponent(Client game)
			: base(game)
		{
			Parent = game;
			Chunks = new ChunkSet();
		}

		public override void Initialize()
		{
			InitializeEffect();
			base.Initialize();
		}

		public void AddChunk(Chunk chunk)
		{
			int posX = chunk.X;
			int posY = chunk.Y;
			int posZ = chunk.Z;
			int startX = posX ^ (posX | 0xff);
			int startZ = posZ ^ (posZ | 0xff);

			if (!Chunks.ContainsKey(new PointI(posX, posZ)))
				Chunks.Add(new Chunk(startX, startZ));
			Chunk target = Chunks[posX, posZ];

			for (int x = 0; x < chunk.SizeX; x++)
			{
				for (int y = 0; y < chunk.SizeY; y++)
				{
					for (int z = 0; z < chunk.SizeZ; z++)
					{
						target.ChunkBlocks[posX - startX + x, posY + y, posZ - startZ + z] = chunk.ChunkBlocks[x, y, z];
					}
				}
			}

			NeedsUpdate = true;
		}

		public override void Update(GameTime gameTime)
		{
			if (NeedsUpdate)
			{
				List<Block> blocks = new List<Block>();
				foreach (Chunk c in Chunks.Values)
				{
					foreach (var b in c.ChunkBlocks)
					{
						blocks.Add(new Block(b.X, b.Y, b.Z));
					}
				}
				Blocks = blocks;
			}
			base.Update(gameTime);
		}

		private void InitializeEffect()
		{
			CurrentEffect = new BasicEffect(Parent.GraphicsDevice);
			CurrentEffect.TextureEnabled = false;
			CurrentEffect.LightingEnabled = true;
			CurrentEffect.VertexColorEnabled = true;
			CurrentEffect.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
			CurrentEffect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
			CurrentEffect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
			CurrentEffect.SpecularPower = 5.0f;
			CurrentEffect.Alpha = 1.0f;

			CurrentEffect.DirectionalLight0.Enabled = true;
			CurrentEffect.DirectionalLight0.DiffuseColor = new Vector3(1, 0, 0);
			CurrentEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, 0, 0));
			CurrentEffect.DirectionalLight0.SpecularColor = Vector3.One;
			
			CurrentEffect.DirectionalLight1.Enabled = true;
			CurrentEffect.DirectionalLight1.DiffuseColor = new Vector3(0, 0.75f, 0);
			CurrentEffect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(0, -1, 0));
			CurrentEffect.DirectionalLight1.SpecularColor = Vector3.One;
			
			CurrentEffect.DirectionalLight2.Enabled = true;
			CurrentEffect.DirectionalLight2.DiffuseColor = new Vector3(0, 0, 0.5f);
			CurrentEffect.DirectionalLight2.Direction = Vector3.Normalize(new Vector3(0, 0, -1));
			CurrentEffect.DirectionalLight2.SpecularColor = Vector3.One;
		}

		public override void Draw(GameTime gameTime)
		{
			if (CurrentEffect == null)
				return;

			CurrentEffect.Projection = Parent.Camera.ViewProjectionMatrix;
			CurrentEffect.View = Parent.Camera.ViewMatrix;
			CurrentEffect.World = Matrix.Identity;

			foreach (EffectPass p in CurrentEffect.CurrentTechnique.Passes)
			{
				p.Apply();
				foreach (Block b in Blocks)
					b.Draw(Parent.GraphicsDevice);
			}

			base.Draw(gameTime);
		}
	}
}
