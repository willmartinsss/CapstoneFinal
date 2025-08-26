using Microsoft.UI.Xaml.Media;
using System;

namespace SpaceInvaders.Models
{
    public abstract class GameEntity
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsActive { get; set; } = true;
        
        public abstract ImageBrush GetSkin();
        
        public bool CollidesWith(GameEntity other)
        {
            return X < other.X + other.Width &&
                   X + Width > other.X &&
                   Y < other.Y + other.Height &&
                   Y + Height > other.Y;
        }
    }
}
