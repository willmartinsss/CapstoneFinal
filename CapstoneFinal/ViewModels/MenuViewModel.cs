using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SpaceInvaders.Services;

namespace SpaceInvaders.ViewModels
{
    public class MenuViewModel : INotifyPropertyChanged
    {
        private readonly ScoreService _scoreService;
        private ObservableCollection<ScoreEntry> _highScores = new();
        
        public ObservableCollection<ScoreEntry> HighScores 
        { 
            get => _highScores; 
            private set { _highScores = value; OnPropertyChanged(); }
        }
        
        public MenuViewModel()
        {
            _scoreService = new ScoreService();
            LoadHighScoresAsync();
        }
        
        private async Task LoadHighScoresAsync()
        {
            var scores = await _scoreService.LoadScoresAsync();
            HighScores = new ObservableCollection<ScoreEntry>(scores);
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
