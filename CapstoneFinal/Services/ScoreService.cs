using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace SpaceInvaders.Services
{
    public class ScoreEntry
    {
        public string Nickname { get; set; } = "";
        public int Score { get; set; }
    }
    
    public class ScoreService
    {
        private const string HighScoreFileName = "highscores.txt";
        
        public async Task SaveScoreAsync(string nickname, int score)
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var scoreFile = await localFolder.CreateFileAsync(HighScoreFileName, CreationCollisionOption.OpenIfExists);
                
                if (nickname.Length > 10) nickname = nickname.Substring(0, 10);
                if (string.IsNullOrWhiteSpace(nickname)) nickname = "JOGADOR";
                
                string scoreEntry = $"{nickname.ToUpper()}:{score}\n";
                await FileIO.AppendTextAsync(scoreFile, scoreEntry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving score: {ex.Message}");
            }
        }
        
        public async Task<List<ScoreEntry>> LoadScoresAsync()
        {
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;
                var scoreFile = await localFolder.GetFileAsync(HighScoreFileName);
                var lines = await FileIO.ReadLinesAsync(scoreFile);
                
                return lines
                    .Select(line => line.Split(':'))
                    .Where(parts => parts.Length == 2 && int.TryParse(parts[1], out _))
                    .Select(parts => new ScoreEntry { Nickname = parts[0], Score = int.Parse(parts[1]) })
                    .OrderByDescending(s => s.Score)
                    .Take(10)
                    .ToList();
            }
            catch (FileNotFoundException)
            {
                return new List<ScoreEntry>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading scores: {ex.Message}");
                return new List<ScoreEntry>();
            }
        }
    }
}
