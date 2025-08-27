namespace CapstoneSpaceInvader.Models;
using Microsoft.UI.Xaml.Shapes;
using SpaceInvaders.Models;

/// <summary>
/// Representa um inimigo (alien). Herda de GameObject.
/// </summary>
public class Enemy : GameObject
{
    public int PointValue { get; }

    public Enemy(Rectangle shape, int points) : base(shape)
    {
        PointValue = points;
    }
}
