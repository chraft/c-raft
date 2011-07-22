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
using Chraft.Net;
using System.Net.Sockets;
using System.Net;
using Chraft.World;

namespace MineChraft
{
	public partial class Client : Game
	{
		private GraphicsDeviceManager Graphics;
		private volatile string[] drawChatlog;
		private Rectangle bounds;
		private SpriteFont Font;
		private SpriteBatch sprites;
		private Vector2[] drawPositions;
		private const int LINES = 20;
		private KeyboardState prevState = new KeyboardState();
		private volatile string inputBuffer = "";
		private Vector2 inputBufferPos = Vector2.Zero;
		private List<string> ChatlogLines = new List<string>();
		private int inputPos = 0;
		private WorldComponent World;
		public CameraComponent Camera { get; set; }

		public Client()
		{
			Graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			base.Initialize();
			InitializeConnection();
			this.Components.Add(World = new WorldComponent(this));
			Camera = new CameraComponent(this);
			//this.Components.Add(Camera = new CameraComponent(this));
		}

		protected override void LoadContent()
		{
			sprites = new SpriteBatch(GraphicsDevice);
			Font = Content.Load<SpriteFont>("Fonts/Chat");

			ChatlogLines = new List<string>(new string[] { "Minearth initialized." });
			UpdateLines();

			base.LoadContent();
		}

		protected override void UnloadContent()
		{
		}

		protected override void Update(GameTime gameTime)
		{
			Rectangle bounds = GraphicsDevice.Viewport.Bounds;
			if (bounds.Height != this.bounds.Height || bounds.Width != this.bounds.Width)
			{
				this.bounds = bounds;
				UpdateLines();
			}

			base.Update(gameTime);

			// Read keystrokes
			KeyboardState curState = Keyboard.GetState();
			bool shift = curState.IsKeyDown(Keys.LeftShift) || curState.IsKeyDown(Keys.RightShift);

			foreach (Keys k in curState.GetPressedKeys())
			{
				string inputBuffer = this.inputBuffer;
				char c = (char)k;
				if (prevState.IsKeyDown(k)) continue;
				if (k >= Keys.A && k <= Keys.Z) inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? c : (char)(c + ' ')));
				else if (k == Keys.D0)					/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? ')' : '0'));
				else if (k == Keys.D1)					/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '!' : '1'));
				else if (k == Keys.D2)					/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '@' : '2'));
				else if (k == Keys.D3)					/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '#' : '3'));
				else if (k == Keys.D4)					/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '$' : '4'));
				else if (k == Keys.D5)					/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '%' : '5'));
				else if (k == Keys.D6)					/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '^' : '6'));
				else if (k == Keys.D7)					/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '&' : '7'));
				else if (k == Keys.D8)					/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '*' : '8'));
				else if (k == Keys.D9)					/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '(' : '9'));
				else if (k == Keys.OemMinus)			/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '_' : '-'));
				else if (k == Keys.OemPlus)				/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '+' : '='));
				else if (k == Keys.OemOpenBrackets)		/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '{' : '['));
				else if (k == Keys.OemCloseBrackets)	/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '}' : ']'));
				else if (k == Keys.OemBackslash)		/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '|' : '\\'));
				else if (k == Keys.OemSemicolon)		/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? ':' : ';'));
				else if (k == Keys.OemQuotes)			/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '"' : '\''));
				else if (k == Keys.OemComma)			/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '<' : ','));
				else if (k == Keys.OemPeriod)			/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '>' : '.'));
				else if (k == Keys.OemQuestion)			/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '?' : '/'));
				else if (k == Keys.OemTilde)			/**/inputBuffer = inputBuffer.Insert(inputPos++, Char.ToString(shift ? '~' : '\''));
				else if (k == Keys.Space) inputBuffer = inputBuffer.Insert(inputPos++, " ");
				else if (k == Keys.Back && inputPos > 0) inputBuffer = inputBuffer.Remove(--inputPos, 1);
				else if (k == Keys.Left && inputPos > 0) inputPos--;
				else if (k == Keys.Right && inputPos < inputBuffer.Length) inputPos++;
				else if (k == Keys.Enter && !string.IsNullOrWhiteSpace(this.inputBuffer))
				{
					this.inputBuffer = "";
					this.inputPos = 0;
					SendMessage(inputBuffer);
					break;
				}
				else continue;
				this.inputBuffer = inputBuffer;
			}

			prevState = curState;
		}

		private void UpdateLines()
		{
			Rectangle bounds = GraphicsDevice.Viewport.Bounds;
			List<string> chatlog = new List<string>();

			foreach (string message in ChatlogLines)
			{
				int x = 0;
				float width = 0;
				for (int i = 0, word = 0; i < message.Length; i++)
				{
					float cwidth = Font.MeasureString(message[i].ToString()).X;
					width += cwidth;
					if (width > bounds.Width)
					{
						chatlog.Add(message.Substring(x, word - x));
						x = word;
						width = cwidth;
						continue;
					}

					if (!char.IsLetterOrDigit(message[i])) word = i;
				}
				if (x < message.Length) chatlog.Add(message.Substring(x));
			}

			string[] drawChatlog = chatlog.Reverse<string>().ToArray();
			Vector2[] drawPositions = this.drawPositions;
			string inputBuffer = this.inputBuffer;

			Vector2 inputBufferPos = new Vector2(1, bounds.Height - Font.MeasureString(" ").Y);

			float y = inputBufferPos.Y - 3;
			drawPositions = new Vector2[drawChatlog.Length];
			for (int i = 0; i < drawPositions.Length; i++)
			{
				y -= Font.MeasureString(" ").Y;
				drawPositions[i] = new Vector2(1, y);
			}

			this.bounds = bounds;
			this.drawPositions = drawPositions;
			this.inputBufferPos = inputBufferPos;
			this.drawChatlog = drawChatlog;
		}

		protected override void Draw(GameTime gameTime)
		{
			string[] drawChatlog = this.drawChatlog;
			Vector2[] drawPositions = this.drawPositions;
			string inputBuffer = this.inputBuffer;
			Vector2 inputBufferPos = this.inputBufferPos;
			int inputPos = this.inputPos;

			Rectangle bounds = GraphicsDevice.Viewport.Bounds;
			inputBufferPos.X = Font.MeasureString(inputBuffer).X - 2;
			inputBufferPos.X = (inputBufferPos.X < bounds.Width ? 2 : bounds.Width - inputBufferPos.X);

			sprites.Begin();
			Color shadow = Color.Black;
			sprites.DrawString(Font, inputBuffer, inputBufferPos - new Vector2(1), shadow);
			sprites.DrawString(Font, inputBuffer, inputBufferPos + new Vector2(1), shadow);
			sprites.DrawString(Font, inputBuffer, inputBufferPos - new Vector2(1, 0), shadow);
			sprites.DrawString(Font, inputBuffer, inputBufferPos + new Vector2(1, 0), shadow);
			sprites.DrawString(Font, inputBuffer, inputBufferPos - new Vector2(0, 1), shadow);
			sprites.DrawString(Font, inputBuffer, inputBufferPos + new Vector2(0, 1), shadow);
			sprites.DrawString(Font, inputBuffer, inputBufferPos, Color.White);

			Vector2 cursorPos = inputBufferPos + new Vector2(Font.MeasureString(inputPos >= inputBuffer.Length ? inputBuffer : inputBuffer.Remove(inputPos)).X, 0);
			sprites.DrawString(Font, "_", cursorPos - new Vector2(1), shadow);
			sprites.DrawString(Font, "_", cursorPos + new Vector2(1), shadow);
			sprites.DrawString(Font, "_", cursorPos - new Vector2(1, 0), shadow);
			sprites.DrawString(Font, "_", cursorPos + new Vector2(1, 0), shadow);
			sprites.DrawString(Font, "_", cursorPos - new Vector2(0, 1), shadow);
			sprites.DrawString(Font, "_", cursorPos + new Vector2(0, 1), shadow);
			sprites.DrawString(Font, "_", cursorPos, Color.LightGray);

			for (int i = 0; i < drawChatlog.Length && i < drawPositions.Length; i++)
			{
				sprites.DrawString(Font, drawChatlog[i], drawPositions[i] - new Vector2(1), shadow);
				sprites.DrawString(Font, drawChatlog[i], drawPositions[i] + new Vector2(1), shadow);
				sprites.DrawString(Font, drawChatlog[i], drawPositions[i] - new Vector2(1, 0), shadow);
				sprites.DrawString(Font, drawChatlog[i], drawPositions[i] + new Vector2(1, 0), shadow);
				sprites.DrawString(Font, drawChatlog[i], drawPositions[i] - new Vector2(0, 1), shadow);
				sprites.DrawString(Font, drawChatlog[i], drawPositions[i] + new Vector2(0, 1), shadow);
				sprites.DrawString(Font, drawChatlog[i], drawPositions[i], Color.White);
			}

			sprites.End();
			base.Draw(gameTime);
		}
	}
}
