using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.TextureAtlases;

namespace GeometricBattle;

#region GLOBAL_USAGE

public class Cursor
{
    private Texture2D Texture;

    private Vector2 Position;
    private Vector2 Origin;

    private float Rotation;
    private float Scale;
    private Color Colour;

    public Cursor(Texture2D texture)
    {
        this.Texture = texture;

        MouseState mState = Mouse.GetState();
        this.Position = new Vector2(mState.X, mState.Y);

        this.Origin = new Vector2(
            this.Texture.Bounds.Center.X,
            this.Texture.Bounds.Center.Y
        );

        Rotation = 0f;
        Scale    = 1f;
        Colour   = Color.White;
    }

    public void Update(MouseState mState)
    {
        this.Position = new Vector2(mState.X, mState.Y);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            this.Texture,
            this.Position,
            null,
            this.Colour,
            this.Rotation,
            this.Origin,
            this.Scale,
            SpriteEffects.None,
            0f
        );
    }
}

public abstract class Button
{
    public Texture2D Texture { get; private set; }
    public Vector2 Position { get; private set; }
    private Vector2 Origin;

    private SoundEffect Sound;
    private SpriteFont Font;
    public string Text;
    private Vector2 TextOrigin;

    private Color[] Colour;
    private byte ActiveColour;
    private bool ActiveClick;
    private bool BlockClick;

    public abstract void Action();

    public Button(Texture2D texture, int posX, int posY, SoundEffect sound, SpriteFont font, string text)
    {
        this.Texture = texture;

        this.Position = new Vector2(posX, posY);

        this.Origin = new Vector2(
            this.Texture.Bounds.Center.X,
            this.Texture.Bounds.Center.Y
        );

        this.Sound = sound;
        this.Font = font;
        this.Text = text;

        this.TextOrigin = this.Font.MeasureString(this.Text)*.5f;

        this.Colour = new Color[3] {
            Color.FromNonPremultiplied(225, 225, 225, 255),
            Color.FromNonPremultiplied(255, 255, 255, 255),
            Color.FromNonPremultiplied(155, 155, 155, 255)
        };

        this.ActiveColour = 0;
        this.ActiveClick = false;
        this.BlockClick = false;
    }

    public void Update(MouseState mState)
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
                this.Action();

                if (Settings.IsSoundOn)
                    this.Sound.Play();
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
            1f,
            SpriteEffects.None,
            0f
        );
    }
}

public class ButtonTitle : Button
{
    public ButtonTitle(Texture2D texture, int posX, int posY, SoundEffect sound, SpriteFont font, string text)
        : base(texture, posX, posY, sound, font, text) { }

    public override void Action()
    {
        Tools.SaveGame();

        if (GameObjects.Reset)
        {
            GameObjects.Reset = false;
            GameObjects.Init();
        }

        Game1.gameState = GameState.G_TITLE;
    }
}

public class ButtonSettings : Button
{
    public ButtonSettings(Texture2D texture, int posX, int posY, SoundEffect sound, SpriteFont font, string text)
        : base(texture, posX, posY, sound, font, text) { }

    public override void Action()
    {
        Game1.gameState = GameState.G_SETTINGS;
    }
}

public class FrameCounter
{
    private Vector2 Position;
    private int Frames, DisplayFrequency; // DisplayFrequency could be put in float type if needed
    private double ElapsedTime, Last, Now;
    private bool Precision;
    private SpriteFont Font;
    private string MessageText;
    private Color MessageColor;

    public FrameCounter(int posX, int posY, int frequency, bool precision, SpriteFont font, Color colour)
    {
        this.Position = new Vector2(posX, posY);

        this.Frames = 0;
        this.DisplayFrequency = frequency;

        this.ElapsedTime = .0;
        this.Last = .0;
        this.Now = .0;

        this.Precision = precision;

        this.Font = font;
        this.MessageText = "";
        this.MessageColor = colour;
    }

    public void Update(GameTime gameTime)
    {
        Now = gameTime.TotalGameTime.TotalSeconds;

        ElapsedTime = Now - Last;

        if (ElapsedTime > DisplayFrequency)
        {
            if (!Precision)
                 MessageText = " FPS: " + Math.Round(Frames/ElapsedTime).ToString();
            else MessageText = " FPS: " + (Frames/ElapsedTime).ToString();

            Frames = 0; ElapsedTime = 0; Last = Now;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(Font, MessageText, Position, MessageColor);
        ++Frames;
    }
}

#endregion

#region G_TITLE

public class Logo
{
    private Texture2D Texture;
    private Vector2 Position;
    private Vector2 Origin;
    private Color Colour;
    private Random Rand;
    private float Timer;
    private bool ActiveClick;
    private bool BlockClick;
    private bool Active;

    public Logo(Texture2D texture, int posX, int posY)
    {
        this.Texture = texture;

        this.Position = new Vector2(posX, posY);
        this.Origin = new Vector2(
            this.Texture.Bounds.Center.X,
            this.Texture.Bounds.Center.Y
        );

        this.Colour = Color.White;
        this.Rand = new Random();
        this.Timer = 0f;

        this.Active = false;
    }

    public void Update(MouseState mState, GameTime gameTime)
    {
        if (this.Active)
        {
            if (this.Timer < .25f)
                this.Timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            else
            {
                this.Colour = Color.FromNonPremultiplied(
                    this.Rand.Next(0, 256), this.Rand.Next(0, 256),
                    this.Rand.Next(0, 256), 255
                ); this.Timer = 0f;
            }
        }

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
                if (!this.ActiveClick) this.ActiveClick = true;
            }
            else if (inBox && this.ActiveClick)
            {
                this.Active = !this.Active;
                this.ActiveClick = false;
            }
            else if (this.ActiveClick)
            {
                this.ActiveClick = false;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            this.Texture, 
            this.Position,
            null, 
            this.Colour, 
            0f, 
            this.Origin, 
            1f,
            SpriteEffects.None, 
            0f
        );
    }
}

public class ButtonPlay : Button
{
    public ButtonPlay(Texture2D texture, int posX, int posY, SoundEffect sound, SpriteFont font, string text)
        : base(texture, posX, posY, sound, font, text) { }

    public override void Action()
    {
        Game1.gameState = GameState.G_PLAY;
    }
}

public class ShowHiScore
{
    private SpriteFont Font;
    private Vector2 Position;
    private Vector2 Origin;

    private int PrevScore;
    private string Text;

    public ShowHiScore(SpriteFont font, int posX, int posY)
    {
        this.Font = font;
        this.Position = new Vector2(posX, posY);
        this.PrevScore = Game1.HiScore;
        this.Text = "HI-SCORE: "+this.PrevScore.ToString();
        this.Origin = this.Font.MeasureString(this.Text)*.5f;
    }

    public void Update()
    {
        if (Game1.HiScore > this.PrevScore)
        {
            this.PrevScore = Game1.HiScore;
            this.Text = "HI-SCORE: " + this.PrevScore.ToString() + " ";
            this.Origin = this.Font.MeasureString(this.Text)*.5f;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (Game1.HiScore > 0)
            spriteBatch.DrawString(
                this.Font,
                this.Text,
                this.Position,
                Color.White,
                0f,
                this.Origin,
                1f,
                SpriteEffects.None,
                0f
            );
    }

}

#endregion

#region G_PLAY

public static class GameObjects
{
    public static bool Reset = false;

    public static Texture2D playerTexture;
    public static Texture2D bulletsTexture;
    public static Texture2D[] enemysTextures;

    public static SoundEffect shootSound;
    public static SoundEffect impactSound;
    public static SoundEffect destructionSound;
    public static SoundEffect gameOverSound;

    public static void Init()
    {
        Game1.player  = new Player(playerTexture, Settings.SelectedControl, 200f, 0f, 1f);
        Game1.bullets = new Bullets(bulletsTexture);
        Game1.enemys  = new Enemys(enemysTextures, 10);
        
        #if DEBUG
		Console.WriteLine("INFO: Game objects initalized.");
		#endif
    }

    public class Player
    {
        private Texture2D Texture;
        public Vector2 Position { get; private set; }
        public Vector2 Origin;

        private float Speed;
        private float Rotation;
        private float Scale;
        private byte[] ColorBytes;
        private Color Colour;
        private byte PrevLife;
        public byte Life;
        public int Score;

        private float[] VelMove;
        private Keys[] Controls;

        public Player(Texture2D texture, Keys[] controls, float speed, float rotation, float scale)
        {
            this.Texture = texture;

            this.Position = new Vector2(
                Game1.screenSize[0] / 2,
                Game1.screenSize[1] / 2
            );

            this.Origin = new Vector2(
                this.Texture.Bounds.Center.X,
                this.Texture.Bounds.Center.Y
            );

            this.ColorBytes = new byte[4] {255,255,255,255};
            this.Colour = Color.FromNonPremultiplied(
                this.ColorBytes[0], this.ColorBytes[1],
                this.ColorBytes[2], this.ColorBytes[3]
            );

            this.Speed    = speed;
            this.Rotation = rotation;
            this.Scale    = scale;
            this.PrevLife = 3;
            this.Life     = 3;
            this.Score    = 0;

            this.VelMove  = new float[2] { 0f, 0f }; // 0:X - 1:Y
            this.Controls = controls;
        }

        public void Update(KeyboardState kState, MouseState mState, GameTime gameTime)
        {
            float[] pos = new float[2] {this.Position.X, this.Position.Y};

            /* Set direction of player to the cursor of mouse */

            float deltaX = this.Position.X - mState.X;
            float deltaY = this.Position.Y - mState.Y;

            this.Rotation = -MathF.Atan2(deltaX, deltaY);

            /* Calculation of the requested movement */

            if (mState.RightButton == ButtonState.Pressed) // Move with mouse
            {
                if (!(mState.X < this.Position.X + this.Texture.Bounds.Center.X
                && mState.X > this.Position.X - this.Texture.Bounds.Center.X
                && mState.Y < this.Position.Y + this.Texture.Bounds.Center.Y
                && mState.Y > this.Position.Y - this.Texture.Bounds.Center.Y))
                {
                    this.VelMove[0] = MathF.Sin(this.Rotation) * this.Speed;
                    this.VelMove[1] = -MathF.Cos(this.Rotation) * this.Speed;
                }
                else if (this.VelMove[0] != 0 || this.VelMove[1] != 0)
                {
                    this.VelMove[0] = 0; this.VelMove[1] = 0;
                }
            }
            else // Move with keyboard
            {
                float vx = 0f, vy = 0f;

                if (kState.IsKeyDown(Keys.Up)    || kState.IsKeyDown(Controls[0])) vy -= 1f;
                if (kState.IsKeyDown(Keys.Down)  || kState.IsKeyDown(Controls[1])) vy += 1f;
                if (kState.IsKeyDown(Keys.Left)  || kState.IsKeyDown(Controls[2])) vx -= 1f;
                if (kState.IsKeyDown(Keys.Right) || kState.IsKeyDown(Controls[3])) vx += 1f;

                float length = MathF.Sqrt(vx * vx + vy * vy);

                if (length > 0f)
                {
                    if (vx != 0) this.VelMove[0] = (vx / length) * this.Speed;
                    if (vy != 0) this.VelMove[1] = (vy / length) * this.Speed;
                }
            }

            /* Calculation of final movement, collision check and velocity calculation */

            for (int i = 0; i < 2; ++i)
            {
                if (this.VelMove[i] != 0)
                {
                    pos[i] += this.VelMove[i] * (float)gameTime.ElapsedGameTime.TotalSeconds; // Apply movement

                    switch (i) // Check collision with walls
                    {
                        case 0:
                            if (pos[0] - (this.Texture.Width/2) < 0) pos[0] = this.Texture.Width/2;
                            else if (pos[0] + (this.Texture.Width/2) > Game1.screenSize[0])
                                pos[0] = Game1.screenSize[0] - this.Texture.Width/2;
                            break;

                        case 1:
                            if (pos[1] - (this.Texture.Height/2) < 0) pos[1] = this.Texture.Height/2;
                            else if (pos[1] + (this.Texture.Height/2) > Game1.screenSize[1])
                                pos[1] = Game1.screenSize[1] - this.Texture.Height/2;
                            break;
                    }

                    if (this.VelMove[i] > 0) // Reduce velocity of movement
                    {
                        this.VelMove[i] -= this.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (this.VelMove[i] < 0) this.VelMove[i] = 0f;
                    }
                    else
                    {
                        this.VelMove[i] += this.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (this.VelMove[i] > 0) this.VelMove[i] = 0f;
                    }
                }
            }

            /* Apply movement if necessary */

            if (this.VelMove[0] != 0 || this.VelMove[1] != 0)
                this.Position = new Vector2(pos[0], pos[1]);

            /* Events related to the player's life */

            if (Life < PrevLife)
            {
                PrevLife = Life;

                for (int i = 0; i < 3; ++i)
                    ColorBytes[i] /= 2;

                Colour = Color.FromNonPremultiplied(
                    ColorBytes[0], ColorBytes[1],
                    ColorBytes[2], ColorBytes[3]
                );

                if (Life < 1)
                {
                    Reset = true;
                    if (Settings.IsSoundOn)
                        gameOverSound.Play();
                    if (this.Score > Game1.HiScore)
                        Game1.HiScore = this.Score;
                    Game1.gameState = GameState.G_OVER;
                }

            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                this.Texture,
                this.Position,
                null,
                this.Colour,
                this.Rotation,
                this.Origin,
                this.Scale,
                SpriteEffects.None,
                0f
            );
        }
    }

    public class Bullets
    {
        private Texture2D Texture;
        public List<Vector2> Position { get; private set; }
        private Vector2 Origin;
        private float Speed;
        private List<float> Rotation;
        private Color Colour;

        private const int MaxBullets = 10;
        public int NumActiveBullets { get; private set; }
        public int IndexBulletToDestroy;

        public Bullets(Texture2D texture)
        {
            this.Texture = texture;
            this.Position = new List<Vector2>();

            this.Origin = new Vector2(
                this.Texture.Bounds.Center.X,
                this.Texture.Bounds.Center.Y
            );

            this.Speed = 400f;

            this.Rotation = new List<float>();
            this.Colour = Color.Yellow;

            this.NumActiveBullets = 0;
            this.IndexBulletToDestroy = -1;
        }

        private void Destroy(int index_of_bullet)
        {
            Tools.RemoveUnorderedAt<Vector2>(this.Position, index_of_bullet);
            Tools.RemoveUnorderedAt<float>(this.Rotation, index_of_bullet);
            --this.NumActiveBullets;
        }

        public void Update(MouseState mState, GameTime gameTime)
        {
            /* Checking if a bullet must be destroyed following a collision */

            if (this.IndexBulletToDestroy != -1)
            {
                if (Settings.IsSoundOn) impactSound.Play();
                this.Destroy(this.IndexBulletToDestroy);
                this.IndexBulletToDestroy = -1;
            }

            /* Create new bullet when player "shoot" */

            if (this.NumActiveBullets < MaxBullets && mState.LeftButton == ButtonState.Pressed)
            {
                ++this.NumActiveBullets;

                this.Position.Add(new Vector2(Game1.player.Position.X, Game1.player.Position.Y));

                float deltaX = Game1.player.Position.X - mState.X;
                float deltaY = Game1.player.Position.Y - mState.Y;
                this.Rotation.Add(-MathF.Atan2(deltaX, deltaY));

                if (Settings.IsSoundOn) shootSound.Play();
            }

            /* Event handling of active bullets */

            for (int i = 0; i < this.NumActiveBullets; ++i)
            {
                /* Bullet movement */

                this.Position[i] = new Vector2(
                    this.Position[i].X + MathF.Sin(this.Rotation[i]) * this.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds,
                    this.Position[i].Y + -MathF.Cos(this.Rotation[i]) * this.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds
                );

                /* Wall collision */

                if (this.Position[i].X > Game1.screenSize[0]+this.Texture.Bounds.Center.X
                 || this.Position[i].X < -this.Texture.Bounds.Center.X
                 || this.Position[i].Y > Game1.screenSize[1]+this.Texture.Bounds.Center.Y
                 || this.Position[i].Y < -this.Texture.Bounds.Center.Y)
                {
                    this.Destroy(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < this.NumActiveBullets; ++i)
                spriteBatch.Draw(
                    this.Texture, 
                    this.Position[i],
                    null, 
                    this.Colour, 
                    this.Rotation[i], 
                    this.Origin, 
                    1f,
                    SpriteEffects.None, 
                    0f
                );
        }
    }

    public class Enemys
    {
        private Random Rand;

        private Texture2D[] Textures;

        private int NumOfEnemys, PrevNumOfEnemys;

        private List<int> Lifes;
        private List<int> TextureOfEnemys;
        private List<bool> RotateEnemy;
        private List<bool> DestructEnemy;

        public Vector2 Origin;
        public Vector2[] Position;

        public float[] Speed;
        public int[,] Direction;
        public float[] Rotation;
        public float[] Scale;
        public List<Color> Colour;

        private class EnemyDestructionEffect
        {
            private ParticleEffect _particleEffect;
            private Texture2D _particleTexture;

            public EnemyDestructionEffect(GraphicsDevice graphicsDevice, int posX, int posY, int radius, Color color)
            {
                this._particleTexture = new Texture2D(graphicsDevice, 1, 1);
                this._particleTexture.SetData(new[] { Color.White });

                TextureRegion2D textureRegion = new TextureRegion2D(this._particleTexture);
                this._particleEffect = new ParticleEffect(autoTrigger: false)
                {
                    Position = new Vector2(posX, posY),
                    Emitters = new List<ParticleEmitter>
                    {
                        new ParticleEmitter(textureRegion, 500, System.TimeSpan.FromSeconds(1),
                            Profile.Ring(radius, Profile.CircleRadiation.In))
                        {
                            Parameters = new ParticleReleaseParameters
                            {
                                Speed = new Range<float>(0f, 50f),
                                Quantity = 3,
                                Rotation = new Range<float>(-1f, 1f),
                                Scale = new Range<float>(1f, 3f)
                            },
                            Modifiers =
                            {
                                new AgeModifier()
                                {
                                    Interpolators = new List<Interpolator>()
                                    {
                                        new RotationInterpolator {
                                            StartValue = 0f,
                                            EndValue = 5f
                                        },
                                        new ColorInterpolator {
                                            StartValue = color.ToHsl(),
                                            EndValue = Color.White.ToHsl()
                                        }
                                    }
                                },
                                new RotationModifier {RotationRate = -2.1f},
                            }
                        }
                    }
                };
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                spriteBatch.Draw(this._particleEffect);
            }

            public void Update(GameTime gameTime)
            {
                this._particleEffect.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            ~EnemyDestructionEffect()
            {
                this._particleTexture.Dispose();
                this._particleEffect.Dispose();
            }
        }

        private EnemyDestructionEffect[] destructionEffect;

        public Enemys(Texture2D[] textures, int numOfEnemys)
        {
            this.Rand = new Random();

            this.Textures = textures; // all enemys textures must be the same size

            this.Origin = new Vector2(
                this.Textures[0].Bounds.Center.X,
                this.Textures[0].Bounds.Center.Y
            );

            this.PrevNumOfEnemys = numOfEnemys;
            this.Init(numOfEnemys);
        }

        private void Init(int numOfEnemys)
        {
            /* Arrays & Lists Creation */

            this.NumOfEnemys = numOfEnemys;

            this.Lifes           = new List<int>();
            this.TextureOfEnemys = new List<int>();
            this.RotateEnemy     = new List<bool>();
            this.DestructEnemy   = new List<bool>();
            this.Position        = new Vector2[numOfEnemys];
            this.Speed           = new float[numOfEnemys];
            this.Direction       = new int[numOfEnemys, 2];
            this.Rotation        = new float[numOfEnemys];
            this.Scale           = new float[numOfEnemys];
            this.Colour          = new List<Color>();

            this.destructionEffect = new EnemyDestructionEffect[numOfEnemys];

            /* Calculation of the player's safe zone for the verification after */

            float Xp_1 = Game1.player.Position.X - 256f, Xp_2 = Game1.player.Position.X + 256f;
            float Yp_1 = Game1.player.Position.X - 256f, Yp_2 = Game1.player.Position.X + 256f;

            /* Generation des enemys */

            for (int i = 0; i < numOfEnemys; ++i)
            {
                /* Set random position, without one being above another or in contact with the player */

                bool position_is_okay; do {
                    position_is_okay = true;

                    /* Generating a position for the enemy being created */

                    this.Position[i].X = (float)this.Rand.Next(
                        this.Textures[0].Bounds.Center.X+1,
                        Game1.screenSize[0]-this.Textures[0].Bounds.Center.X
                    );

                    this.Position[i].Y = (float)this.Rand.Next(
                        this.Textures[0].Bounds.Center.Y+1,
                        Game1.screenSize[1]-this.Textures[0].Bounds.Center.Y
                    );

                    /* Calculation of the generated position of the enemy being created */

                    float Xi_1 = this.Position[i].X - this.Textures[0].Bounds.Center.X,
                        Xi_2 = this.Position[i].X + this.Textures[0].Bounds.Center.X;

                    float Yi_1 = this.Position[i].Y - this.Textures[0].Bounds.Center.Y,
                        Yi_2 = this.Position[i].Y + this.Textures[0].Bounds.Center.Y;

                    /* Verification if the generated position is in collision with the player */

                    if ( ( (Xi_1 > Xp_1 && Xi_1 < Xp_2) || (Xi_2 > Xp_1 && Xi_2 < Xp_2) )
                    &&  ( (Yi_1 > Yp_1 && Yi_1 < Yp_2) || (Yi_2 > Yp_1 && Yi_2 < Yp_2) ) )
                    {
                        position_is_okay = false;
                    }

                    /* If OK, check if the generated position is in collision with another enemy */

                    else
                    {
                        for (int j = 0; j < i; j++)
                        {
                            float Xj_1 = this.Position[j].X - this.Textures[0].Bounds.Center.X,
                                Xj_2 = this.Position[j].X + this.Textures[0].Bounds.Center.X;

                            float Yj_1 = this.Position[j].Y - this.Textures[0].Bounds.Center.Y,
                                Yj_2 = this.Position[j].Y + this.Textures[0].Bounds.Center.Y;

                            if ( ( (Xi_1 > Xj_1 && Xi_1 < Xj_2) || (Xi_2 > Xj_1 && Xi_2 < Xj_2) )
                            &&  ( (Yi_1 > Yj_1 && Yi_1 < Yj_2) || (Yi_2 > Yj_1 && Yi_2 < Yj_2) ) )
                            {
                                position_is_okay = false; break;
                            }
                        }
                    }

                } while(!position_is_okay);

                /* Set random direction (without direction being null ->= 0) */

                do {
                    this.Direction[i, 0] = this.Rand.Next(-1, 2); // X
                    this.Direction[i, 1] = this.Rand.Next(-1, 2); // Y
                } while (this.Direction[i, 0] == 0 || this.Direction[i, 1] == 0);

                /* Init other values of the enemy */

                this.Lifes.Add(10);
                this.TextureOfEnemys.Add(this.Rand.Next(0,2));
                this.RotateEnemy.Add(false);
                this.DestructEnemy.Add(false);

                this.Speed[i]    = 100f;
                this.Rotation[i] = 0f;
                this.Scale[i]    = 1f;

                this.Colour.Add(Color.FromNonPremultiplied((byte)this.Rand.Next(256), (byte)this.Rand.Next(256), (byte)this.Rand.Next(256), (byte)255)); // Set random color
            }
        }

        private void Destroy(int index_of_enemy) // Destroy the indicated enemy
        {
            this.Lifes.RemoveAt(index_of_enemy);

            this.TextureOfEnemys.RemoveAt(index_of_enemy);

            this.RotateEnemy.RemoveAt(index_of_enemy);

            this.DestructEnemy.RemoveAt(index_of_enemy);

            this.Position = this.Position.Where((source, index) => index != index_of_enemy).ToArray();

            this.Speed = this.Speed.Where((source, index) => index != index_of_enemy).ToArray();

            this.Direction = Tools.DeleteRow(index_of_enemy, this.Direction);

            this.Rotation = this.Rotation.Where((source, index) => index != index_of_enemy).ToArray();

            this.Scale = this.Scale.Where((source, index) => index != index_of_enemy).ToArray();

            this.Colour.RemoveAt(index_of_enemy);

            this.destructionEffect = this.destructionEffect.Where((source, index) => index != index_of_enemy).ToArray();

            --this.NumOfEnemys;
        }

        private Vector2 GetMovement(int i, GameTime gameTime)
        {
            return new Vector2 (
                this.Position[i].X + (this.Direction[i, 0] * this.Speed[i]) * (float)gameTime.ElapsedGameTime.TotalSeconds,
                this.Position[i].Y + (this.Direction[i, 1] * this.Speed[i]) * (float)gameTime.ElapsedGameTime.TotalSeconds
            );
        }

        public void Update(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            float Xp_1 = Game1.player.Position.X - 16f, Xp_2 = Game1.player.Position.X + 16f;
            float Yp_1 = Game1.player.Position.Y - 16f, Yp_2 = Game1.player.Position.Y + 16f;

            if (this.NumOfEnemys < 1)
                this.Init(++this.PrevNumOfEnemys);

            for (int i = 0; i < this.NumOfEnemys; ++i)
            {
                if (this.DestructEnemy[i])
                {
                    destructionEffect[i].Update(gameTime);
                    this.Scale[i] -= .5f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (this.Scale[i] < 0f) this.Destroy(i);
                }
                else /* If enemy is alive */
                {
                    /* Apply movement for 'i' enemy */

                    this.Position[i] = this.GetMovement(i, gameTime);

                    /* Rotation if enemy was shooted */

                    if (this.RotateEnemy[i])
                    {
                        this.Rotation[i] += 20 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (this.Rotation[i] > 6.2831855f) // 6,2831855 -> 360 degrees
                        {
                            this.Rotation[i] = 0f;
                            this.RotateEnemy[i] = false;
                        }
                    }

                    /* Get enemy (i) collide box */

                    float Xi_1 = this.Position[i].X - this.Textures[0].Bounds.Center.X,
                        Xi_2 = this.Position[i].X + this.Textures[0].Bounds.Center.X;

                    float Yi_1 = this.Position[i].Y - this.Textures[0].Bounds.Center.Y,
                        Yi_2 = this.Position[i].Y + this.Textures[0].Bounds.Center.Y;

                    /* Check if the enemy hits the player */

                    if ( ( (Xi_1 > Xp_1 && Xi_1 < Xp_2) || (Xi_2 > Xp_1 && Xi_2 < Xp_2) )
                    &&  ( (Yi_1 > Yp_1 && Yi_1 < Yp_2) || (Yi_2 > Yp_1 && Yi_2 < Yp_2) ) )
                    {
                        this.Destroy(i); --Game1.player.Life; continue;
                    }

                    /* Check if one of the bullets hits the enemy */

                    bool continue_loop = true;

                    for (int b = 0; b < Game1.bullets.NumActiveBullets; ++b)
                    {
                        if ((Xi_1 < Game1.bullets.Position[b].X && Xi_2 > Game1.bullets.Position[b].X)
                         && (Yi_1 < Game1.bullets.Position[b].Y && Yi_2 > Game1.bullets.Position[b].Y))
                        {
                            Game1.bullets.IndexBulletToDestroy = b;

                            if (this.Lifes[i] > 1)
                            {
                                --this.Lifes[i];
                                this.Scale[i] -= .0193333f; // base -> life 30: .0083333f
                                this.RotateEnemy[i] = true;
                                this.Colour[i] = Color.FromNonPremultiplied(
                                    (byte)this.Rand.Next(256),
                                    (byte)this.Rand.Next(256),
                                    (byte)this.Rand.Next(256),
                                    (byte)255
                                );
                            }
                            else
                            {
                                ++Game1.player.Score;

                                this.destructionEffect[i] = new EnemyDestructionEffect(
                                    graphicsDevice,
                                    (int)this.Position[i].X,
                                    (int)this.Position[i].Y,
                                    this.Textures[0].Width/2,
                                    this.Colour[i]
                                );

                                if (Settings.IsSoundOn)
                                    destructionSound.Play();

                                this.DestructEnemy[i] = true;
                                continue_loop = false;
                                break;
                            }
                        }
                    }

                    if (!continue_loop) continue; // Stop if enemy (i) is shooted

                    /*
                        Verification of collision with another enemy then inversion of directions.
                        Note: All the tests that follow the inversion of motion are used to avoid a blocking nesting.
                            To see if a better technique would be feasible because nothing simpler was 100% effective.
                    */

                    for (int j = i+1; j < this.NumOfEnemys; ++j)
                    {
                        if (this.DestructEnemy[j]) continue;

                        float Xj_1 = this.Position[j].X - this.Textures[0].Bounds.Center.X,
                            Xj_2 = this.Position[j].X + this.Textures[0].Bounds.Center.X;

                        float Yj_1 = this.Position[j].Y - this.Textures[0].Bounds.Center.Y,
                            Yj_2 = this.Position[j].Y + this.Textures[0].Bounds.Center.Y;

                        if ( ( (Xi_1 > Xj_1 && Xi_1 < Xj_2) || (Xi_2 > Xj_1 && Xi_2 < Xj_2) )
                        &&  ( (Yi_1 > Yj_1 && Yi_1 < Yj_2) || (Yi_2 > Yj_1 && Yi_2 < Yj_2) ) )
                        {
                            for (int k = 0; k < 2; ++k) // Invert directions
                            {
                                this.Direction[i, k] = -this.Direction[i, k];
                                this.Direction[j, k] = -this.Direction[j, k];
                            }

                            /* Calculate and test the next move to know if they will be nested */

                            Vector2 futureMovment = GetMovement(i, gameTime);

                            float fXi_1 = futureMovment.X - this.Textures[0].Bounds.Center.X,
                                fXi_2 = futureMovment.X + this.Textures[0].Bounds.Center.X;

                            float fYi_1 = futureMovment.Y - this.Textures[0].Bounds.Center.Y,
                                fYi_2 = futureMovment.Y + this.Textures[0].Bounds.Center.Y;

                            while ( ( (fXi_1 > Xj_1 && fXi_1 < Xj_2) || (fXi_2 > Xj_1 && fXi_2 < Xj_2) )
                                &&  ( (fYi_1 > Yj_1 && fYi_1 < Yj_2) || (fYi_2 > Yj_1 && fYi_2 < Yj_2) ) )
                            {
                                this.Position[i].X = futureMovment.X += this.Direction[i, 0];
                                this.Position[i].Y = futureMovment.Y += this.Direction[i, 1];

                                fXi_1 = futureMovment.X - this.Textures[0].Bounds.Center.X;
                                fXi_2 = futureMovment.X + this.Textures[0].Bounds.Center.X;
                                fYi_1 = futureMovment.Y - this.Textures[0].Bounds.Center.Y;
                                fYi_2 = futureMovment.Y + this.Textures[0].Bounds.Center.Y;
                            }
                        }
                    }

                    /* Reverse direction if the enemy hits a wall */

                    if (this.Position[i].X > Game1.screenSize[0]-this.Textures[0].Bounds.Center.X)
                    {
                        this.Position[i].X = Game1.screenSize[0]-this.Textures[0].Bounds.Center.X;
                        this.Direction[i, 0] = -this.Direction[i, 0];
                    }
                    else if (this.Position[i].X < this.Textures[0].Bounds.Center.X)
                    {
                        this.Position[i].X = this.Textures[0].Bounds.Center.X;
                        this.Direction[i, 0] = -this.Direction[i, 0];
                    }
                    if (this.Position[i].Y > Game1.screenSize[1]-this.Textures[0].Bounds.Center.Y)
                    {
                        this.Position[i].Y = Game1.screenSize[1]-this.Textures[0].Bounds.Center.Y;
                        this.Direction[i, 1] = -this.Direction[i, 1];
                    }
                    else if (this.Position[i].Y < this.Textures[0].Bounds.Center.Y)
                    {
                        this.Position[i].Y = this.Textures[0].Bounds.Center.Y;
                        this.Direction[i, 1] = -this.Direction[i, 1];
                    }

                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < this.NumOfEnemys; ++i)
            {
                if (this.DestructEnemy[i])
                    destructionEffect[i].Draw(spriteBatch);

                spriteBatch.Draw(
                    this.Textures[this.TextureOfEnemys[i]],
                    this.Position[i],
                    null,
                    this.Colour[i],
                    this.Rotation[i],
                    this.Origin,
                    this.Scale[i],
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }

}

public class ShowScore
{
    private SpriteFont Font;
    private Vector2 Position;
    private Vector2 Origin;

    public const string defText = "SCORE: 0 ";

    private int PrevScore;
    private string Text;

    public ShowScore(SpriteFont font, int posX, int posY)
    {
        this.Font = font; this.Text = defText;
        this.Position = new Vector2(posX, posY);
        this.Origin = this.Font.MeasureString(this.Text)*.5f;
        this.PrevScore = Game1.player.Score;
    }

    public void Update()
    {
        if (Game1.player.Score > this.PrevScore)
        {
            this.PrevScore = Game1.player.Score;
            this.Text = "SCORE: " + this.PrevScore.ToString() + " ";
            this.Origin = this.Font.MeasureString(this.Text)*.5f;
            this.Position.X = Game1.screenSize[0] - this.Origin.X;
        }
        else if (GameObjects.Reset)
        {
            this.Text = defText;
            this.PrevScore = 0;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(
            this.Font,
            this.Text,
            this.Position,
            Color.White,
            0f,
            this.Origin,
            1f,
            SpriteEffects.None,
            0f
        );
    }
}

#endregion

#region G_OVER

public class GameOverLogo
{
    private Texture2D Texture;
    private Vector2 Position;
    private Vector2 Origin;
    private Color Colour;
    private bool Show;
    private Random Rand;

    public GameOverLogo(Texture2D texture, int posX, int posY)
    {
        this.Texture = texture;

        this.Position = new Vector2(posX, posY);

        this.Origin = new Vector2(
            this.Texture.Bounds.Center.X,
            this.Texture.Bounds.Center.Y
        );

        this.Colour = Color.White;

        Rand = new Random();
    }

    public void Update(GameTime gameTime)
    {
        this.Show = (int)gameTime.TotalGameTime.TotalSeconds % 2 == 0;

        if (!Show) this.Colour = Color.FromNonPremultiplied(
            this.Rand.Next(0,256), this.Rand.Next(0,256),
            this.Rand.Next(0,256), 255
        );
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (this.Show)
            spriteBatch.Draw(
                this.Texture, 
                this.Position, 
                null, 
                this.Colour, 
                0f,
                this.Origin,
                1f,
                SpriteEffects.None, 
                0f
            );
    }
}

#endregion

#region G_SETTINGS

public static class Settings
{
    public static Keys[] WASD { get; private set; } = new Keys[4] { Keys.W, Keys.S, Keys.A, Keys.D };
    public static Keys[] ZQSD { get; private set; } = new Keys[4] { Keys.Z, Keys.S, Keys.Q, Keys.D };

    public static Keys[] SelectedControl = WASD;
    public static bool   IsSoundOn = true;

    public class ButtonSetControls : Button
    {
        public ButtonSetControls(Texture2D texture, int posX, int posY, SoundEffect sound, SpriteFont font, string text)
            : base(texture, posX, posY, sound, font, text)
            {
                if (SelectedControl == ZQSD)
                     this.Text = "ZQSD";
                else this.Text = "WASD";
            }

        public override void Action()
        {
            if (SelectedControl == WASD)
            {
                SelectedControl = ZQSD;
                this.Text = "ZQSD";
            }
            else
            {
                SelectedControl = WASD;
                this.Text = "WASD";
            }
            GameObjects.Reset = true;
        }
    }

    public class ButtonSoundControl : Button
    {
        public ButtonSoundControl(Texture2D texture, int posX, int posY, SoundEffect sound, SpriteFont font, string text)
            : base(texture, posX, posY, sound, font, text) { }

        public override void Action()
        {
            IsSoundOn = !IsSoundOn;

            if (IsSoundOn)
                 this.Text = "SOUND ON";
            else this.Text = "SOUND OFF";
        }
    }
}

#endregion