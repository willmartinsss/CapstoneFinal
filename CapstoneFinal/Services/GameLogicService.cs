using SpaceInvaders.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceInvaders.Services
{
    public class GameLogicService
    {
        public void CreateEnemies(GameState gameState, double canvasWidth)
        {
            gameState.Enemies.Clear();
            
            int enemyRows = 5;
            int enemyCols = 11;
            double enemyWidth = 40;
            double enemyHeight = 30;
            double enemySpacing = 15;
            double startY = 70 + ((gameState.WaveNumber - 1) * 20);
            
            double gridWidth = (enemyCols * enemyWidth) + ((enemyCols - 1) * enemySpacing);
            double startX = (canvasWidth - gridWidth) / 2;
            
            for (int row = 0; row < enemyRows; row++)
            {
                for (int col = 0; col < enemyCols; col++)
                {
                    EnemyType type = row == 0 ? EnemyType.Type40 : (row < 3 ? EnemyType.Type20 : EnemyType.Type10);
                    var enemy = new Enemy(type)
                    {
                        X = startX + col * (enemyWidth + enemySpacing),
                        Y = startY + row * (enemyHeight + enemySpacing)
                    };
                    gameState.Enemies.Add(enemy);
                }
            }
        }
        
        public void CreateShields(GameState gameState, double canvasWidth)
        {
            gameState.ShieldParts.Clear();
            
            int shieldBlockSize = 8;
            int numberOfShields = 4;
            int shieldGridWidth = 7;
            int shieldGridHeight = 5;
            
            double shieldPixelWidth = shieldGridWidth * shieldBlockSize;
            double totalShieldsArea = canvasWidth * 0.8;
            double shieldSpacing = (totalShieldsArea - (numberOfShields * shieldPixelWidth)) / (numberOfShields - 1);
            double startX = canvasWidth * 0.1;
            double shieldBaseY = 720;
            
            for (int i = 0; i < numberOfShields; i++)
            {
                double shieldBaseX = startX + i * (shieldPixelWidth + shieldSpacing);
                for (int row = 0; row < shieldGridHeight; row++)
                {
                    for (int col = 0; col < shieldGridWidth; col++)
                    {
                        if (row == 0 && (col == 0 || col == shieldGridWidth - 1)) continue;
                        if (row == shieldGridHeight - 1 && (col > 1 && col < shieldGridWidth - 2)) continue;
                        
                        var shieldPart = new ShieldPart
                        {
                            X = shieldBaseX + col * shieldBlockSize,
                            Y = shieldBaseY + row * shieldBlockSize
                        };
                        gameState.ShieldParts.Add(shieldPart);
                    }
                }
            }
        }
        
        public void MoveEnemies(GameState gameState, double canvasWidth)
        {
            if (!gameState.Enemies.Any()) return;
            
            bool edgeReached = false;
            foreach (var enemy in gameState.Enemies)
            {
                enemy.X += gameState.EnemySpeed * gameState.EnemyDirection;
                
                if ((enemy.X <= 0 && gameState.EnemyDirection == -1) || 
                    (enemy.X + enemy.Width >= canvasWidth && gameState.EnemyDirection == 1))
                {
                    edgeReached = true;
                }
            }
            
            if (edgeReached)
            {
                gameState.EnemyDirection *= -1;
                gameState.EnemySpeed += 0.1;
                if (gameState.EnemyFireRate > 300) gameState.EnemyFireRate -= 50;
                
                foreach (var enemy in gameState.Enemies)
                {
                    enemy.Y += 20;
                }
            }
        }
        
        public Enemy? GetRandomShooter(GameState gameState)
        {
            var shooters = gameState.Enemies
                .GroupBy(enemy => enemy.X)
                .Select(group => group.OrderByDescending(enemy => enemy.Y).First())
                .ToList();
                
            return shooters.Any() ? shooters[new Random().Next(shooters.Count)] : null;
        }
    }
}
