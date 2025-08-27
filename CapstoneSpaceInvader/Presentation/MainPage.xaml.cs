using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using SpaceInvaders.Managers;
using System;
using System.Collections.Generic;
using Windows.System;
using CapstoneSpaceInvader.Managers;
using static CapstoneSpaceInvader.Managers.GameManager;

namespace CapstoneSpaceInvader
{
    public sealed partial class MainPage : Page
    {
        private readonly GameManager _gameManager;
        private readonly AudioManager _audioManager;
        private readonly ScoreManager _scoreManager;
        private readonly DispatcherTimer _gameLoopTimer;

        private bool _isMovingLeft, _isMovingRight;

        public MainPage()
        {
            this.InitializeComponent();

            _audioManager = new AudioManager();
            _scoreManager = new ScoreManager();
            _gameManager = new GameManager(GameCanvas, PlayerShip, _audioManager, UpdateScoreUI, UpdateLivesUI, OnGameStateChanged);

            _gameLoopTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _gameLoopTimer.Tick += GameLoop;

            this.Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(100); 

            RootGrid.Focus(FocusState.Programmatic);
            ShowMenu();
        }

        private void GameLoop(object? sender, object e)
        {
            _gameManager.Update();
        }

        #region Callbacks da UI

        private void UpdateScoreUI(int score) => ScoreText.Text = $"PONTOS: {score}";

        private void UpdateLivesUI(int lives)
        {
            LivesPanel.Children.Clear();
            var playerSkin = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/images/player.png")) };
            for (int i = 0; i < lives; i++)
            {
                LivesPanel.Children.Add(new Rectangle { Width = 30, Height = 20, Fill = playerSkin });
            }
        }

        private void OnGameStateChanged(GameManager.GameState newState)
        {
            MenuPanel.Visibility = Visibility.Collapsed;
            GameUIPanel.Visibility = Visibility.Collapsed;
            GameOverPanel.Visibility = Visibility.Collapsed;
            HighScoresPanel.Visibility = Visibility.Collapsed;
            ControlsPanel.Visibility = Visibility.Collapsed;
            _gameLoopTimer.Stop();

            switch (newState)
            {
                case GameManager.GameState.Menu:
                    MenuPanel.Visibility = Visibility.Visible;
                    break;
                case GameManager.GameState.Playing:
                    GameUIPanel.Visibility = Visibility.Visible;
                    _gameLoopTimer.Start();
                    break;
                case GameManager.GameState.GameOver:
                    FinalScoreText.Text = $"SUA PONTUAÇÃO: {_gameManager.Score}";
                    NicknameInput.Text = "";
                    GameOverPanel.Visibility = Visibility.Visible;
                    break;
            }
        }

        #endregion

        #region Eventos de Input

        private void StartButton_Click(object sender, RoutedEventArgs e) => _gameManager.StartNewGame();
        private void HighScoresButton_Click(object sender, RoutedEventArgs e) => ShowHighScores();
        private void ControlsButton_Click(object sender, RoutedEventArgs e) => ShowControls();
        private void BackToMenuButton_Click(object sender, RoutedEventArgs e) => ShowMenu();
        
        private async void SaveScoreButton_Click(object sender, RoutedEventArgs e)
        {
            await _scoreManager.SaveScoreAsync(NicknameInput.Text, _gameManager.Score);
            ShowHighScores();
        }

        // --- Teclado ---

        /// <summary>
        /// Chamado sempre que uma tecla é pressionada.
        /// </summary>
        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Só processa o input se o jogo estiver a decorrer
            if (_gameManager.CurrentState != GameState.Playing) return;

            // Verifica qual tecla foi pressionada
            switch (e.Key)
            {
                case VirtualKey.Left:
                case VirtualKey.A:
                    _isMovingLeft = true;
                    break;

                case VirtualKey.Right:
                case VirtualKey.D:
                    _isMovingRight = true;
                    break;

                case VirtualKey.Space:
                    // Avisa o GameManager para disparar
                    _gameManager.PlayerShoot();
                    break;
            }

            // Avisa o GameManager sobre o estado atual do movimento
            _gameManager.SetPlayerMovement(_isMovingLeft, _isMovingRight);
        }

        /// <summary>
        /// Chamado sempre que uma tecla é solta.
        /// </summary>
        private void Page_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // Verifica qual tecla foi solta
            switch (e.Key)
            {
                case VirtualKey.Left:
                case VirtualKey.A:
                    _isMovingLeft = false;
                    break;

                case VirtualKey.Right:
                case VirtualKey.D:
                    _isMovingRight = false;
                    break;
            }

            // Avisa o GameManager sobre o novo estado do movimento
            _gameManager.SetPlayerMovement(_isMovingLeft, _isMovingRight);
        }
        #endregion

        #region Gestão de Painéis

        private void ShowMenu() => _gameManager.GoToMenu();

        private async void ShowHighScores()
        {
            MenuPanel.Visibility = Visibility.Collapsed;
            GameOverPanel.Visibility = Visibility.Collapsed;
            List<string> scores = await _scoreManager.LoadScoresAsync();
            HighScoresListView.ItemsSource = scores;
            HighScoresPanel.Visibility = Visibility.Visible;
        }

        private void ShowControls()
        {
            MenuPanel.Visibility = Visibility.Collapsed;
            ControlsPanel.Visibility = Visibility.Visible;
        }

        #endregion
    }
}
