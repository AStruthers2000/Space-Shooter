using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace SpaceShooter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DispatcherTimer gameTimer = new DispatcherTimer();
        bool moveLeft, moveRight;
        List<Rectangle> itemRemover = new List<Rectangle>();

        Random rand = new Random();

        int enemySpriteCounter = 0;

        //enemyCounter and limit are the seconds between spawns
        //enemyCounter should start higher than limit so that the player has a few more seconds before game starts
        double enemyCounter = 10;
        int limit = 5;

        //scalar multiplier to control how fast enemies spawn. a higher multiplier means spawning faster
        double enemySpawnRate = 1.0;

        int enemySpeed = 250;
        int playerSpeed = 300;
        int bulletSpeed = 600;

        int score = 0;
        int damage = 0;

        int edgePadding = 10;

        Rect playerHitBox;

        int targetFPS = 144;

        Stopwatch timeManager = Stopwatch.StartNew();
        float deltaTime = 0.0f;

        List<ImageBrush> enemySprites = new List<ImageBrush>();

        public MainWindow()
        {
            InitializeComponent();

            //frequency = 1/period -> period = 1/frequency
            gameTimer.Interval = TimeSpan.FromSeconds(1.0 / targetFPS);

            //assigning the gameTimer Tick event call to custom function GameLoop 
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            GameScreen.Focus();

            //loading and setting background image
            ImageBrush bg = new ImageBrush();
            bg.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/purple.png"));
            bg.TileMode = TileMode.Tile;
            bg.Viewport = new Rect(0, 0, 0.15, 0.15);
            bg.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            GameScreen.Background = bg;

            //loading player image
            ImageBrush playerImage = new ImageBrush();
            playerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/player.png"));
            Player.Fill = playerImage;

            //load all enemy sprites in initalization
            for(int i = 1; i <= 5; i++)
            {
                ImageBrush temp = new ImageBrush();

                string uri = string.Format("pack://application:,,,/Assets/{0}.png", i);
                temp.ImageSource = new BitmapImage(new Uri(uri));
                enemySprites.Add(temp);
            }

            //initializing labels
            lbl_ScoreText.Content = "Score: " + score;
            lbl_DamageText.Content = "Damage: " + damage;
        }

        private void GameLoop(object sender, EventArgs e)
        {
            //calculating deltaTime
            timeManager.Stop();
            deltaTime = timeManager.ElapsedMilliseconds / 1000.0f;
            timeManager.Restart();

            playerHitBox = new Rect(Canvas.GetLeft(Player), Canvas.GetTop(Player), Player.Width, Player.Height);

            enemyCounter -= enemySpawnRate * deltaTime;

            if (enemyCounter < 0)
            {
                MakeEnemy();
                enemyCounter = limit;
            }

            if (moveLeft && playerHitBox.X > 0 + edgePadding)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) - playerSpeed * deltaTime);
            }

            //for some reason, frame is always 17 pixels wider than specified
            if (moveRight && playerHitBox.X + playerHitBox.Width < Application.Current.MainWindow.Width - 17 - edgePadding)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) + playerSpeed * deltaTime);
            }

            foreach (Rectangle r in GameScreen.Children.OfType<Rectangle>())
            {
                if ((string)r.Tag == "bullet")
                {
                    
                    Canvas.SetTop(r, Canvas.GetTop(r) - bulletSpeed * deltaTime);

                    Rect bulletHitBox = new Rect(Canvas.GetLeft(r), Canvas.GetTop(r), r.Width, r.Height);

                    if (Canvas.GetTop(r) + r.Width < 0)
                    {
                        itemRemover.Add(r);
                    }

                    foreach (Rectangle r1 in GameScreen.Children.OfType<Rectangle>())
                    {
                        if ((string)r1.Tag == "enemy")
                        {
                            Rect enemyHit = new Rect(Canvas.GetLeft(r1), Canvas.GetTop(r1) + r1.Height / 4, r1.Width, r1.Height - r1.Height / 2);

                            if (bulletHitBox.IntersectsWith(enemyHit))
                            {
                                itemRemover.Add(r);
                                itemRemover.Add(r1);

                                score++;

                                //TODO: make enemy spawn rate calculations into a function
                                if (score > 5)
                                {
                                    if (score >= 100)
                                    {
                                        enemySpawnRate = 20.0;
                                    }
                                    else
                                    {
                                        enemySpawnRate = ((score - 5.0) * (score - 5.0)) / 500.0 + 1.0;
                                    }
                                }

                                //TODO: make updating labels into a function
                                lbl_ScoreText.Content = "Score: " + score;
                            }
                        }
                    }
                }

                if ((string)r.Tag == "enemy")
                {
                    Canvas.SetTop(r, Canvas.GetTop(r) + enemySpeed * deltaTime);

                    //TODO: make taking damage as well as updating labels into two seperate functions

                    //if the enemy is at twice the height of the screen, remove them
                    if (Canvas.GetTop(r) > Application.Current.MainWindow.Height + edgePadding)
                    {
                        itemRemover.Add(r);

                        //if you don't shoot the enemy, and it makes it to your side, you take damage
                        damage += 10;
                        lbl_DamageText.Content = "Damage: " + damage;
                    }

                    Rect enemyHitBox = new Rect(Canvas.GetLeft(r), Canvas.GetTop(r), r.Width, r.Height);

                    if (playerHitBox.IntersectsWith(enemyHitBox))
                    {
                        itemRemover.Add(r);

                        //if the enemy hits you, you take damage
                        damage += 5;
                        lbl_DamageText.Content = "Damage: " + damage;
                    }
                }
            }

            //TODO: make removing items into a function
            if (itemRemover.Count > 0)
            {
                foreach (Rectangle r in itemRemover)
                {
                    GameScreen.Children.Remove(r);
                }
                itemRemover.Clear();
            }

            //TODO: change the restart game code to a function, and make it so that the current instance resets, not opening a new process
            if(damage > 99)
            {
                gameTimer.Stop();
                timeManager.Stop();
                lbl_DamageText.Content = "Damage: 100";
                lbl_DamageText.Foreground = Brushes.Red;

                MessageBox.Show("Captain! You have destroyed " + score + " Alien Ships\n Press OK to Play Again", "Shooter Game:");
                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                moveLeft = true;
            }
            if (e.Key == Key.Right)
            {
                moveRight = true;
            }

            if (e.Key == Key.Space)
            {
                //TODO: potentially turn bullet into a class with custom constructor for custom images, speed, damage, etc.
                Rectangle newBullet = new Rectangle
                {
                    Tag = "bullet",
                    Height = 20,
                    Width = 5,
                    Fill = Brushes.White,
                    Stroke = Brushes.Red
                };
                Canvas.SetLeft(newBullet, Canvas.GetLeft(Player) + Player.Width / 2 - newBullet.Width / 2);
                Canvas.SetTop(newBullet, Canvas.GetTop(Player) - newBullet.Height);

                GameScreen.Children.Add(newBullet);
            }

            if (e.Key == Key.Escape || e.Key == Key.P)
            {
                TogglePause();
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                moveLeft = false;
            }
            if (e.Key == Key.Right)
            {
                moveRight = false;
            }
        }

        private void OnPauseButtonClick(object sender, RoutedEventArgs e)
        {
            TogglePause();
        }

        //TODO: fix the pause functionality to work with the new framerate independent code
        //Tried: I think I just need to pause the timeManager Stopwatch when pausing the game, then restart when unpausing
        //Note: When the game is paused, the user can still fire a new bullet. the player can't move since that code is in GameLoop(), but bullets still spawn
        private void TogglePause()
        {
            switch (gameTimer.IsEnabled)
            {
                case true:
                    timeManager.Stop();
                    gameTimer.Stop();
                    break;
                case false:
                    timeManager.Restart();
                    gameTimer.Start();
                    break;
                default:
                    Console.WriteLine("Cosmic bit flip...");
                    break;
            }
        }

        //Stopwatch watch = Stopwatch.StartNew();
        private void MakeEnemy()
        {
            enemySpriteCounter = rand.Next(1, 5);

            //TODO: turn enemy into a class with custom constructor for different enemy types, health, etc. 
            Rectangle newEnemy = new Rectangle
            {
                Tag = "enemy",
                Height = 50,
                Width = 56,
                Fill = enemySprites[enemySpriteCounter],
            };

            Canvas.SetLeft(newEnemy, rand.Next(30, 430));
            Canvas.SetTop(newEnemy, -100);
            GameScreen.Children.Add(newEnemy);

            /*
            watch.Stop();
            Console.WriteLine("Spawned new enemy at time=" + watch.ElapsedMilliseconds / 1000.0);
            watch.Restart();
            */
        }
    }
}
