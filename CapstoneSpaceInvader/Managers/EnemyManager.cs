using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SpaceInvaders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using SpaceInvaders.Managers;

namespace CapstoneSpaceInvader.Managers
{
    /// <summary>
    /// Gere todos os aspetos dos inimigos: criação da onda, movimento, disparos e o OVNI.
    /// </summary>
    public class EnemyManager
    {
        private readonly Canvas _gameCanvas;
        private readonly EntityFactory _factory;
        private readonly AudioManager _audioManager;

        public List<Enemy> Enemies { get; private set; } = new();
        public List<Projectile> EnemyProjectiles { get; private set; } = new();
        public Enemy? SpecialEnemy { get; private set; }

        // Parâmetros de movimento e dificuldade
        private double _enemySpeed = 1.5;
        private int _enemyDirection = 1;
        private double _enemyFireRate = 1000;
        private double _specialEnemySpeed = 3;
        private int _waveNumber = 1;

        // Timers
        private DispatcherTimer? _specialEnemyTimer;
        private DispatcherTimer? _enemyFireTimer;

        public EnemyManager(Canvas gameCanvas, EntityFactory factory, AudioManager audioManager)
        {
            _gameCanvas = gameCanvas;
            _factory = factory;
            _audioManager = audioManager;
        }

        public void CreateWave()
        {
            int enemyRows = 5;
            int enemyCols = 11;
            double enemyWidth = 40;
            double enemyHeight = 30;
            double enemySpacing = 15;
            double startY = 70 + ((_waveNumber - 1) * 20);

            double gridWidth = (enemyCols * enemyWidth) + ((enemyCols - 1) * enemySpacing);
            double startX = (_gameCanvas.ActualWidth - gridWidth) / 2;

            for (int row = 0; row < enemyRows; row++)
            {
                for (int col = 0; col < enemyCols; col++)
                {
                    var enemy = _factory.CreateEnemy(row, col);
                    enemy.SetPosition(startX + col * (enemyWidth + enemySpacing), startY + row * (enemyHeight + enemySpacing));
                    Enemies.Add(enemy);
                }
            }
        }

        public void Update()
        {
            MoveEnemies();
            MoveEnemyProjectiles();
            MoveSpecialEnemy();
        }

        public void Reset()
        {
            // Limpa todos os inimigos e projéteis
            foreach (var enemy in Enemies) _gameCanvas.Children.Remove(enemy.Shape);
            Enemies.Clear();
            
            foreach (var projectile in EnemyProjectiles) _gameCanvas.Children.Remove(projectile.Shape);
            EnemyProjectiles.Clear();

            RemoveSpecialEnemy();
            StopTimers();

            // Reinicia os parâmetros de dificuldade
            _waveNumber = 1;
            _enemySpeed = 1.5;
            _enemyDirection = 1;
            _enemyFireRate = 1000;
        }

        public void StartTimers()
        {
            _specialEnemyTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(20) };
            _specialEnemyTimer.Tick += (s, e) => CreateSpecialEnemy();
            _specialEnemyTimer.Start();

            _enemyFireTimer = new DispatcherTimer();
            UpdateEnemyFireTimer();
            _enemyFireTimer.Tick += EnemyShoot;
            _enemyFireTimer.Start();
        }

        public void StopTimers()
        {
            _specialEnemyTimer?.Stop();
            _enemyFireTimer?.Stop();
        }

        public void RemoveEnemy(Enemy enemy)
        {
            _gameCanvas.Children.Remove(enemy.Shape);
            Enemies.Remove(enemy);
        }

        public void RemoveSpecialEnemy()
        {
            if (SpecialEnemy == null) return;
            _gameCanvas.Children.Remove(SpecialEnemy.Shape);
            SpecialEnemy = null;
            _audioManager.StopUfoSound();
        }
        
        public void RemoveProjectile(Projectile projectile)
        {
            _gameCanvas.Children.Remove(projectile.Shape);
            EnemyProjectiles.Remove(projectile);
        }

        private void MoveEnemies()
        {
            if (!Enemies.Any()) return;

            bool edgeReached = false;
            foreach (var enemy in Enemies)
            {
                enemy.SetPosition(enemy.X + (_enemySpeed * _enemyDirection), enemy.Y);
                if ((enemy.X <= 0 && _enemyDirection == -1) || (enemy.X + enemy.Width >= _gameCanvas.ActualWidth && _enemyDirection == 1))
                {
                    edgeReached = true;
                }
            }

            if (edgeReached)
            {
                _enemyDirection *= -1;
                _enemySpeed += 0.1;
                if (_enemyFireRate > 300) _enemyFireRate -= 50;
                UpdateEnemyFireTimer();

                foreach (var enemy in Enemies)
                {
                    enemy.SetPosition(enemy.X, enemy.Y + 20);
                }
            }
        }

        private void MoveEnemyProjectiles()
        {
            for (int i = EnemyProjectiles.Count - 1; i >= 0; i--)
            {
                var projectile = EnemyProjectiles[i];
                projectile.SetPosition(projectile.X, projectile.Y + projectile.SpeedY);

                if (projectile.Y > _gameCanvas.ActualHeight)
                {
                    RemoveProjectile(projectile);
                }
            }
        }
        
        private void MoveSpecialEnemy()
        {
            if (SpecialEnemy == null) return;
            SpecialEnemy.SetPosition(SpecialEnemy.X + _specialEnemySpeed, SpecialEnemy.Y);
            if (SpecialEnemy.X > _gameCanvas.ActualWidth)
            {
                RemoveSpecialEnemy();
            }
        }

        private void CreateSpecialEnemy()
        {
            if (SpecialEnemy != null) return;
            _audioManager.StartUfoSound();
            SpecialEnemy = _factory.CreateSpecialEnemy();
        }
        
        private void EnemyShoot(object? sender, object e)
        {
            if (!Enemies.Any()) return;

            var shooters = Enemies
                .GroupBy(enemy => enemy.X)
                .Select(group => group.OrderByDescending(enemy => enemy.Y).First())
                .ToList();

            if (!shooters.Any()) return;

            var shooter = shooters[new Random().Next(shooters.Count)];
            var projectile = _factory.CreateProjectile(shooter.X + shooter.Width / 2 - 2, shooter.Y + shooter.Height, 8, new SolidColorBrush(Colors.LightGreen));
            EnemyProjectiles.Add(projectile);
        }

        private void UpdateEnemyFireTimer()
        {
            if (_enemyFireTimer != null)
            {
                _enemyFireTimer.Interval = TimeSpan.FromMilliseconds(_enemyFireRate);
            }
        }
    }
}
