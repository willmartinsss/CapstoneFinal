using Microsoft.UI;
using Microsoft.UI.Xaml; // Namespace correto para Visibility
using Microsoft.UI.Xaml.Media;
using CapstoneSpaceInvader.Models; // Certifique-se de que este é o namespace do seu projeto
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using SpaceInvaders.Managers; // Namespace correto para a struct Rect

namespace CapstoneSpaceInvader.Managers
{
    /// <summary>
    /// Responsável por detetar todas as colisões entre os objetos do jogo.
    /// Não modifica o estado do jogo diretamente, apenas reporta colisões
    /// ou delega a ação para o manager apropriado (ex: PlayerManager.PlayerHit).
    /// </summary>
    public class CollisionManager
    {
        private readonly PlayerManager _playerManager;
        private readonly EnemyManager _enemyManager;
        private readonly List<ShieldPart> _shieldParts;
        private readonly AudioManager _audioManager;
        private readonly System.Action<int> _onScoreUpdate; // Action para notificar o GameManager para atualizar a pontuação

        public CollisionManager(PlayerManager playerManager, EnemyManager enemyManager, List<ShieldPart> shieldParts, AudioManager audioManager, System.Action<int> onScoreUpdate)
        {
            _playerManager = playerManager;
            _enemyManager = enemyManager;
            _shieldParts = shieldParts;
            _audioManager = audioManager;
            _onScoreUpdate = onScoreUpdate;
        }

        /// <summary>
        /// Função auxiliar que substitui o método IntersectsWith.
        /// Verifica se dois retângulos se sobrepõem.
        /// </summary>
        private bool CheckIntersection(Rect rect1, Rect rect2)
        {
            rect1.Intersect(rect2);
            return !rect1.IsEmpty;
        }

        /// <summary>
        /// Executa todas as verificações de colisão para o frame atual, numa ordem lógica.
        /// </summary>
        public void CheckAllCollisions()
        {
            var player = _playerManager.Player;
            var playerProjectile = _playerManager.PlayerProjectile;

            // 1. Colisão do projétil do jogador com outras entidades
            if (playerProjectile != null)
            {
                var enemyHit = _enemyManager.Enemies.FirstOrDefault(e => CheckIntersection(playerProjectile.GetBounds(), e.GetBounds()));
                if (enemyHit != null) { HandlePlayerProjectileHit(enemyHit); return; }

                if (_enemyManager.SpecialEnemy != null && CheckIntersection(playerProjectile.GetBounds(), _enemyManager.SpecialEnemy.GetBounds()))
                {
                    HandlePlayerProjectileHit(_enemyManager.SpecialEnemy, isSpecial: true);
                    return;
                }
            }

            // 2. Colisão dos projéteis inimigos com o jogador
            var enemyProjectileHit = _enemyManager.EnemyProjectiles.FirstOrDefault(p => CheckIntersection(p.GetBounds(), player.GetBounds()));
            if (enemyProjectileHit != null)
            {
                _enemyManager.RemoveProjectile(enemyProjectileHit);
                _playerManager.PlayerHit(); // Delega a lógica de "ser atingido" para o PlayerManager
                return;
            }

            // 3. Colisão de todos os projéteis com os escudos
            CheckProjectileShieldCollisions();
        }

        /// <summary>
        /// Lida com as consequências de um projétil do jogador atingir um inimigo.
        /// </summary>
        private void HandlePlayerProjectileHit(Enemy enemy, bool isSpecial = false)
        {
            _audioManager.PlaySoundEffect("invaderkilled.wav");
            _onScoreUpdate(enemy.PointValue); // Notifica o GameManager para adicionar pontos

            if (isSpecial) { _enemyManager.RemoveSpecialEnemy(); }
            else { _enemyManager.RemoveEnemy(enemy); }
            
            _playerManager.RemoveProjectile();
        }

        /// <summary>
        /// Verifica colisões entre projéteis (de ambos os lados) e os escudos.
        /// </summary>
        private void CheckProjectileShieldCollisions()
        {
            // Projétil do jogador
            if (_playerManager.PlayerProjectile != null)
            {
                var shieldPartHit = _shieldParts.FirstOrDefault(s => CheckIntersection(_playerManager.PlayerProjectile.GetBounds(), s.GetBounds()));
                if (shieldPartHit != null) { DamageShield(shieldPartHit); _playerManager.RemoveProjectile(); return; }
            }

            // Projéteis inimigos
            foreach (var projectile in _enemyManager.EnemyProjectiles.ToList())
            {
                var shieldPartHit = _shieldParts.FirstOrDefault(s => CheckIntersection(projectile.GetBounds(), s.GetBounds()));
                if (shieldPartHit != null) { DamageShield(shieldPartHit); _enemyManager.RemoveProjectile(projectile); }
            }
        }

        /// <summary>
        /// Aplica dano a uma parte do escudo e atualiza a sua aparência ou remove-a.
        /// </summary>
        private void DamageShield(ShieldPart shieldPart)
        {
            shieldPart.Health--;
            if (shieldPart.Health <= 0)
            {
                shieldPart.Shape.Visibility = Visibility.Collapsed;
                _shieldParts.Remove(shieldPart);
            }
            else
            {
                shieldPart.Shape.Fill = shieldPart.Health switch
                {
                    2 => new SolidColorBrush(Colors.YellowGreen),
                    1 => new SolidColorBrush(Colors.Orange),
                    _ => shieldPart.Shape.Fill
                };
            }
        }
    }
}
