using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace SpaceInvaders.Managers
{
    /// <summary>
    /// Gere a persistência (leitura e escrita) das pontuações mais altas em um ficheiro.
    /// </summary>
    public class ScoreManager
    {
        private const string HighScoreFileName = "highscores.txt";

        /// <summary>
        /// Salva a pontuação de um jogador no ficheiro de texto.
        /// </summary>
        /// <param name="nickname">O nome do jogador.</param>
        /// <param name="finalScore">A pontuação final.</param>
        public async Task SaveScoreAsync(string nickname, int finalScore)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile scoreFile = await localFolder.CreateFileAsync(HighScoreFileName, CreationCollisionOption.OpenIfExists);

            // Garante que o nome não seja muito longo ou vazio.
            if (nickname.Length > 10) nickname = nickname.Substring(0, 10);
            if (string.IsNullOrWhiteSpace(nickname)) nickname = "JOGADOR";

            string scoreEntry = $"{nickname.ToUpper()}:{finalScore}\n";
            await FileIO.AppendTextAsync(scoreFile, scoreEntry);
        }

        /// <summary>
        /// Carrega as 10 melhores pontuações do ficheiro.
        /// </summary>
        /// <returns>Uma lista de strings formatadas, prontas para serem exibidas na UI.</returns>
        public async Task<List<string>> LoadScoresAsync()
        {
            var formattedScores = new List<string>();
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile scoreFile = await localFolder.GetFileAsync(HighScoreFileName);
                var lines = await FileIO.ReadLinesAsync(scoreFile);

                var scores = lines
                    .Select(line => line.Split(':'))
                    .Where(parts => parts.Length == 2 && int.TryParse(parts[1], out _))
                    .Select(parts => new { Nickname = parts[0], Score = int.Parse(parts[1]) })
                    .OrderByDescending(s => s.Score)
                    .Take(10)
                    .ToList();

                if (scores.Any())
                {
                    foreach (var score in scores)
                    {
                        // Formata a string para alinhamento
                        formattedScores.Add($"{score.Nickname.PadRight(12)} {score.Score}");
                    }
                }
                else
                {
                    formattedScores.Add("NENHUMA PONTUAÇÃO REGISTRADA");
                }
            }
            catch (FileNotFoundException)
            {
                // Se o ficheiro não existir, retorna a mensagem padrão.
                formattedScores.Add("NENHUMA PONTUAÇÃO REGISTRADA");
            }

            return formattedScores;
        }
    }
}
