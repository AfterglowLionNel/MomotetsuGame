using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Core.Interfaces
{
    /// <summary>
    /// �Q�[���S�̂̐i�s���Ǘ�����C���^�[�t�F�[�X
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
        /// �Q�[���i�s�C�x���g
        /// </summary>
        event EventHandler<GameEventArgs>? GameEventOccurred;

        /// <summary>
        /// �^�[���ύX�C�x���g
        /// </summary>
        event EventHandler<TurnChangedEventArgs>? TurnChanged;

        /// <summary>
        /// �v���C���[�ړ��C�x���g
        /// </summary>
        event EventHandler<PlayerMovedEventArgs>? PlayerMoved;

        /// <summary>
        /// ���z�ύX�C�x���g
        /// </summary>
        event EventHandler<MoneyChangedEventArgs>? MoneyChanged;

        /// <summary>
        /// �����w���C�x���g
        /// </summary>
        event EventHandler<PropertyPurchasedEventArgs>? PropertyPurchased;

        /// <summary>
        /// �J�[�h�g�p�C�x���g
        /// </summary>
        event EventHandler<CardUsedEventArgs>? CardUsed;

        /// <summary>
        /// �Q�[�����b�Z�[�W�C�x���g
        /// </summary>
        event EventHandler<GameMessageEventArgs>? GameMessage;

        // �Q�[���J�n�E�I��
        Task<GameState> StartNewGameAsync(GameSettings settings);
        Task<GameState> LoadGameAsync(string saveId);
        Task SaveGameAsync(string saveId);
        Task EndGameAsync();

        // �^�[���i�s
        Task StartTurnAsync();
        Task EndTurnAsync();
        Task<DiceResult> RollDiceAsync();
        Task<MoveResult> MovePlayerAsync(Player player, int steps);
        Task ProcessArrivalAsync(Player player, Station station);

        // �v���C���[�A�N�V����
        Task<PurchaseResult> PurchasePropertiesAsync(Player player, List<Property> properties);
        Task<bool> SellPropertyAsync(Player player, Property property);
        Task<CardEffectResult> UseCardAsync(Card card);
        Task<List<Card>> ShowCardShopAsync(Station station);
        Task<bool> PurchaseCardAsync(Player player, Card card);

        // �ړI�n�֘A
        void SetDestination(Station station);
        Station GetDestination();
        Task ProcessDestinationArrivalAsync(Player player);

        // �C�x���g����
        Task ProcessSpecialStationAsync(Station station);
        Task ProcessMonthEndAsync();
        Task ProcessYearEndAsync();

        // ���[�e�B���e�B
        List<Station> GetRoute(Station from, int steps);
        Money CalculatePlusStationBonus();
        Money CalculateMinusStationPenalty();
        bool CheckMonopoly(Player player, Station station);
        void UpdatePlayerRankings();
    }

    /// <summary>
    /// �ړ�����
    /// </summary>
    public class MoveResult
    {
        public Player Player { get; set; } = null!;
        public List<Station> Route { get; set; } = new List<Station>();
        public Station FinalStation { get; set; } = null!;
        public List<Station> PassedStations { get; set; } = new List<Station>();
        public bool ReachedDestination { get; set; }
    }

    /// <summary>
    /// �w������
    /// </summary>
    public class PurchaseResult
    {
        public bool Success { get; set; }
        public List<Property> PurchasedProperties { get; set; } = new List<Property>();
        public Money TotalCost { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// �J�[�h���ʌ���
    /// </summary>
    public class CardEffectResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool AllowContinuation { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }

    #region Event Args

    /// <summary>
    /// �Q�[���C�x���g�����̊��N���X
    /// </summary>
    public class GameEventArgs : EventArgs
    {
        public DateTime OccurredAt { get; set; }
        public string Description { get; set; } = string.Empty;
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
        public int Steps { get; set; }
    }

    /// <summary>
    /// ���z�ύX�C�x���g����
    /// </summary>
    public class MoneyChangedEventArgs : GameEventArgs
    {
        public Player Player { get; set; } = null!;
        public Money OldAmount { get; set; }
        public Money NewAmount { get; set; }
        public Money Difference { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// �����w���C�x���g����
    /// </summary>
    public class PropertyPurchasedEventArgs : GameEventArgs
    {
        public Player Player { get; set; } = null!;
        public List<Property> Properties { get; set; } = new List<Property>();
        public Money TotalCost { get; set; }
        public bool AchievedMonopoly { get; set; }
    }

    /// <summary>
    /// �J�[�h�g�p�C�x���g����
    /// </summary>
    public class CardUsedEventArgs : GameEventArgs
    {
        public Player Player { get; set; } = null!;
        public Card Card { get; set; } = null!;
        public CardEffectResult Result { get; set; } = null!;
    }

    /// <summary>
    /// �Q�[�����b�Z�[�W�C�x���g����
    /// </summary>
    public class GameMessageEventArgs : GameEventArgs
    {
        public MessageType Type { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    /// <summary>
    /// ���b�Z�[�W�^�C�v
    /// </summary>
    public enum MessageType
    {
        Info,
        Success,
        Warning,
        Error,
        Important
    }

    #endregion
}