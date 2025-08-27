using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using CapstoneSpaceInvader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;

namespace CapstoneSpaceInvader.Managers
{
    /// <summary>
    /// O maestro do jogo. Coordena todos os outros managers.
    /// Gere o estado principal do jogo (Playing, GameOver), a pontuação e o loop de atualização.
    /// </summary>
    public class GameManager
    {
        public enum GameState { Menu, Playing, WaveCleared, GameOver }
        public GameState CurrentState { get; private set; }

        private readonly PlayerManager _playerManager;
        private readonly EnemyManager _enemyManager;
        private readonly CollisionManager _collisionManager;
        private readonly EntityFactory _factory;
        private readonly AudioManager _audioManager;
        
        private readonly Canvas _gameCanvas;
        private readonly Action<int> _updateScoreUICallback;
        private readonly Action<int> _updateLivesUICallback;
        private readonly Action<GameState> _gameStateChangedCallback;

        private readonly List<ShieldPart> _shieldParts = new();

        public int Score { get; private set; }

        public GameManager(Canvas canvas, Rectangle playerShape, AudioManager audioManager, Action<int> onScoreUpdate, Action<int> onLivesUpdate, Action<GameState> onStateChange)
        {
            _gameCanvas = canvas;
            _audioManager = audioManager;
            _updateScoreUICallback = onScoreUpdate;
            _updateLivesUICallback = onLivesUpdate;
            _gameStateChangedCallback = onStateChange;

            _factory = new EntityFactory(_gameCanvas);
            var player = _factory.CreatePlayer(playerShape);
            _playerManager = new PlayerManager(_gameCanvas, _factory, _audioManager, player);
            _enemyManager = new EnemyManager(_gameCanvas, _factory, _audioManager);
            _collisionManager = new CollisionManager(_playerManager, _enemyManager, _shieldParts, _audioManager, AddScore);
        }

        public void StartNewGame()
        {
            ResetGame();
            
            _enemyManager.CreateWave();
            CreateShields();
            
            _playerManager.Player.Shape.Visibility = Visibility.Visible;
            _enemyManager.StartTimers();
            _audioManager.StartThemeMusic();
            
            SwitchGameState(GameState.Playing);
        }

        public void Update()
        {
            if (CurrentState != GameState.Playing) return;

            _playerManager.Update();
            _enemyManager.Update();
            _collisionManager.CheckAllCollisions();
            CheckGameState();
        }

        public void SetPlayerMovement(bool isMovingLeft, bool isMovingRight)
        {
            _playerManager.IsMovingLeft = isMovingLeft;
            _playerManager.IsMovingRight = isMovingRight;
        }

        public void PlayerShoot()
        {
            _playerManager.Shoot();
        }

        /// <summary>
        /// *** CORREÇÃO DO BUG DA VIDA EXTRA (PARTE 1) ***
        /// Callback que o CollisionManager chama para notificar que a pontuação deve ser aumentada.
        /// </summary>
        private void AddScore(int points)
        {
            int scoreBefore = Score;
            Score += points;
            _updateScoreUICallback(Score);

            // *** CORREÇÃO DO BUG DA VIDA EXTRA (PARTE 2) ***
            // Compara a pontuação nova e antiga para ver se cruzou um limite de 1000.
            if (Score / 1000 > scoreBefore / 1000)
            {
                if (_playerManager.Player.Lives < 6) // Usa uma constante se preferir (maxLives)
                {
                    _playerManager.Player.Lives++;
                    _updateLivesUICallback(_playerManager.Player.Lives);
                    _audioManager.PlaySoundEffect("extraShip.wav");
                }
            }
        }

        /// <summary>
        /// *** CORREÇÃO DO BUG DA PRÓXIMA ONDA (PARTE 1) ***
        /// Verifica as condições globais de vitória ou derrota.
        /// </summary>
        private async void CheckGameState()
        {
            if (_playerManager.Player.Lives <= 0) { GameOver(); return; }
            if (_enemyManager.Enemies.Any(e => (e.Y + e.Height) >= _playerManager.Player.Y)) { GameOver(); return; }

            // Verifica se a onda terminou e se o jogo está no estado 'Playing'.
            if (!_enemyManager.Enemies.Any() && CurrentState == GameState.Playing)
            {
                // Muda para um estado intermédio para esta lógica não correr em todos os frames.
                CurrentState = GameState.WaveCleared;
                
                // Espera 2 segundos para dar um respiro ao jogador.
                await Task.Delay(2000);

                // Prepara e inicia a próxima onda.
                StartNextWave();
            }

            // Atualiza a UI das vidas (caso tenha mudado)
            _updateLivesUICallback(_playerManager.Player.Lives);
        }

        /// <summary>
        /// *** CORREÇÃO DO BUG DA PRÓXIMA ONDA (PARTE 2) ***
        /// Lógica para iniciar a próxima onda de inimigos.
        /// </summary>
        private void StartNextWave()
        {
            // Aumenta a dificuldade (se necessário)
            // _enemyManager.IncreaseDifficulty(); // Idealmente, esta lógica estaria no EnemyManager

            // Limpa projéteis e cria a nova onda
            _playerManager.RemoveProjectile();
            _enemyManager.CreateWave();
            
            // Retorna o jogo ao estado normal de 'Playing'.
            SwitchGameState(GameState.Playing);
        }
        
        private void GameOver()
        {
            _enemyManager.StopTimers();
            _audioManager.StopThemeMusic();
            _audioManager.StopUfoSound();
            SwitchGameState(GameState.GameOver);
        }

        public void GoToMenu()
        {
            ResetGame();
            SwitchGameState(GameState.Menu);
        }

        private void ResetGame()
        {
            Score = 0;
            _updateScoreUICallback(Score);
            _playerManager.Reset();
            _updateLivesUICallback(_playerManager.Player.Lives);
            _enemyManager.Reset();
            foreach (var part in _shieldParts) _gameCanvas.Children.Remove(part.Shape);
            _shieldParts.Clear();
            _playerManager.Player.Shape.Visibility = Visibility.Collapsed;
        }

        private void CreateShields()
        {
            int shieldBlockSize = 8;
            int numberOfShields = 4;
            double shieldBaseY = 920 - 200;
            double totalAvailableSpace = 1080 * 0.8;
            double shieldPixelWidth = 7 * shieldBlockSize;
            double shieldSpacing = (totalAvailableSpace - (numberOfShields * shieldPixelWidth)) / (numberOfShields - 1);
            double startX = 1080 * 0.1;

            for (int i = 0; i < numberOfShields; i++)
            {
                double shieldBaseX = startX + i * (shieldPixelWidth + shieldSpacing);
                for (int row = 0; row < 5; row++)
                {
                    for (int col = 0; col < 7; col++)
                    {
                        if (row == 4 && (col > 1 && col < 5)) continue;
                        var part = _factory.CreateShieldPart(shieldBaseX + col * shieldBlockSize, shieldBaseY + row * shieldBlockSize, shieldBlockSize, new SolidColorBrush(Colors.LightGreen));
                        _shieldParts.Add(part);
                    }
                }
            }
        }

        private void SwitchGameState(GameState newState)
        {
            // Apenas altera o estado se for diferente, exceto para 'Playing' que pode ser redefinido
            if (CurrentState != newState || newState == GameState.Playing)
            {
                CurrentState = newState;
                _gameStateChangedCallback(newState);
            }
        }
    }
}
