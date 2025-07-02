using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Core.Entities
{
    /// <summary>
    /// �v���C���[�G���e�B�e�B
    /// </summary>
    public class Player : INotifyPropertyChanged
    {
        private Money _currentMoney;
        private Money _debt;
        private Station? _currentStation;
        private PlayerStatus _status;
        private int _rank;

        /// <summary>
        /// �v���C���[ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// �v���C���[��
        /// </summary>
        public string Name { get; set; } = string.Empty;

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
        /// �����J�[�h���X�g
        /// </summary>
        public List<Card> Cards { get; set; }

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
        public Hero? AssignedHero { get; set; }

        /// <summary>
        /// �t�����Ă���{���r�[
        /// </summary>
        public Bonby? AttachedBonby { get; set; }

        /// <summary>
        /// �����Y�i�������{�����]���z�j
        /// </summary>
        public Money TotalAssets => CalculateTotalAssets();

        /// <summary>
        /// �l�ԃv���C���[���ǂ���
        /// </summary>
        public bool IsHuman { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public int Rank
        {
            get => _rank;
            set => SetProperty(ref _rank, value);
        }

        /// <summary>
        /// �v���C���[�J���[
        /// </summary>
        public PlayerColor Color { get; set; }

        /// <summary>
        /// �N�Ԏ��v�i���Z���ɐݒ�j
        /// </summary>
        public Money YearlyIncome { get; set; }

        /// <summary>
        /// �J�[�h�������
        /// </summary>
        public int MaxCardCount => 8;

        /// <summary>
        /// �X�e�[�^�X�ُ�̎c��^�[����
        /// </summary>
        public int StatusDuration { get; set; }

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public Player()
        {
            Id = Guid.NewGuid();
            OwnedProperties = new List<Property>();
            Cards = new List<Card>();
            CurrentMoney = new Money(100000000); // ��������1���~
            Debt = Money.Zero;
            Status = PlayerStatus.Normal;
            IsHuman = false;
        }

        /// <summary>
        /// �v���C���[���쐬�i�t�@�N�g�����\�b�h�j
        /// </summary>
        public static Player Create(string name, bool isHuman, PlayerColor color)
        {
            return new Player
            {
                Name = name,
                IsHuman = isHuman,
                Color = color
            };
        }

        /// <summary>
        /// �����Y���v�Z
        /// </summary>
        private Money CalculateTotalAssets()
        {
            var propertyValue = OwnedProperties
                .Aggregate(Money.Zero, (total, prop) => total + prop.CurrentPrice);

            return CurrentMoney + propertyValue - Debt;
        }

        /// <summary>
        /// �������x�����i�s�����͎؋��j
        /// </summary>
        public bool Pay(Money amount)
        {
            if (CurrentMoney >= amount)
            {
                CurrentMoney -= amount;
                return true;
            }
            else
            {
                var shortage = amount - CurrentMoney;
                CurrentMoney = Money.Zero;
                Debt += shortage;
                return false; // �؋�������
            }
        }

        /// <summary>
        /// �������󂯎��i�؋��ԍϗD��j
        /// </summary>
        public void Receive(Money amount)
        {
            if (Debt > Money.Zero)
            {
                if (amount >= Debt)
                {
                    amount -= Debt;
                    Debt = Money.Zero;
                }
                else
                {
                    Debt -= amount;
                    amount = Money.Zero;
                }
            }

            CurrentMoney += amount;
        }

        /// <summary>
        /// �J�[�h��ǉ��i����`�F�b�N�t���j
        /// </summary>
        public bool AddCard(Card card)
        {
            if (Cards.Count >= MaxCardCount)
                return false;

            Cards.Add(card);
            return true;
        }

        /// <summary>
        /// �J�[�h���g�p
        /// </summary>
        public void UseCard(Card card)
        {
            card.UsageCount--;
            if (card.UsageCount <= 0)
            {
                Cards.Remove(card);
            }
        }

        /// <summary>
        /// �X�e�[�^�X�ُ��ݒ�
        /// </summary>
        public void SetStatus(PlayerStatus status, int duration)
        {
            Status = status;
            StatusDuration = duration;
        }

        /// <summary>
        /// �^�[���I�����̏���
        /// </summary>
        public void ProcessTurnEnd()
        {
            // �X�e�[�^�X�ُ�̊��Ԃ����炷
            if (StatusDuration > 0)
            {
                StatusDuration--;
                if (StatusDuration == 0)
                {
                    Status = PlayerStatus.Normal;
                }
            }
        }

        /// <summary>
        /// �v���C���[�J���[��16�i���\�����擾
        /// </summary>
        public string GetColorHex()
        {
            return Color switch
            {
                PlayerColor.Blue => "#0000FF",
                PlayerColor.Red => "#FF0000",
                PlayerColor.Green => "#00FF00",
                PlayerColor.Yellow => "#FFFF00",
                PlayerColor.Purple => "#800080",
                PlayerColor.Orange => "#FFA500",
                PlayerColor.Pink => "#FFC0CB",
                PlayerColor.Cyan => "#00FFFF",
                _ => "#808080"
            };
        }

        /// <summary>
        /// ������\��
        /// </summary>
        public override string ToString()
        {
            return $"{Name} - �����Y: {TotalAssets}, ����: {Rank}��";
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
    /// �q�[���[�i���j��̈̐l�j
    /// </summary>
    public class Hero
    {
        /// <summary>
        /// �q�[���[ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// �q�[���[��
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// �l�������̐���
        /// </summary>
        public string AcquisitionCondition { get; set; } = string.Empty;

        /// <summary>
        /// ���ʂ̐���
        /// </summary>
        public string EffectDescription { get; set; } = string.Empty;

        /// <summary>
        /// �A�C�R���p�X
        /// </summary>
        public string IconPath { get; set; } = string.Empty;

        /// <summary>
        /// ���ʂ�K�p���郁�\�b�h�i��ŏڍ׎����j
        /// </summary>
        public virtual void ApplyEffect(Player player, GameState gameState)
        {
            // �e�q�[���[���Ƃ̌��ʂ�����
        }
    }

    /// <summary>
    /// �{���r�[
    /// </summary>
    public class Bonby
    {
        /// <summary>
        /// �{���r�[�̎��
        /// </summary>
        public BonbyType Type { get; set; }

        /// <summary>
        /// �{���r�[�̖��O
        /// </summary>
        public string Name => GetBonbyName();

        /// <summary>
        /// �A�C�R���p�X
        /// </summary>
        public string IconPath => GetIconPath();

        /// <summary>
        /// ���s�����s
        /// </summary>
        public virtual void ExecuteMischief(Player player, GameState gameState)
        {
            // �{���r�[�̎�ނɉ��������s������
        }

        /// <summary>
        /// �{���r�[�̖��O���擾
        /// </summary>
        private string GetBonbyName()
        {
            return Type switch
            {
                BonbyType.Mini => "�~�j�{���r�[",
                BonbyType.Normal => "�{���r�[",
                BonbyType.King => "�L���O�{���r�[",
                BonbyType.Pokon => "�|�R��",
                BonbyType.BonbyrasAlien => "�{���r���X��",
                _ => "�{���r�["
            };
        }

        /// <summary>
        /// �A�C�R���p�X���擾
        /// </summary>
        private string GetIconPath()
        {
            return $"/Resources/Images/Bonby/{Type}.png";
        }
    }

    /// <summary>
    /// �{���r�[�̎��
    /// </summary>
    public enum BonbyType
    {
        Mini,           // �~�j�{���r�[
        Normal,         // �{���r�[
        King,           // �L���O�{���r�[
        Pokon,          // �|�R��
        BonbyrasAlien   // �{���r���X��
    }
}