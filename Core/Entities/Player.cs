using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;
using Newtonsoft.Json.Linq;

namespace MomotetsuGame.Core.Entities
{
    /// <summary>
    /// �v���C���[�G���e�B�e�B
    /// </summary>
    public class Player : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private Money _currentMoney;
        private Money _debt;
        private Station? _currentStation;
        private PlayerStatus _status;
        private Hero? _assignedHero;
        private Bonby? _attachedBonby;
        private int _rank;
        private bool _isHuman;

        /// <summary>
        /// �v���C���[ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// �v���C���[��
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// ���݂̏�����
        /// </summary>
        public Money CurrentMoney
        {
            get => _currentMoney;
            set => SetProperty(ref _currentMoney, value);
        }

        /// <summary>
        /// �؋��z
        /// </summary>
        public Money Debt
        {
            get => _debt;
            set => SetProperty(ref _debt, value);
        }

        /// <summary>
        /// ���݂���w
        /// </summary>
        public Station? CurrentStation
        {
            get => _currentStation;
            set => SetProperty(ref _currentStation, value);
        }

        /// <summary>
        /// ���L�������X�g
        /// </summary>
        public List<Property> OwnedProperties { get; set; }

        /// <summary>
        /// �����J�[�h�R���N�V����
        /// </summary>
        public CardCollection Cards { get; set; }

        /// <summary>
        /// �v���C���[�X�e�[�^�X
        /// </summary>
        public PlayerStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        /// <summary>
        /// ���蓖�Ă�ꂽ�q�[���[
        /// </summary>
        public Hero? AssignedHero
        {
            get => _assignedHero;
            set => SetProperty(ref _assignedHero, value);
        }

        /// <summary>
        /// �t�����Ă���{���r�[
        /// </summary>
        public Bonby? AttachedBonby
        {
            get => _attachedBonby;
            set => SetProperty(ref _attachedBonby, value);
        }

        /// <summary>
        /// �����Y�i�������{�����]���z�j
        /// </summary>
        public Money TotalAssets => CalculateTotalAssets();

        /// <summary>
        /// �l�ԃv���C���[���ǂ���
        /// </summary>
        public bool IsHuman
        {
            get => _isHuman;
            set => SetProperty(ref _isHuman, value);
        }

        /// <summary>
        /// ���݂̏���
        /// </summary>
        public int Rank
        {
            get => _rank;
            set => SetProperty(ref _rank, value);
        }

        /// <summary>
        /// �F�iUI�\���p�j
        /// </summary>
        public PlayerColor Color { get; set; }

        /// <summary>
        /// �A�C�R���p�X�iUI�\���p�j
        /// </summary>
        public string IconPath { get; set; } = string.Empty;

        /// <summary>
        /// �N�Ԏ��v�i���Z�p�j
        /// </summary>
        public Money YearlyIncome { get; set; }

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public Player()
        {
            Id = Guid.NewGuid();
            OwnedProperties = new List<Property>();
            Cards = new CardCollection();
            CurrentMoney = new Money(100000000); // ��������1���~
            Debt = Money.Zero;
            Status = PlayerStatus.Normal;
            Rank = 1;
            IsHuman = false;
        }

        /// <summary>
        /// �����Y���v�Z
        /// </summary>
        private Money CalculateTotalAssets()
        {
            var propertyValue = OwnedProperties
                .Where(p => p != null)
                .Aggregate(Money.Zero, (sum, prop) => sum + prop.CurrentPrice);

            return CurrentMoney + propertyValue - Debt;
        }

        /// <summary>
        /// �������w��
        /// </summary>
        public bool PurchaseProperty(Property property)
        {
            if (property == null || property.Owner != null)
                return false;

            if (CurrentMoney < property.BasePrice)
                return false;

            CurrentMoney -= property.BasePrice;
            property.Owner = this;
            OwnedProperties.Add(property);

            OnPropertyChanged(nameof(TotalAssets));
            return true;
        }

        /// <summary>
        /// �����𔄋p
        /// </summary>
        public bool SellProperty(Property property)
        {
            if (property == null || !OwnedProperties.Contains(property))
                return false;

            var sellPrice = property.CurrentPrice * 0.7m; // ���p�z��70%
            CurrentMoney += sellPrice;
            property.Owner = null;
            OwnedProperties.Remove(property);

            OnPropertyChanged(nameof(TotalAssets));
            return true;
        }

        /// <summary>
        /// �J�[�h���g�p
        /// </summary>
        public bool UseCard(Card card)
        {
            if (card == null || !Cards.Contains(card))
                return false;

            if (Status == PlayerStatus.Sealed)
                return false;

            return Cards.Use(card);
        }

        /// <summary>
        /// �؋�������
        /// </summary>
        public void Borrow(Money amount)
        {
            Debt += amount;
            CurrentMoney += amount;
        }

        /// <summary>
        /// �؋���ԍ�
        /// </summary>
        public bool RepayDebt(Money amount)
        {
            if (CurrentMoney < amount)
                return false;

            var repayAmount = amount > Debt ? Debt : amount;
            Debt -= repayAmount;
            CurrentMoney -= repayAmount;

            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }

    /// <summary>
    /// �J�[�h�R���N�V����
    /// </summary>
    public class CardCollection : List<Card>
    {
        public const int MaxCards = 8;
        public const int MaxBankCards = 16;

        /// <summary>
        /// �ʏ�̏����J�[�h
        /// </summary>
        public List<Card> HandCards => this.Take(MaxCards).ToList();

        /// <summary>
        /// �J�[�h�o���N�̃J�[�h
        /// </summary>
        public List<Card> BankCards => this.Skip(MaxCards).Take(MaxBankCards).ToList();

        /// <summary>
        /// �J�[�h��ǉ��i��������`�F�b�N�t���j
        /// </summary>
        public bool TryAdd(Card card)
        {
            if (Count >= MaxCards + MaxBankCards)
                return false;

            Add(card);
            return true;
        }

        /// <summary>
        /// �J�[�h���g�p
        /// </summary>
        public bool Use(Card card)
        {
            if (!Contains(card))
                return false;

            card.UsageCount--;
            if (card.UsageCount <= 0)
            {
                Remove(card);
            }

            return true;
        }
    }

    /// <summary>
    /// �q�[���[�N���X�i���j��̈̐l�j
    /// </summary>
    public class Hero
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public HeroType Type { get; set; }
        public Station RequiredStation { get; set; } = null!;

        /// <summary>
        /// �q�[���[���ʂ�K�p
        /// </summary>
        public virtual void ApplyEffect(Player player, GameContext context)
        {
            // �h���N���X�Ŏ���
        }
    }

    /// <summary>
    /// �{���r�[�N���X
    /// </summary>
    public class Bonby
    {
        public string Name { get; set; } = string.Empty;
        public BonbyType Type { get; set; }
        public int TurnsRemaining { get; set; }

        /// <summary>
        /// �{���r�[�̈��s�����s
        /// </summary>
        public virtual void ExecuteMischief(Player player, GameContext context)
        {
            // �h���N���X�Ŏ���
        }
    }

    /// <summary>
    /// �Q�[���R���e�L�X�g�i���j
    /// </summary>
    public class GameContext
    {
        // ��Ŏ���
    }
}