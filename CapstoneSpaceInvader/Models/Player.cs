using Microsoft.UI.Xaml.Shapes;
using SpaceInvaders.Models;

namespace CapstoneSpaceInvader.Models;

/// <summary>
/// Representa a nave do jogador. Herda de GameObject.
/// </summary>
public class Player : GameObject
{
    public int Lives { get; set; }
    public double Speed { get; }

    public Player(Rectangle shape, double speed, int initialLives) : base(shape)
    {
        Speed = speed;
        Lives = initialLives;
    }
}
