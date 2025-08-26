using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace SpaceInvaders.Models
{
    public class Projectile : GameEntity
    {
        public bool IsPlayerProjectile { get; set; }
        public double Speed { get; set; }
        
        public Projectile(bool isPlayerProjectile)
        {
            IsPlayerProjectile = isPlayerProjectile;
            Width = 4;
            Height = 15;
            Speed = isPlayerProjectile ? -15 : 8;
        }
        
        public override ImageBrush GetSkin()
        {
            return new ImageBrush();
        }
        
        public void Move()
        {
            Y += Speed;
        }
    }
}
