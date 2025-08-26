using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace SpaceInvaders.Models
{
    public enum EnemyType { Type10, Type20, Type40, Special }
    
    public class Enemy : GameEntity
    {
        public EnemyType Type { get; set; }
        public int Points { get; set; }
        
        private static ImageBrush? _skin10, _skin20, _skin40, _specialSkin;
        
        public Enemy(EnemyType type)
        {
            Type = type;
            Width = type == EnemyType.Special ? 50 : 40;
            Height = type == EnemyType.Special ? 22 : 30;
            
            Points = type switch
            {
                EnemyType.Type10 => 10,
                EnemyType.Type20 => 20,
                EnemyType.Type40 => 40,
                EnemyType.Special => new Random().Next(5, 16) * 10,
                _ => 10
            };
        }
        
        public override ImageBrush GetSkin()
        {
            return Type switch
            {
                EnemyType.Type10 => _skin10 ??= new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/images/alien1.png")) },
                EnemyType.Type20 => _skin20 ??= new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/images/alien2.png")) },
                EnemyType.Type40 => _skin40 ??= new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/images/alien3.png")) },
                EnemyType.Special => _specialSkin ??= new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/images/alienespecial.png")) },
                _ => _skin10 ??= new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/images/alien1.png")) }
            };
        }
    }
}
