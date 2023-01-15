using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

// Code and project made by Joacim Bäcklund @ Webbutvecklingsprogrammet 2023 for the course DT071G
// Inspiration and guidance taken from the WPF C# tutorials created by Moo ICT @ www.mooict.com

namespace Projekt
{
    public partial class MainWindow : Window
    {
        //Variables
        
        private DispatcherTimer GameTimer = new DispatcherTimer();

        //Randomizer used for spawn locations off screen
        private Random random = new Random();

        //List used for removed enemies from game screen
        private List<Rectangle> removed = new List<Rectangle>();

        //Directional keys true or false
        private bool LeftKey, RightKey;

        //Players hitbox
        private Rect playerHitbox;

        //Players speed in-game
        private int playerSpeed = 20;

        //Currently spawned enemies counter
        private int enemiesAlive = 0;

        //Total amount of enemies on screen
        private int enemiesTotal = 1;

        //Enemy speed in-game
        private int enemySpeed = 8;

        //In-game score. Default is 0. 
        private int score = 0;

        //Remaining lives before game over. Default is 5 lives.
        private int lives = 5;

        //Images for game objects with Image Brush
        ImageBrush playerImg = new ImageBrush();
        ImageBrush backgroundImg = new ImageBrush();
        ImageBrush enemyImg = new ImageBrush();

        //Load sound effects
        MediaPlayer plopSound = new MediaPlayer();
        MediaPlayer splatSound = new MediaPlayer();
        MediaPlayer fartSound = new MediaPlayer();
        MediaPlayer fartEcho = new MediaPlayer();
        MediaPlayer winSound = new MediaPlayer();


        public MainWindow()
        {
            //Start application
            InitializeComponent();
            GameScreen.Focus();

            //Play sound on game start
            fartSound.Open(new Uri("../../sfx/fart_short.mp3", UriKind.RelativeOrAbsolute));
            fartSound.Play();

            //Set 60 fps
            GameTimer.Interval = TimeSpan.FromMilliseconds(16);

            //Tick for the game engine
            GameTimer.Tick += GameTick;

            //Load player image from file source on game start
            playerImg.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/player.png"));
            Player.Fill = playerImg;

            //Load background image from file source on game start
            backgroundImg.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/background.png"));
            GameScreen.Background = backgroundImg;

            //Start game
            GameTimer.Start();
        }

        private void GameTick(object sender, EventArgs e)
        {
            
            //Player movement. When key is pressed, find player position and move in chosen direction

            //Move player to the left on screen
            if (LeftKey == true && Canvas.GetLeft(Player) > 10)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) - playerSpeed);
            }
            //Move player to the right on screen
            if (RightKey == true && Canvas.GetLeft(Player) + (Player.Width + 20) < Application.Current.MainWindow.Width)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) + playerSpeed);
            }
            //Enable player hitbox
            playerHitbox = new Rect(Canvas.GetLeft(Player), Canvas.GetTop(Player), Player.Width, Player.Height);

            //Display player score
            ScoreText.Content = "S: " + score;

            //Display player lives
            LivesText.Content = "Life: " + lives;

            //Enemy total spawn amount control
            if (enemiesAlive < enemiesTotal)
            {
                SpawnEnemies();
                enemiesAlive++;
                removed.Clear();
            }

            //Start enemy spawning with a loop
            foreach (var enemies in GameScreen.Children.OfType<Rectangle>())
            {
                //Look for enemys tag from object properties
                if ((string)enemies.Tag == "Poop")
                {   
                    //Enemy spawning location and speed
                    Canvas.SetTop(enemies, Canvas.GetTop(enemies) + enemySpeed);

                    //Add hitboxes to spawned enemies
                    Rect enemyHitbox = new Rect(Canvas.GetLeft(enemies), Canvas.GetTop(enemies), enemies.Width, enemies.Height);

                    //Turn on object collision detection, when enemy touches player the score will increase and enemy count will go down.
                    //Sound effect "plop.mp3" will play
                    if (playerHitbox.IntersectsWith(enemyHitbox))
                    {
                        plopSound.Open(new Uri("../../sfx/plop.mp3", UriKind.RelativeOrAbsolute));
                        plopSound.Play();
                        removed.Add(enemies);
                        enemiesAlive--;
                        score += 100;
                    }

                    //When an enemy gets too far down, it will despawn and enemy counter will go down.
                    //Sound effect "splat.mp3" will play
                    if (Canvas.GetTop(enemies) > 595)
                    {
                        splatSound.Open(new Uri("../../sfx/splat.mp3", UriKind.RelativeOrAbsolute));
                        splatSound.Play();
                        removed.Add(enemies);
                        enemiesAlive--;
                        lives--;
                    }
                }
            }

            //Completley delete removed enemies from list
            foreach (var i in removed)
            {
                GameScreen.Children.Remove(i);
            }

            //Levels and difficulty. New game starts on easiest but it gets harder when the score amount increases

            //Level 2
            if (score > 800)
            {
                enemiesTotal = 3;
                LevelText.Content = "Lvl 2";
                LevelText.Foreground = Brushes.YellowGreen;
            }

            //Level 3
            if (score > 2000)
            {
                enemySpeed = 12;
                LevelText.Content = "Lvl 3";
                LevelText.Foreground = Brushes.Yellow;
            }

            //Level 4
            if (score > 3000)
            {
                enemiesTotal = 5;
                LevelText.Content = "Lvl 4";
                LevelText.Foreground = Brushes.Brown;
            }

            //Level 5
            if (score > 5000)
            {
                enemiesTotal = 8;
                enemySpeed = 15;
                LevelText.Content = "Lvl 5";
                LevelText.Foreground = Brushes.Red;
            }

            //Max score. Game is won!
            if (score == 10000)
            {
                GameTimer.Stop();

                winSound.Open(new Uri("../../sfx/victory.mp3", UriKind.RelativeOrAbsolute));
                winSound.Play();

                //Message box letting the player choose to keep playing or not.
                if (MessageBox.Show("You reached max points and beat the game!", "Congratulations", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    //If no the application will close
                    Application.Current.Shutdown();
                }
                else
                {
                    //If yes the current application will close but the game will restart
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }
            }

            //Game over when all lives are lost
            if (lives < 1)
            {
                LivesText.Content = "Life: 0";
                GameTimer.Stop();

                fartEcho.Open(new Uri("../../sfx/fart_echo.mp3", UriKind.RelativeOrAbsolute));
                fartEcho.Play();

                //Message box letting the player choose to keep playing or not.
                if (MessageBox.Show("Your score: " + score + Environment.NewLine + "Play again?", "Game Over", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    //If no the application will close
                    Application.Current.Shutdown();
                }
                else
                {
                    //If yes the current application will close but the game will restart
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }
            }
        }

        //Event for when directional keys (left or right) are pressed down
        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                LeftKey = true;  
            }
            if (e.Key == Key.Right)
            {
                RightKey = true;
            }
        }

        //Event for when directional keys (left or right) are released
        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                LeftKey = false;
            }
            if (e.Key == Key.Right)
            {
                RightKey = false;
            }
        }

        private void SpawnEnemies()
        {
            //Load enemy image from file source
            enemyImg.ImageSource = new BitmapImage(new Uri("pack://application:,,,/img/poop.png"));

            //Create new enemy (rectangle) object with properties 
            Rectangle enemy = new Rectangle
            {
                Tag = "Poop",
                Height = 50,
                Width = 50,
                Fill = enemyImg
            };

            //Set enemy spawn location off-screen
            Canvas.SetLeft(enemy, random.Next(5, 450));
            Canvas.SetTop(enemy, random.Next(50, 100) * -1);
            
            //Spawn enemy to the in-game screen
            GameScreen.Children.Add(enemy);
        }
    }
}
