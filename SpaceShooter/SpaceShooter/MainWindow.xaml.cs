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

        public MainWindow()
        {
            InitializeComponent();

            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            MyCanvas.Focus();

            ImageBrush bg = new ImageBrush();
            bg.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/purple.png"));
            bg.TileMode = TileMode.Tile;
            bg.Viewport = new Rect(0, 0, 0.15, 0.15);
            bg.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            MyCanvas.Background = bg;

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

            if(enemyCounter < 0)
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

        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Left)
            {
                moveLeft = true;
            }
            if(e.Key == Key.Right)
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

                MyCanvas.Children.Add(newBullet);
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
            MyCanvas.Children.Add(newEnemy);
        }
    }
}
