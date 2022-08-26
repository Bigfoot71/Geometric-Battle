using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace GeometricBattle;

public enum GameState {
    G_TITLE = 0,
    G_PLAY = 1,
    G_OVER = 2,
    G_SETTINGS = 3
}

public class Game1 : Game
{
    public static int[] screenSize { get; private set; }
    public static string savePath { get; private set; }
    public static GameState gameState;

    private Cursor _cursor;

    private Logo _titleLogo;
    private Button _buttonPlay;
    private Button _buttonTitle;
    private Button _buttonSettings;
    private Button _buttonSetControls;
    private Button _buttonSoundControl;
    private ShowHiScore _showHiScore;
    private SpriteFont _generalFont;
    private Vector2 _creditsPosition;

    public static int HiScore = 0;
    public static GameObjects.Player player;
    public static GameObjects.Bullets bullets;
    public static GameObjects.Enemys enemys;
    private ShowScore _showScore;

    private GameOverLogo _gameOverLogo;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    #if DEBUG
    private FrameCounter _frameCounter;
    #endif

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void Initialize()
    {
        screenSize = new int[2] {
            _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight
        };

        /* Check save directory and create if necessary */

        savePath = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "Bigfoot71 Games", "Geometric-Battle"
        );

        if (!System.IO.Directory.Exists(savePath))
        {
            System.IO.Directory.CreateDirectory(savePath);

            #if DEBUG
            System.Console.WriteLine("INFO: '"+savePath+"' directory is created.");
            #endif
        }

        /* Load save if file exist */

        savePath = System.IO.Path.Combine(savePath, "save.dat");

        Tools.LoadGame();

        /* Set default game state */

        gameState = GameState.G_TITLE;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        /* Initializing content load variables */

        Texture2D   loadedTexture;
        SpriteFont  loadedFont;
        SoundEffect loadedSound;

        /* Load Cursor */

        loadedTexture = this.Content.Load<Texture2D>("images/cursors/01");
        _cursor = new Cursor(loadedTexture);

        /* Load title logo */

        loadedTexture = this.Content.Load<Texture2D>("images/logo");
        _titleLogo = new Logo(loadedTexture, screenSize[0]/2, (int)(screenSize[1]*.25f));

        /* Load buttons */

        loadedTexture = this.Content.Load<Texture2D>("images/buttons/01");
        loadedFont = Content.Load<SpriteFont>("fonts/buttonFont");
        loadedSound   = this.Content.Load<SoundEffect>("sounds/click");

        _buttonPlay = new ButtonPlay(loadedTexture, screenSize[0]/2, (int)(screenSize[1]*.7f), loadedSound, loadedFont, "PLAY");
        _buttonTitle = new ButtonTitle(loadedTexture, screenSize[0]/2, (int)(screenSize[1]*.775f), loadedSound, loadedFont, "TITLE");
        _buttonSettings = new ButtonSettings(loadedTexture, screenSize[0]/2, (int)(screenSize[1]*.85f), loadedSound, loadedFont, "SETTINGS");

        /* Load button settings */

        _buttonSetControls = new Settings.ButtonSetControls(loadedTexture, (int)(screenSize[0]*.35f), screenSize[1]/2, loadedSound, loadedFont, "WASD");
        _buttonSoundControl = new Settings.ButtonSoundControl(loadedTexture, (int)(screenSize[0]*.65f), screenSize[1]/2, loadedSound, loadedFont, "SOUND ON");

        /* Load GameObjects */

        GameObjects.playerTexture  = this.Content.Load<Texture2D>("images/player");
        GameObjects.bulletsTexture = this.Content.Load<Texture2D>("images/bullet");
        GameObjects.enemysTextures = new Texture2D[2] {
            this.Content.Load<Texture2D>("images/enemys/01"),
            this.Content.Load<Texture2D>("images/enemys/02")
        };

        GameObjects.shootSound       = this.Content.Load<SoundEffect>("sounds/shoot");
        GameObjects.impactSound      = this.Content.Load<SoundEffect>("sounds/impact");
        GameObjects.destructionSound = this.Content.Load<SoundEffect>("sounds/destruction");
        GameObjects.gameOverSound    = this.Content.Load<SoundEffect>("sounds/game_over");

        GameObjects.Init();

        /* Load hiscore display */

        loadedFont = Content.Load<SpriteFont>("fonts/scoreFont");
        _showHiScore = new ShowHiScore(loadedFont, (int)(screenSize[0]*.5f), (int)(screenSize[1]*.5f));

        /* Load score display */

        Vector2 textSize = loadedFont.MeasureString(ShowScore.defText);
        _showScore = new ShowScore(loadedFont, (int)(screenSize[0] - textSize.X*.5f), (int)(textSize.Y*.6f));

        /* Load gme over logo */

        loadedTexture = this.Content.Load<Texture2D>("images/game_over");
        _gameOverLogo = new GameOverLogo(loadedTexture, screenSize[0]/2, (int)(screenSize[1]*.35f));

        /* Load general font */

        _generalFont = Content.Load<SpriteFont>("fonts/generalFont");

        /* Set credits position (in G_Settings) */

        _creditsPosition = new Vector2(0, screenSize[1] - _generalFont.MeasureString("0").Y);

        /* Load FrameCounter if DEBUG mode */

        #if DEBUG
        _frameCounter = new FrameCounter(0, 0, 1, false, loadedFont, Color.Yellow);
        #endif
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState kState = Keyboard.GetState();
        MouseState    mState = Mouse.GetState();

        if (kState.IsKeyDown(Keys.Escape)) Exit();

        switch (gameState)
        {
            case GameState.G_TITLE:
                _titleLogo.Update(mState, gameTime);
                _buttonPlay.Update(mState);
                _buttonSettings.Update(mState);
                _showHiScore.Update();
                break;

            case GameState.G_PLAY:
                player.Update(kState, mState, gameTime);
                bullets.Update(mState, gameTime);
                enemys.Update(_graphics.GraphicsDevice, gameTime);
                _showScore.Update();
                break;

            case GameState.G_OVER:
                _gameOverLogo.Update(gameTime);
                _buttonTitle.Update(mState);
                break;

            case GameState.G_SETTINGS:
                _buttonSetControls.Update(mState);
                _buttonSoundControl.Update(mState);
                _buttonTitle.Update(mState);
                break;
        }

        _cursor.Update(mState);

        #if DEBUG
        _frameCounter.Update(gameTime);
        #endif

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();
        {
            switch (gameState)
            {
                case GameState.G_TITLE:
                    _titleLogo.Draw(_spriteBatch);
                    _buttonPlay.Draw(_spriteBatch);
                    _buttonSettings.Draw(_spriteBatch);
                    _showHiScore.Draw(_spriteBatch);
                    break;

                case GameState.G_PLAY:
                    player.Draw(_spriteBatch);
                    bullets.Draw(_spriteBatch);
                    enemys.Draw(_spriteBatch);
                    _showScore.Draw(_spriteBatch);
                    break;

                case GameState.G_OVER:
                    _gameOverLogo.Draw(_spriteBatch);
                    _buttonTitle.Draw(_spriteBatch);
                    break;

                case GameState.G_SETTINGS:
                    _buttonSetControls.Draw(_spriteBatch);
                    _buttonSoundControl.Draw(_spriteBatch);
                    _buttonTitle.Draw(_spriteBatch);
                    _spriteBatch.DrawString( // Draw credits
                        _generalFont,
                        " Credits: Bigfoot71",
                        _creditsPosition,
                        Color.White
                    );
                    break;
            }

            _cursor.Draw(_spriteBatch);

            #if DEBUG
            _frameCounter.Draw(_spriteBatch);
            #endif
        }
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
