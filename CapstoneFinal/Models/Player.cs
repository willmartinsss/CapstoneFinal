using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace SpaceInvaders.Models
{
    public class Player : GameEntity
    {
        public int Lives { get; set; } = 3;
        public double Speed { get; set; } = 8;
        public bool CanShoot { get; set; } = true;
        
        private static ImageBrush? _playerSkin;
        
        public Player()
        {
            Width = 50;
            Height = 30;
        }
        
        public override ImageBrush GetSkin()
        {
            _playerSkin ??= new ImageBrush 
            { 
                ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/images/player.png")) 
            };
            return _playerSkin;
        }
        
        public void MoveLeft(double canvasWidth)
        {
            if (X > 0) X -= Speed;
        }
        
        public void MoveRight(double canvasWidth)
        {
            if (X + Width < canvasWidth) X += Speed;
        }
    }
}
