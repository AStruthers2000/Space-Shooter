using System;
using System.Collections.Generic;
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
        int enemyCounter = 100;
        int enemySpeed = 10;

        int playerSpeed = 10;

        int limit = 50;
        int score = 0;
        int damage = 0;

        int edgePadding = 10;

        Rect playerHitBox;

        int targetFPS = 60;

        public MainWindow()
        {
            InitializeComponent();

            //frequency = 1/period -> period = 1/frequency
            gameTimer.Interval = TimeSpan.FromSeconds(1.0 / targetFPS);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            GameScreen.Focus();

            ImageBrush bg = new ImageBrush();
            bg.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/purple.png"));
            bg.TileMode = TileMode.Tile;
            bg.Viewport = new Rect(0, 0, 0.15, 0.15);
            bg.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            GameScreen.Background = bg;

            ImageBrush playerImage = new ImageBrush();
            playerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/player.png"));
            Player.Fill = playerImage;
        }

        private void GameLoop(object sender, EventArgs e)
        {
            playerHitBox = new Rect(Canvas.GetLeft(Player), Canvas.GetTop(Player), Player.Width, Player.Height);

            enemyCounter -= 1;

            lbl_ScoreText.Content = "Score: " + score;
            lbl_DamageText.Content = "Damage: " + damage;

            if (enemyCounter < 0)
            {
                MakeEnemy();
                enemyCounter = limit;
            }

            if (moveLeft && playerHitBox.X > 0 + edgePadding)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) - playerSpeed);
            }

            //for some reason, frame is always 17 pixels wider than specified
            if (moveRight && playerHitBox.X + playerHitBox.Width < Application.Current.MainWindow.Width - 17 - edgePadding)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) + playerSpeed);
            }

            foreach (Rectangle r in GameScreen.Children.OfType<Rectangle>())
            {
                if ((string)r.Tag == "bullet")
                {
                    Canvas.SetTop(r, Canvas.GetTop(r) - 20);

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
                            }
                        }
                    }
                }

                if ((string)r.Tag == "enemy")
                {
                    Canvas.SetTop(r, Canvas.GetTop(r) + enemySpeed);

                    //if the enemy is at twice the height of the screen, remove them
                    if (Canvas.GetTop(r) > Application.Current.MainWindow.Height + edgePadding)
                    {
                        itemRemover.Add(r);

                        //if you don't shoot the enemy, and it makes it to your side, you take damage
                        damage += 10;
                    }

                    Rect enemyHitBox = new Rect(Canvas.GetLeft(r), Canvas.GetTop(r), r.Width, r.Height);

                    if (playerHitBox.IntersectsWith(enemyHitBox))
                    {
                        itemRemover.Add(r);
                        damage += 5;
                    }
                }
            }

            foreach (Rectangle r in itemRemover)
            {
                GameScreen.Children.Remove(r);
            }
            itemRemover.Clear();
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

        private void TogglePause()
        {
            switch (gameTimer.IsEnabled)
            {
                case true:
                    gameTimer.Stop();
                    break;
                case false:
                    gameTimer.Start();
                    break;
                default:
                    Console.WriteLine("Cosmic bit flip...");
                    break;
            }
        }

        private void MakeEnemy()
        {
            ImageBrush enemySprite = new ImageBrush();
            enemySpriteCounter = rand.Next(1, 5);

            
            string uri = string.Format("pack://application:,,,/Assets/{0}.png", enemySpriteCounter);
            enemySprite.ImageSource = new BitmapImage(new Uri(uri));

            Rectangle newEnemy = new Rectangle
            {
                Tag = "enemy",
                Height = 50,
                Width = 56,
                Fill = enemySprite
            };

            Canvas.SetLeft(newEnemy, rand.Next(30, 430));
            Canvas.SetTop(newEnemy, -100);
            GameScreen.Children.Add(newEnemy);
        }
    }
}
