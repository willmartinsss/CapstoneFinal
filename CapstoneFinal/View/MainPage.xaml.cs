using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using SpaceInvaders.ViewModels;
using System.Threading.Tasks;

namespace SpaceInvaders.Views
{
    public sealed partial class MainPage : Page
    {
        private bool _isMovingLeft = false;
        private bool _isMovingRight = false;
        
        public GameViewModel GameViewModel { get; private set; }
        
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += OnPageLoaded;
        }
        
        // This will be called by Uno's DI system
        public void SetViewModel(GameViewModel viewModel)
        {
            GameViewModel = viewModel;
            this.DataContext = viewModel;
        }
        
        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            // Set canvas size for the view model
            GameViewModel?.SetCanvasSize(GameCanvas.ActualWidth, GameCanvas.ActualHeight);
            
            await Task.Delay(100);
            this.Focus(FocusState.Programmatic);
        }
        
        private async void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (GameViewModel == null) return;
            
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
                    await GameViewModel.ShootAsync();
                    break;
            }
            
            GameViewModel.MovePlayer(_isMovingLeft, _isMovingRight);
        }
        
        private void Page_KeyUp(object sender, KeyRoutedEventArgs e)
        {
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
        }
    }
}
