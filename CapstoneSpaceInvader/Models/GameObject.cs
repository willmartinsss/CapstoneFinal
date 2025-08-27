using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace SpaceInvaders.Models
{
    /// <summary>
    /// Classe base para todos os objetos visuais no jogo.
    /// Contém o elemento visual (Shape) e propriedades comuns como posição e tamanho.
    /// </summary>
    public abstract class GameObject
    {
        // O elemento da UI que representa este objeto no Canvas.
        public Rectangle Shape { get; protected set; }

        // Propriedades para fácil acesso à posição e dimensões.
        public double X => Canvas.GetLeft(Shape);
        public double Y => Canvas.GetTop(Shape);
        public double Width => Shape.Width;
        public double Height => Shape.Height;

        protected GameObject(Rectangle shape)
        {
            Shape = shape;
        }

        /// <summary>
        /// Retorna o retângulo de limites (hitbox) para detecção de colisão.
        /// </summary>
        public Rect GetBounds() => new Rect(X, Y, Width, Height);

        /// <summary>
        /// Define a posição do objeto no Canvas.
        /// </summary>
        public void SetPosition(double x, double y)
        {
            Canvas.SetLeft(Shape, x);
            Canvas.SetTop(Shape, y);
        }
    }
    
}
