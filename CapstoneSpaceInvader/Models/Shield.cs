namespace CapstoneSpaceInvader.Models;
using Microsoft.UI.Xaml.Shapes;
using SpaceInvaders.Models;

/// <summary>
/// Representa um único bloco de um escudo. Herda de GameObject.
/// </summary>
public class ShieldPart : GameObject
{
    public int Health { get; set; }

    public ShieldPart(Rectangle shape, int initialHealth) : base(shape)
    {
        Health = initialHealth;
    }
}
