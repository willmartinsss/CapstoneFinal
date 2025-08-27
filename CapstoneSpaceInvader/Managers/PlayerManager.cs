using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SpaceInvaders.Models;
using System.Threading.Tasks;
using CapstoneSpaceInvader.Managers;
using SpaceInvaders.Managers;

namespace CapstoneSpaceInvader.Managers{
    /// <summary>
    /// Gere todos os aspetos do jogador: movimento, disparo, vidas e estado.
    /// </summary>
    public class PlayerManager
    {
        private readonly Canvas _gameCanvas;
        private readonly EntityFactory _factory;
        private readonly AudioManager _audioManager;
        
        public Player Player { get; private set; }
        public Projectile? PlayerProjectile { get; private set; }

        // Controlo de Input
        public bool IsMovingLeft { get; set; }
        public bool IsMovingRight { get; set; }
        private bool _canShoot = true;

        public PlayerManager(Canvas gameCanvas, EntityFactory factory, AudioManager audioManager, Player player)
        {
            _gameCanvas = gameCanvas;
            _factory = factory;
            _audioManager = audioManager;
            Player = player;
        }

        public void Update()
        {
            MovePlayer();
            MovePlayerProjectile();
        }

        public void Reset()
        {
            Player.Lives = 3;
            Player.SetPosition((_gameCanvas.ActualWidth - Player.Width) / 2, _gameCanvas.ActualHeight - Player.Height - 40);
            Player.Shape.Visibility = Visibility.Visible;
            RemoveProjectile();
        }

        public void Shoot()
        {
            if (!_canShoot) return;

            _audioManager.PlaySoundEffect("shoot.wav");
            _canShoot = false;
            PlayerProjectile = _factory.CreateProjectile(Player.X + Player.Width / 2 - 2, Player.Y - 15, -15, new SolidColorBrush(Colors.White));
        }

        public async void PlayerHit()
        {
            _audioManager.PlaySoundEffect("explosion.wav");
            Player.Lives--;

            if (Player.Lives > 0)
            {
                // Efeito de invencibilidade temporária
                Player.Shape.Opacity = 0.5;
                await Task.Delay(1500);
                Player.Shape.Opacity = 1.0;
            }
        }

        public void RemoveProjectile()
        {
            if (PlayerProjectile == null) return;
            _gameCanvas.Children.Remove(PlayerProjectile.Shape);
            PlayerProjectile = null;
            _canShoot = true;
        }

        private void MovePlayer()
        {
            double nextX = Player.X;
            if (IsMovingLeft && Player.X > 0)
            {
                nextX -= Player.Speed;
            }
            if (IsMovingRight && (Player.X + Player.Width < _gameCanvas.ActualWidth))
            {
                nextX += Player.Speed;
            }
            Player.SetPosition(nextX, Player.Y);
        }

        private void MovePlayerProjectile()
        {
            if (PlayerProjectile == null) return;

            PlayerProjectile.SetPosition(PlayerProjectile.X, PlayerProjectile.Y + PlayerProjectile.SpeedY);

            if (PlayerProjectile.Y < 0)
            {
                RemoveProjectile();
            }
        }
    }
}
