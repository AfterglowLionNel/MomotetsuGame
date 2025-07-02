using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Core.Interfaces
{
    /// <summary>
    /// �Q�[���Ǘ��̃C���^�[�t�F�[�X
    /// </summary>
    public interface IGameManager
    {
        /// <summary>
        /// ���݂̃Q�[�����
        /// </summary>
        GameState CurrentGameState { get; }

        /// <summary>
        /// ���݂̃v���C���[
        /// </summary>
        Player CurrentPlayer { get; }

        /// <summary>
        /// �Q�[���C�x���g
        /// </summary>
        event EventHandler<GameEventArgs>? GameEvent;

        /// <summary>
        /// �^�[���ύX�C�x���g
        /// </summary>
        event EventHandler<TurnChangedEventArgs>? TurnChanged;

        /// <summary>
        /// �v���C���[�ړ��C�x���g
        /// </summary>
        event EventHandler<PlayerMovedEventArgs>? PlayerMoved;

        /// <summary>
        /// �V�����Q�[�����J�n
        /// </summary>
        Task<GameState> StartNewGameAsync(GameSettings settings);

        /// <summary>
        /// �Q�[�������[�h
        /// </summary>
        Task<GameState> LoadGameAsync(string saveId);

        /// <summary>
        /// �Q�[�����Z�[�u
        /// </summary>
        Task SaveGameAsync(string saveId);

        /// <summary>
        /// �T�C�R����U��
        /// </summary>
        Task<DiceResult> RollDiceAsync();

        /// <summary>
        /// �v���C���[���ړ�
        /// </summary>
        Task<MoveResult> MovePlayerAsync(Player player, int steps);

        /// <summary>
        /// �J�[�h���g�p
        /// </summary>
        Task<CardEffectResult> UseCardAsync(Card card);

        /// <summary>
        /// �������w��
        /// </summary>
        Task<bool> PurchasePropertyAsync(Player player, Property property);

        /// <summary>
        /// �����𔄋p
        /// </summary>
        Task<bool> SellPropertyAsync(Player player, Property property);

        /// <summary>
        /// �^�[�����I��
        /// </summary>
        Task EndTurnAsync();

        /// <summary>
        /// �ړI�n��������
        /// </summary>
        Task ProcessDestinationArrivalAsync(Player player);

        /// <summary>
        /// �ړ��\�ȉw���擾
        /// </summary>
        List<Station> GetReachableStations(Station from, int steps);

        /// <summary>
        /// �Q�[���I���`�F�b�N
        /// </summary>
        bool IsGameOver();

        /// <summary>
        /// �ŏI���ʂ��擾
        /// </summary>
        GameResult GetGameResult();
    }

    /// <summary>
    /// �Q�[���C�x���g�����̊��N���X
    /// </summary>
    public class GameEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// �^�[���ύX�C�x���g����
    /// </summary>
    public class TurnChangedEventArgs : GameEventArgs
    {
        public Player CurrentPlayer { get; set; } = null!;
        public int Turn { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }

    /// <summary>
    /// �v���C���[�ړ��C�x���g����
    /// </summary>
    public class PlayerMovedEventArgs : GameEventArgs
    {
        public Player Player { get; set; } = null!;
        public Station FromStation { get; set; } = null!;
        public Station ToStation { get; set; } = null!;
        public List<Station> Route { get; set; } = new List<Station>();
    }

    /// <summary>
    /// �ړ�����
    /// </summary>
    public class MoveResult
    {
        public Player Player { get; set; } = null!;
        public List<Station> Route { get; set; } = new List<Station>();
        public Station FinalStation { get; set; } = null!;
        public bool ReachedDestination { get; set; }
        public List<string> Events { get; set; } = new List<string>();
    }

    /// <summary>
    /// �Q�[������
    /// </summary>
    public class GameResult
    {
        public List<PlayerResult> PlayerResults { get; set; } = new List<PlayerResult>();
        public Player Winner { get; set; } = null!;
        public int TotalTurns { get; set; }
        public int Years { get; set; }
        public DateTime GameDuration { get; set; }
    }

    /// <summary>
    /// �v���C���[����
    /// </summary>
    public class PlayerResult
    {
        public Player Player { get; set; } = null!;
        public int FinalRank { get; set; }
        public Money FinalAssets { get; set; }
        public int PropertyCount { get; set; }
        public int DestinationCount { get; set; }
        public Money MaxAssets { get; set; }
    }
}