using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using SpaceInvaders.Models;
using System;

namespace CapstoneSpaceInvader.Managers
{
    /// <summary>
    /// Responsável por criar (instanciar e configurar) todos os GameObjects do jogo.
    /// Centraliza a configuração visual e de dados iniciais das entidades.
    /// </summary>
    public class EntityFactory
    {
        private readonly Canvas _gameCanvas;
        private readonly ImageBrush _playerSkin, _alienSkin10, _alienSkin20, _alienSkin40, _specialAlienSkin;

        public EntityFactory(Canvas gameCanvas)
        {
            _gameCanvas = gameCanvas;

            // Carrega todas as skins uma única vez
            _alienSkin40 = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/images/alien3.png")) };
            _alienSkin20 = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/images/alien2.png")) };
            _alienSkin10 = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/images/alien1.png")) };
            _specialAlienSkin = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/images/alienespecial.png")) };
            _playerSkin = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/images/player.png")) };
        }

        public Player CreatePlayer(Rectangle playerShape)
        {
            playerShape.Fill = _playerSkin;
            return new Player(playerShape, 8, 3);
        }

        public Enemy CreateEnemy(int row, int col)
        {
            var shape = new Rectangle { Width = 40, Height = 30 };
            ImageBrush skin;
            int points;

            if (row == 0) { skin = _alienSkin40; points = 40; }
            else if (row < 3) { skin = _alienSkin20; points = 20; }
            else { skin = _alienSkin10; points = 10; }

            shape.Fill = skin;
            var enemy = new Enemy(shape, points);
            _gameCanvas.Children.Add(shape);
            return enemy;
        }
        
        public Enemy CreateSpecialEnemy()
        {
            var shape = new Rectangle { Width = 50, Height = 22, Fill = _specialAlienSkin };
            int points = new Random().Next(5, 16) * 10;
            var specialEnemy = new Enemy(shape, points);
            specialEnemy.SetPosition(-specialEnemy.Width, 40);
            _gameCanvas.Children.Add(shape);
            return specialEnemy;
        }

        public Projectile CreateProjectile(double x, double y, double speed, Brush color)
        {
            var shape = new Rectangle { Width = 4, Height = 15, Fill = color };
            var projectile = new Projectile(shape, speed);
            projectile.SetPosition(x, y);
            _gameCanvas.Children.Add(shape);
            return projectile;
        }

        public ShieldPart CreateShieldPart(double x, double y, double size, Brush color)
        {
            var shape = new Rectangle { Width = size, Height = size, Fill = color };
            var part = new ShieldPart(shape, 3);
            part.SetPosition(x, y);
            _gameCanvas.Children.Add(shape);
            return part;
        }
    }
}
