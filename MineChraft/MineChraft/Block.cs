using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MineChraft
{
	public class Block
	{
		private VertexPositionNormalTexture[] Vertices = new VertexPositionNormalTexture[36];

		public Block(float x, float y, float z)
		{
			Vector3 topLeftFront = new Vector3(-1.0f + x, 1.0f + y, 1.0f + z);
			Vector3 bottomLeftFront = new Vector3(-1.0f + x, -1.0f + y, 1.0f + z);
			Vector3 topRightFront = new Vector3(1.0f + x, 1.0f + y, 1.0f + z);
			Vector3 bottomRightFront = new Vector3(1.0f + x, -1.0f + y, 1.0f + z);
			Vector3 topLeftBack = new Vector3(-1.0f + x, 1.0f + y, -1.0f + z);
			Vector3 topRightBack = new Vector3(1.0f + x, 1.0f + y, -1.0f + z);
			Vector3 bottomLeftBack = new Vector3(-1.0f + x, -1.0f + y, -1.0f + z);
			Vector3 bottomRightBack = new Vector3(1.0f + x, -1.0f + y, -1.0f + z);

			Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
			Vector2 textureTopRight = new Vector2(1.0f, 0.0f);
			Vector2 textureBottomLeft = new Vector2(0.0f, 1.0f);
			Vector2 textureBottomRight = new Vector2(1.0f, 1.0f);

			Vector3 frontNormal = new Vector3(0.0f, 0.0f, 1.0f);
			Vector3 backNormal = new Vector3(0.0f, 0.0f, -1.0f);
			Vector3 topNormal = new Vector3(0.0f, 1.0f, 0.0f);
			Vector3 bottomNormal = new Vector3(0.0f, -1.0f, 0.0f);
			Vector3 leftNormal = new Vector3(-1.0f, 0.0f, 0.0f);
			Vector3 rightNormal = new Vector3(1.0f, 0.0f, 0.0f);

			// Front face
			Vertices[0] = new VertexPositionNormalTexture(topLeftFront, frontNormal, textureTopLeft);
			Vertices[1] = new VertexPositionNormalTexture(bottomLeftFront, frontNormal, textureBottomLeft);
			Vertices[2] = new VertexPositionNormalTexture(topRightFront, frontNormal, textureTopRight);
			Vertices[3] = new VertexPositionNormalTexture(bottomLeftFront, frontNormal, textureBottomLeft);
			Vertices[4] = new VertexPositionNormalTexture(bottomRightFront, frontNormal, textureBottomRight);
			Vertices[5] = new VertexPositionNormalTexture(topRightFront, frontNormal, textureTopRight);

			// Back face
			Vertices[6] = new VertexPositionNormalTexture(topLeftBack, backNormal, textureTopRight);
			Vertices[7] = new VertexPositionNormalTexture(topRightBack, backNormal, textureTopLeft);
			Vertices[8] = new VertexPositionNormalTexture(bottomLeftBack, backNormal, textureBottomRight);
			Vertices[9] = new VertexPositionNormalTexture(bottomLeftBack, backNormal, textureBottomRight);
			Vertices[10] = new VertexPositionNormalTexture(topRightBack, backNormal, textureTopLeft);
			Vertices[11] = new VertexPositionNormalTexture(bottomRightBack, backNormal, textureBottomLeft);

			// Top face
			Vertices[12] = new VertexPositionNormalTexture(topLeftFront, topNormal, textureBottomLeft);
			Vertices[13] = new VertexPositionNormalTexture(topRightBack, topNormal, textureTopRight);
			Vertices[14] = new VertexPositionNormalTexture(topLeftBack, topNormal, textureTopLeft);
			Vertices[15] = new VertexPositionNormalTexture(topLeftFront, topNormal, textureBottomLeft);
			Vertices[16] = new VertexPositionNormalTexture(topRightFront, topNormal, textureBottomRight);
			Vertices[17] = new VertexPositionNormalTexture(topRightBack, topNormal, textureTopRight);

			// Bottom face
			Vertices[18] = new VertexPositionNormalTexture(bottomLeftFront, bottomNormal, textureTopLeft);
			Vertices[19] = new VertexPositionNormalTexture(bottomLeftBack, bottomNormal, textureBottomLeft);
			Vertices[20] = new VertexPositionNormalTexture(bottomRightBack, bottomNormal, textureBottomRight);
			Vertices[21] = new VertexPositionNormalTexture(bottomLeftFront, bottomNormal, textureTopLeft);
			Vertices[22] = new VertexPositionNormalTexture(bottomRightBack, bottomNormal, textureBottomRight);
			Vertices[23] = new VertexPositionNormalTexture(bottomRightFront, bottomNormal, textureTopRight);

			// Left face
			Vertices[24] = new VertexPositionNormalTexture(topLeftFront, leftNormal, textureTopRight);
			Vertices[25] = new VertexPositionNormalTexture(bottomLeftBack, leftNormal, textureBottomLeft);
			Vertices[26] = new VertexPositionNormalTexture(bottomLeftFront, leftNormal, textureBottomRight);
			Vertices[27] = new VertexPositionNormalTexture(topLeftBack, leftNormal, textureTopLeft);
			Vertices[28] = new VertexPositionNormalTexture(bottomLeftBack, leftNormal, textureBottomLeft);
			Vertices[29] = new VertexPositionNormalTexture(topLeftFront, leftNormal, textureTopRight);

			// Right face
			Vertices[30] = new VertexPositionNormalTexture(topRightFront, rightNormal, textureTopLeft);
			Vertices[31] = new VertexPositionNormalTexture(bottomRightFront, rightNormal, textureBottomLeft);
			Vertices[32] = new VertexPositionNormalTexture(bottomRightBack, rightNormal, textureBottomRight);
			Vertices[33] = new VertexPositionNormalTexture(topRightBack, rightNormal, textureTopRight);
			Vertices[34] = new VertexPositionNormalTexture(topRightFront, rightNormal, textureTopLeft);
			Vertices[35] = new VertexPositionNormalTexture(bottomRightBack, rightNormal, textureBottomRight);
		}

		public void Draw(GraphicsDevice device)
		{
			device.DrawUserPrimitives(PrimitiveType.TriangleStrip, Vertices, 0, Vertices.Length);
		}
	}
}
