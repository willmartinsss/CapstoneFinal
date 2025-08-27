using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using SpaceInvaders.ViewModels;
using SpaceInvaders.Models;
using System.ComponentModel;

namespace SpaceInvaders.Views
{
    public sealed partial class MainPage : Page
    {
        private bool _isMovingLeft = false;
        private bool _isMovingRight = false;
        
        public GameViewModel? Vm { get; private set; }
        
        public MainPage()
        {
            this.InitializeComponent();
        }
        
        public void SetViewModel(GameViewModel viewModel)
        {
            Vm = viewModel;
            this.DataContext = Vm; 
            // Assina o evento para saber quando o ViewModel muda
            Vm.PropertyChanged += ViewModel_PropertyChanged;
            this.Loaded += OnPageLoaded;
        }
        
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if (Vm != null)
            {
                Vm.SetCanvasSize(this.ActualWidth, this.ActualHeight);
                // Define a visibilidade inicial da UI
                UpdateUIVisibility(Vm.GameState.CurrentState);
            }
            this.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Chamado sempre que uma propriedade no GameViewModel muda.
        /// </summary>
        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Se a propriedade que mudou foi o estado do jogo...
            if (e.PropertyName == nameof(Vm.GameState))
            {
                // ...chama nosso método para atualizar a UI na thread correta.
                DispatcherQueue.TryEnqueue(() => UpdateUIVisibility(Vm!.GameState.CurrentState));
            }
        }

        /// <summary>
        /// A nova lógica central para controlar qual tela é exibida.
        /// </summary>
        private void UpdateUIVisibility(GameStateType state)
        {
            // 1. Esconde todas as telas
            MenuScreen.Visibility = Visibility.Collapsed;
            GameCanvasScreen.Visibility = Visibility.Collapsed;
            GameOverScreen.Visibility = Visibility.Collapsed;
            HighScoresScreen.Visibility = Visibility.Collapsed;
            ControlsScreen.Visibility = Visibility.Collapsed;

            // 2. Mostra apenas a tela correta com base no estado
            switch (state)
            {
                case GameStateType.Menu:
                    MenuScreen.Visibility = Visibility.Visible;
                    break;
                case GameStateType.Playing:
                    GameCanvasScreen.Visibility = Visibility.Visible;
                    break;
                case GameStateType.GameOver:
                    GameOverScreen.Visibility = Visibility.Visible;
                    break;
                case GameStateType.HighScores:
                    HighScoresScreen.Visibility = Visibility.Visible;
                    break;
                case GameStateType.Controls:
                    ControlsScreen.Visibility = Visibility.Visible;
                    break;
            }
        }
        
        private async void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (Vm == null || Vm.GameState.CurrentState != GameStateType.Playing) return;

            switch (e.Key)
            {
                case VirtualKey.Left: case VirtualKey.A: _isMovingLeft = true; break;
                case VirtualKey.Right: case VirtualKey.D: _isMovingRight = true; break;
                case VirtualKey.Space: await Vm.ShootAsync(); break;
            }
            Vm.MovePlayer(_isMovingLeft, _isMovingRight);
        }
        
        private void Page_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Left: case VirtualKey.A: _isMovingLeft = false; break;
                case VirtualKey.Right: case VirtualKey.D: _isMovingRight = false; break;
            }
        }
    }
}
