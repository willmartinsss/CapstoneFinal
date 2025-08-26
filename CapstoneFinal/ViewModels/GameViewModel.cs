using Microsoft.UI.Xaml;
using SpaceInvaders.Models;
using SpaceInvaders.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace SpaceInvaders.ViewModels
{
    public partial class GameViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly AudioService _audioService;
        private readonly GameLogicService _gameLogicService;
        private readonly ScoreService _scoreService;
        private readonly ILogger<GameViewModel> _logger;
        
        private DispatcherTimer? _gameLoopTimer;
        private DispatcherTimer? _enemyFireTimer;
        private DispatcherTimer? _specialEnemyTimer;
        
        private GameState _gameState;
        private string _displayScore = "PONTOS: 0";
        private string _playerLives = "3";
        private double _canvasWidth = 800;
        private double _canvasHeight = 900;
        
        public GameState GameState => _gameState;
        public string DisplayScore 
        { 
            get => _displayScore; 
            private set { _displayScore = value; OnPropertyChanged(); }
        }
        public string PlayerLives 
        { 
            get => _playerLives; 
            private set { _playerLives = value; OnPropertyChanged(); }
        }
        
        public ICommand StartGameCommand { get; }
        public ICommand ShowMenuCommand { get; }
        public ICommand ShowHighScoresCommand { get; }
        public ICommand ShowControlsCommand { get; }
        public ICommand SaveScoreCommand { get; }
        
        // Constructor with dependency injection
        public GameViewModel(
            AudioService audioService, 
            GameLogicService gameLogicService, 
            ScoreService scoreService,
            ILogger<GameViewModel> logger)
        {
            _audioService = audioService;
            _gameLogicService = gameLogicService;
            _scoreService = scoreService;
            _logger = logger;
            _gameState = new GameState();
            
            StartGameCommand = new RelayCommand(StartNewGame);
            ShowMenuCommand = new RelayCommand(() => SwitchGameState(GameStateType.Menu));
            ShowHighScoresCommand = new RelayCommand(() => SwitchGameState(GameStateType.HighScores));
            ShowControlsCommand = new RelayCommand(() => SwitchGameState(GameStateType.Controls));
            SaveScoreCommand = new RelayCommand<string>(async nickname => await SaveScoreAsync(nickname));
        }
        
        public void SetCanvasSize(double width, double height)
        {
            _canvasWidth = width;
            _canvasHeight = height;
        }
        
        public void StartNewGame()
        {
            try
            {
                _gameState.Reset();
                _gameState.Player.X = _canvasWidth / 2 - _gameState.Player.Width / 2;
                _gameState.Player.Y = _canvasHeight - _gameState.Player.Height - 40;
                
                _gameLogicService.CreateEnemies(_gameState, _canvasWidth);
                _gameLogicService.CreateShields(_gameState, _canvasWidth);
                
                UpdateDisplays();
                SetupTimers();
                _audioService.StartThemeMusic();
                SwitchGameState(GameStateType.Playing);
                
                _logger.LogInformation("New game started");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting new game");
            }
        }
        
        private void SetupTimers()
        {
            _gameLoopTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _gameLoopTimer.Tick += GameLoop;
            _gameLoopTimer.Start();
            
            _enemyFireTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_gameState.EnemyFireRate) };
            _enemyFireTimer.Tick += EnemyFire;
            _enemyFireTimer.Start();
            
            _specialEnemyTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(20) };
            _specialEnemyTimer.Tick += CreateSpecialEnemy;
            _specialEnemyTimer.Start();
        }
        
        private void GameLoop(object? sender, object e)
        {
            if (_gameState.CurrentState != GameStateType.Playing) return;
            
            try
            {
                // Move projectiles
                for (int i = _gameState.Projectiles.Count - 1; i >= 0; i--)
                {
                    var projectile = _gameState.Projectiles[i];
                    projectile.Move();
                    
                    if (projectile.Y < 0 || projectile.Y > _canvasHeight)
                    {
                        _gameState.Projectiles.RemoveAt(i);
                    }
                }
                
                // Move enemies
                _gameLogicService.MoveEnemies(_gameState, _canvasWidth);
                
                // Move special enemy
                if (_gameState.SpecialEnemy != null)
                {
                    _gameState.SpecialEnemy.X += 3;
                    if (_gameState.SpecialEnemy.X > _canvasWidth + 50)
                    {
                        _gameState.SpecialEnemy = null;
                        _audioService.StopUfoSound();
                        OnPropertyChanged(nameof(GameState));
                    }
                }
                
                CheckCollisions();
                CheckGameState();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in game loop");
            }
        }
        
        private void EnemyFire(object? sender, object e)
        {
            var shooter = _gameLogicService.GetRandomShooter(_gameState);
            if (shooter != null)
            {
                var projectile = new Projectile(false)
                {
                    X = shooter.X + shooter.Width / 2 - 2,
                    Y = shooter.Y + shooter.Height
                };
                _gameState.Projectiles.Add(projectile);
            }
        }
        
        private async void CreateSpecialEnemy(object? sender, object e)
        {
            if (_gameState.SpecialEnemy != null) return;
            
            _gameState.SpecialEnemy = new Enemy(EnemyType.Special)
            {
                X = -50,
                Y = 40
            };
            OnPropertyChanged(nameof(GameState));
            await _audioService.StartUfoSoundAsync();
        }
        
        public void MovePlayer(bool left, bool right)
        {
            if (_gameState.CurrentState != GameStateType.Playing) return;
            
            if (left) _gameState.Player.MoveLeft(_canvasWidth);
            if (right) _gameState.Player.MoveRight(_canvasWidth);
            OnPropertyChanged(nameof(GameState));
        }
        
        public async Task ShootAsync()
        {
            if (_gameState.CurrentState != GameStateType.Playing || !_gameState.Player.CanShoot) return;
            
            await _audioService.PlaySoundAsync("shoot.wav");
            _gameState.Player.CanShoot = false;
            
            var projectile = new Projectile(true)
            {
                X = _gameState.Player.X + _gameState.Player.Width / 2 - 2,
                Y = _gameState.Player.Y - 15
            };
            _gameState.Projectiles.Add(projectile);
            
            // Re-enable shooting after brief delay
            await Task.Delay(100);
            _gameState.Player.CanShoot = true;
        }
        
        private void CheckCollisions()
        {
            // Player projectiles vs enemies and special enemy
            for (int i = _gameState.Projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = _gameState.Projectiles[i];
                if (!projectile.IsPlayerProjectile) continue;

                // Check collision with regular enemies
                for (int j = _gameState.Enemies.Count - 1; j >= 0; j--)
                {
                    var enemy = _gameState.Enemies[j];
                    if (projectile.CollidesWith(enemy))
                    {
                        _ = _audioService.PlaySoundAsync("invaderkilled.wav");
                        _gameState.Score += enemy.Points;
                        _gameState.Enemies.RemoveAt(j);
                        _gameState.Projectiles.RemoveAt(i);
                        UpdateDisplays();
                        return;
                    }
                }

                // Check collision with special enemy
                if (_gameState.SpecialEnemy != null && projectile.CollidesWith(_gameState.SpecialEnemy))
                {
                    _ = _audioService.PlaySoundAsync("invaderkilled.wav");
                    _gameState.Score += _gameState.SpecialEnemy.Points;
                    _gameState.SpecialEnemy = null;
                    _gameState.Projectiles.RemoveAt(i);
                    _audioService.StopUfoSound();
                    UpdateDisplays();
                    OnPropertyChanged(nameof(GameState));
                    return;
                }

                // Check collision with shields
                CheckProjectileShieldCollision(projectile, i, true);
            }

            // Enemy projectiles vs player and shields
            for (int i = _gameState.Projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = _gameState.Projectiles[i];
                if (projectile.IsPlayerProjectile) continue;

                // Check collision with player
                if (projectile.CollidesWith(_gameState.Player))
                {
                    _ = _audioService.PlaySoundAsync("explosion.wav");
                    _gameState.Player.Lives--;
                    _gameState.Projectiles.RemoveAt(i);
                    UpdateDisplays();
                    
                    if (_gameState.Player.Lives <= 0)
                    {
                        GameOver();
                    }
                    return;
                }

                // Check collision with shields
                CheckProjectileShieldCollision(projectile, i, false);
            }
        }
        
        private void CheckProjectileShieldCollision(Projectile projectile, int projectileIndex, bool isPlayerProjectile)
        {
            for (int k = _gameState.ShieldParts.Count - 1; k >= 0; k--)
            {
                var shield = _gameState.ShieldParts[k];
                if (!shield.IsActive) continue;
                
                if (projectile.CollidesWith(shield))
                {
                    shield.TakeDamage();
                    if (!shield.IsActive)
                    {
                        _gameState.ShieldParts.RemoveAt(k);
                    }
                    _gameState.Projectiles.RemoveAt(projectileIndex);
                    return;
                }
            }
        }
        
        private void CheckGameState()
        {
            if (!_gameState.Enemies.Any())
            {
                _gameState.WaveNumber++;
                _gameLogicService.CreateEnemies(_gameState, _canvasWidth);
            }
        }
        
        private void UpdateDisplays()
        {
            DisplayScore = $"PONTOS: {_gameState.Score}";
            PlayerLives = _gameState.Player.Lives.ToString();
        }
        
        private void SwitchGameState(GameStateType newState)
        {
            _gameState.CurrentState = newState;
            OnPropertyChanged(nameof(GameState));
        }
        
        private async Task SaveScoreAsync(string? nickname)
        {
            if (string.IsNullOrEmpty(nickname)) nickname = "JOGADOR";
            await _scoreService.SaveScoreAsync(nickname, _gameState.Score);
            SwitchGameState(GameStateType.HighScores);
        }
        
        private void GameOver()
        {
            StopTimers();
            _audioService.StopThemeMusic();
            _audioService.StopUfoSound();
            SwitchGameState(GameStateType.GameOver);
        }
        
        private void StopTimers()
        {
            _gameLoopTimer?.Stop();
            _enemyFireTimer?.Stop();
            _specialEnemyTimer?.Stop();
        }
        
        public void Dispose()
        {
            StopTimers();
            _audioService.Dispose();
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
