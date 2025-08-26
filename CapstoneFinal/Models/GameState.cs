using System.Collections.ObjectModel;

namespace SpaceInvaders.Models
{
    public enum GameStateType { Menu, Playing, GameOver, HighScores, Controls }
    
    public class GameState
    {
        public GameStateType CurrentState { get; set; } = GameStateType.Menu;
        public int Score { get; set; }
        public int WaveNumber { get; set; } = 1;
        public double EnemySpeed { get; set; } = 1.5;
        public int EnemyDirection { get; set; } = 1;
        public double EnemyFireRate { get; set; } = 1000;
        
        public Player Player { get; set; } = new Player();
        public ObservableCollection<Enemy> Enemies { get; set; } = new();
        public ObservableCollection<Projectile> Projectiles { get; set; } = new();
        public ObservableCollection<ShieldPart> ShieldParts { get; set; } = new();
        public Enemy? SpecialEnemy { get; set; }
        
        public void Reset()
        {
            Score = 0;
            WaveNumber = 1;
            EnemySpeed = 1.5;
            EnemyDirection = 1;
            EnemyFireRate = 1000;
            Player = new Player();
            Enemies.Clear();
            Projectiles.Clear();
            ShieldParts.Clear();
            SpecialEnemy = null;
        }
    }
}
