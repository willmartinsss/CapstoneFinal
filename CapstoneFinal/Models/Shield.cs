using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace SpaceInvaders.Models
{
    public class ShieldPart : GameEntity
    {
        public int Health { get; set; } = 3;
        
        public ShieldPart()
        {
            Width = 8;
            Height = 8;
        }
        
        public override ImageBrush GetSkin()
        {
            return new ImageBrush();
        }
        
        public SolidColorBrush GetHealthColor()
        {
            return Health switch
            {
                3 => new SolidColorBrush(Colors.LightGreen),
                2 => new SolidColorBrush(Colors.YellowGreen),
                1 => new SolidColorBrush(Colors.Orange),
                _ => new SolidColorBrush(Colors.Red)
            };
        }
        
        public void TakeDamage()
        {
            Health--;
            if (Health <= 0) IsActive = false;
        }
    }
}
