using System;
using System.IO;
using System.Collections.Generic;

namespace GeometricBattle;

public static class Tools
{
	public static void SaveGame()
	{
		/* Preparation */

		byte playerControls;
		if (Settings.SelectedControl == Settings.WASD)
			 playerControls = 0;
		else playerControls = 1;

		/* Write data on save file */

		FileStream fs = new FileStream(Game1.savePath, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fs);
		{
			bw.Write(Game1.HiScore);
			bw.Write(playerControls);
		}
		bw.Close();
		fs.Close();

		#if DEBUG
		Console.WriteLine("INFO: Game saved.");
		#endif
	}

	public static bool LoadGame()
	{
		if (File.Exists(Game1.savePath))
		{
			FileStream fs = new FileStream(Game1.savePath, FileMode.Open);
        	BinaryReader br = new BinaryReader(fs);
			{
				Game1.HiScore = br.ReadInt32();

				if (br.ReadByte() == 0)
					 Settings.SelectedControl = Settings.WASD;
				else Settings.SelectedControl = Settings.ZQSD;
			}
			br.Close();
			fs.Close();

			#if DEBUG
			Console.WriteLine("INFO: Save loaded.");
			#endif

			return true;
		}
		return false;
	}

	public static int[,] DeleteRow(int rowDeleteIndex, int[,] sourceArray) // To remove a row from a two-dimensional array
	{
		int rows = sourceArray.GetLength(0);
		int cols = sourceArray.GetLength(1);
		int[,] result = new int[rows - 1, cols];
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				if (i != rowDeleteIndex)
				{
					result[i >= rowDeleteIndex ? i - 1 : i, j] = sourceArray[i, j];
				}
			}
		}
		return result;
	}

    public static void RemoveUnorderedAt<T>(this List<T> list, int index)
    {
        list[index] = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
    }

/* // NOTE: Not used in this version.

    public static int IntRestrict(int value, int vMin, int vMax)
    {
        if (value < vMin) return vMin;
        if (value > vMax) return vMax;

        return value;
    }

	public static bool IsIntersectPixels(Rectangle rectangleA, Texture2D textureA, Rectangle rectangleB, Texture2D textureB)
	{
		// Creation of arrays of the right size and get color data
		Color[] dataA = new Color[textureA.Width * textureA.Height]; textureA.GetData(dataA);
		Color[] dataB = new Color[textureB.Width * textureB.Height]; textureB.GetData(dataB);

		// Get bounds
		int top = System.Math.Max(rectangleA.Top, rectangleB.Top);
		int bottom = System.Math.Min(rectangleA.Bottom, rectangleB.Bottom);
		int left = System.Math.Max(rectangleA.Left, rectangleB.Left);
		int right = System.Math.Min(rectangleA.Right, rectangleB.Right);

		for (int y = top; y < bottom; y++)
		{
			for (int x = left; x < right; x++)
			{
				// Get color from current point
				Color colorA = dataA[(x - rectangleA.Left) + (y - rectangleA.Top) * rectangleA.Width];
				Color colorB = dataB[(x - rectangleB.Left) + (y - rectangleB.Top) * rectangleB.Width];

				// If both not transparent, has intersection
				if (colorA.A != 0 && colorB.A != 0)
				{
					return true;
				}
			}
		}

		return false;
	}

	public static int[] GetIntersectPixels(Rectangle rectangleA, Texture2D textureA, Rectangle rectangleB, Texture2D textureB)
	{
		// Creation of arrays of the right size and get color data
		Color[] dataA = new Color[textureA.Width * textureA.Height]; textureA.GetData(dataA);
		Color[] dataB = new Color[textureB.Width * textureB.Height]; textureB.GetData(dataB);

		// Get bounds
		int top = System.Math.Max(rectangleA.Top, rectangleB.Top);
		int bottom = System.Math.Min(rectangleA.Bottom, rectangleB.Bottom);
		int left = System.Math.Max(rectangleA.Left, rectangleB.Left);
		int right = System.Math.Min(rectangleA.Right, rectangleB.Right);

		for (int y = top; y < bottom; y++)
		{
			for (int x = left; x < right; x++)
			{
				// Get color from current point
				Color colorA = dataA[(x - rectangleA.Left) + (y - rectangleA.Top) * rectangleA.Width];
				Color colorB = dataB[(x - rectangleB.Left) + (y - rectangleB.Top) * rectangleB.Width];

				// If both not transparent, has intersection
				if (colorA.A != 0 && colorB.A != 0)
				{
					return new int[2] {x,y};
				}
			}
		}

		return null;
	}

	// Old version of 'Button' class //

	public abstract class ButtonActionOnRef<T>
	{
		public Texture2D Texture { get; private set; }
		public Vector2 Position { get; private set; }
		private Vector2 Origin;

		private SpriteFont Font;
		public string Text;
		private Vector2 TextOrigin;

		private Color[] Colour;
		private byte ActiveColour;
		private bool ActiveClick;
		private bool BlockClick;

		public abstract void Action(ref T param); // The reference is used in this case to pass the value of 'gameState', see inheriting classes.

		public ButtonActionOnRef(Texture2D texture, int posX, int posY, SpriteFont font, string text)
		{
			this.Texture = texture;

			this.Position = new Vector2(posX, posY);

			this.Origin = new Vector2(
				this.Texture.Bounds.Center.X,
				this.Texture.Bounds.Center.Y
			);

			this.Font = font; this.Text = text;

			Vector2 textSize = this.Font.MeasureString(this.Text);
			this.TextOrigin = textSize * .5f;

			this.Colour = new Color[3] {
				Color.FromNonPremultiplied(225, 225, 225, 255),
				Color.FromNonPremultiplied(255, 255, 255, 255),
				Color.FromNonPremultiplied(155, 155, 155, 255)
			};

			this.ActiveColour = 0;
			this.ActiveClick = false;
			this.BlockClick = false;
		}

		public void Update(MouseState mState, ref T param)
		{
			bool inBox = (mState.X > this.Position.X - this.Texture.Bounds.Center.X
					&& mState.X < this.Position.X + this.Texture.Bounds.Center.X
					&& mState.Y > this.Position.Y - this.Texture.Bounds.Center.Y
					&& mState.Y < this.Position.Y + this.Texture.Bounds.Center.Y);

			if (!inBox && !this.BlockClick && mState.LeftButton == ButtonState.Pressed)
				this.BlockClick = true;

			else if (this.BlockClick && mState.LeftButton == ButtonState.Released)
				this.BlockClick = false;

			else if (!this.BlockClick)
			{
				if (inBox && mState.LeftButton == ButtonState.Pressed)
				{
					if (this.ActiveColour != 2) this.ActiveColour = 2;
					if (!this.ActiveClick) this.ActiveClick = true;
				}
				else if (inBox && this.ActiveClick)
				{
					this.ActiveClick = false;
					this.Action(ref param);
				}
				else if (this.ActiveClick)
				{
					this.ActiveClick = false;
					this.ActiveColour = 0;
				}
				else if (!inBox && this.ActiveColour != 0) this.ActiveColour = 0;
				else if (inBox && this.ActiveColour != 1)  this.ActiveColour = 1;     
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(
				this.Texture,
				this.Position,
				null,
				this.Colour[this.ActiveColour],
				0f,
				this.Origin,
				1f,
				SpriteEffects.None,
				0f
			);

			spriteBatch.DrawString(
				this.Font,
				this.Text,
				this.Position,
				Color.Black,
				0f,
				this.TextOrigin,
				1,
				SpriteEffects.None,
				0f
			);
		}
	}

*/
}
