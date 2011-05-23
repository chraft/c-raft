using java.util;
using Chraft.Utils;


namespace Chraft.World
{
	public class MapGenCaves : MapGenBase
	{
		private void UnknownA(int i, int j, byte[] abyte0, double d, double d1, double d2)
		{
			UnknownB(i, j, abyte0, d, d1, d2, 1.0F + rand.nextFloat() * 6F, 0.0F, 0.0F, -1, -1, 0.5D);
		}

		private void UnknownB(int i, int j, byte[] abyte0, double d, double d1, double d2, float f, float f1, float f2, int k, int l, double d3)
		{
			double d4 = i * 16 + 8;
			double d5 = j * 16 + 8;
			float f3 = 0.0F;
			float f4 = 0.0F;
			Random random = new Random(rand.nextLong());
			if (l <= 0)
			{
				int i1 = field_947_a * 16 - 16;
				l = i1 - random.nextInt(i1 / 4);
			}
			bool flag = false;
			if (k == -1)
			{
				k = l / 2;
				flag = true;
			}
			int j1 = random.nextInt(l / 2) + l / 4;
			bool flag1 = random.nextInt(6) == 0;
			for (; k < l; k++)
			{
				double d6 = 1.5D + (double)(MathHelper.Sin(((float)k * 3.141592f) / (float)l) * f * 1.0f);
				double d7 = d6 * d3;
				float f5 = MathHelper.cos(f2);
				float f6 = MathHelper.Sin(f2);
				d += MathHelper.cos(f1) * f5;
				d1 += f6;
				d2 += MathHelper.Sin(f1) * f5;
				if (flag1)
				{
					f2 *= 0.92F;
				}
				else
				{
					f2 *= 0.7F;
				}
				f2 += f4 * 0.1F;
				f1 += f3 * 0.1F;
				f4 *= 0.9F;
				f3 *= 0.75F;
				f4 += (random.nextFloat() - random.nextFloat()) * random.nextFloat() * 2.0F;
				f3 += (random.nextFloat() - random.nextFloat()) * random.nextFloat() * 4F;
				if (!flag && k == j1 && f > 1.0F)
				{
					UnknownB(i, j, abyte0, d, d1, d2, random.nextFloat() * 0.5F + 0.5F, f1 - 1.570796F, f2 / 3F, k,
									  l, 1.0D);
					UnknownB(i, j, abyte0, d, d1, d2, random.nextFloat() * 0.5F + 0.5F, f1 + 1.570796F, f2 / 3F, k,
									  l, 1.0D);
					return;
				}
				if (!flag && random.nextInt(4) == 0)
				{
					continue;
				}
				double d8 = d - d4;
				double d9 = d2 - d5;
				double d10 = l - k;
				double d11 = f + 2.0F + 16F;
				if ((d8 * d8 + d9 * d9) - d10 * d10 > d11 * d11)
				{
					return;
				}
				if (d < d4 - 16D - d6 * 2D || d2 < d5 - 16D - d6 * 2D || d > d4 + 16D + d6 * 2D || d2 > d5 + 16D + d6 * 2D)
				{
					continue;
				}
				d8 = MathHelper.floor_double(d - d6) - i * 16 - 1;
				int k1 = (MathHelper.floor_double(d + d6) - i * 16) + 1;
				d9 = MathHelper.floor_double(d1 - d7) - 1;
				int l1 = MathHelper.floor_double(d1 + d7) + 1;
				d10 = MathHelper.floor_double(d2 - d6) - j * 16 - 1;
				int i2 = (MathHelper.floor_double(d2 + d6) - j * 16) + 1;

				if (d8 < 0)
					d8 = 0;
				if (k1 > 16)
					k1 = 16;
				if (d9 < 1)
					d9 = 1;
				if (l1 > 120)
					l1 = 120;
				if (d10 < 0)
					d10 = 0;
				if (i2 > 16)
					i2 = 16;

				for (int j2 = (int)d8; j2 < k1; j2++)
				{
					for (int l2 = (int)d10; l2 < i2; l2++)
					{
						for (int i3 = l1 + 1; i3 >= d9 - 1; i3--)
						{
							int j3 = (j2 * 16 + l2) * 128 + i3;
							if (i3 < 0 || i3 >= 128)
								continue;
							if (abyte0[j3] == (byte)BlockData.Blocks.Still_Water || abyte0[j3] == (byte)BlockData.Blocks.Water)
								goto cont;
							if (i3 != d9 - 1 && j2 != d8 && j2 != k1 - 1 && l2 != d10 && l2 != i2 - 1)
								i3 = (int)d9;
						}
					}
				}

				for (int k2 = (int)d8; k2 < k1; k2++)
				{
					double d12 = (((double)(k2 + i * 16) + 0.5D) - d) / d6;

					for (int k3 = (int)d10; k3 < i2; k3++)
					{
						double d13 = (((double)(k3 + j * 16) + 0.5D) - d2) / d6;
						int l3 = (k2 * 16 + k3) * 128 + l1;
						bool flag3 = false;

						if (d12 * d12 + d13 * d13 >= 1.0D)
							continue;

						int i4 = l1 - 1;
						while (i4 >= d9)
						{
							double d14 = (((double)i4 + 0.5D) - d1) / d7;
							if (d14 > -0.7 && d12 * d12 + d14 * d14 + d13 * d13 < 1.0)
							{
								byte byte0 = abyte0[l3];
								if (byte0 == (byte)BlockData.Blocks.Grass)
								{
									flag3 = true;
								}
								if (byte0 == (byte)BlockData.Blocks.Stone || byte0 == (byte)BlockData.Blocks.Dirt ||
									byte0 == (byte)BlockData.Blocks.Grass)
								{
									if (i4 < 10)
									{
										abyte0[l3] = (byte)BlockData.Blocks.Still_Lava;
									}
									else
									{
										abyte0[l3] = 0;
										if (flag3 && abyte0[l3 - 1] == (byte)BlockData.Blocks.Dirt)
										{
											abyte0[l3 - 1] = (byte)BlockData.Blocks.Grass;
										}
									}
								}
							}
							l3--;
							i4--;
						}
					}
				}

				if (flag)
				{
					break;
				}

			cont:
				continue;
			}
		}

		public override void GenerateB(WorldManager world, int i, int j, int k, int l, byte[] abyte0)
		{
			int i1 = rand.nextInt(rand.nextInt(rand.nextInt(40) + 1) + 1);
			if (rand.nextInt(15) != 0)
			{
				i1 = 0;
			}
			for (int j1 = 0; j1 < i1; j1++)
			{
				double d = i * 16 + rand.nextInt(16);
				double d1 = rand.nextInt(rand.nextInt(120) + 8);
				double d2 = j * 16 + rand.nextInt(16);
				int k1 = 1;
				if (rand.nextInt(4) == 0)
				{
					UnknownA(k, l, abyte0, d, d1, d2);
					k1 += rand.nextInt(4);
				}
				for (int l1 = 0; l1 < k1; l1++)
				{
					float f = rand.nextFloat() * 3.141593F * 2.0F;
					float f1 = ((rand.nextFloat() - 0.5F) * 2.0F) / 8F;
					float f2 = rand.nextFloat() * 2.0F + rand.nextFloat();
					UnknownB(k, l, abyte0, d, d1, d2, f2, f, f1, 0, 0, 1.0D);
				}
			}
		}
	}
}