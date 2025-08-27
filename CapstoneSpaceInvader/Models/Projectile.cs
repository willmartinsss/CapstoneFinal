namespace CapstoneSpaceInvader.Models;
using Microsoft.UI.Xaml.Shapes;
using SpaceInvaders.Models;

/// <summary>
/// Representa um projétil (do jogador ou inimigo). Herda de GameObject.
/// </summary>
public class Projectile : GameObject
{
    // A velocidade vertical do projétil. Negativa para subir, positiva para descer.
    public double SpeedY { get; }

    public Projectile(Rectangle shape, double speedY) : base(shape)
    {
        SpeedY = speedY;
    }
}
