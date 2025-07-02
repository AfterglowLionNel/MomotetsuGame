using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Application.Services
{
    /// <summary>
    /// �����T�[�r�X�̃C���^�[�t�F�[�X
    /// </summary>
    public interface IPropertyService
    {
        /// <summary>
        /// �������w��
        /// </summary>
        Task<PropertyTransactionResult> PurchasePropertyAsync(Player player, Property property);

        /// <summary>
        /// �����̕������w��
        /// </summary>
        Task<PropertyTransactionResult> PurchasePropertiesAsync(Player player, List<Property> properties);

        /// <summary>
        /// �����𔄋p
        /// </summary>
        Task<PropertyTransactionResult> SellPropertyAsync(Player player, Property property);

        /// <summary>
        /// �Ɛ�`�F�b�N
        /// </summary>
        bool CheckMonopoly(Player player, Station station);

        /// <summary>
        /// �������v���v�Z
        /// </summary>
        Money CalculateIncome(Player player);

        /// <summary>
        /// �������l���X�V
        /// </summary>
        void UpdatePropertyValues(PropertyMarket market);

        /// <summary>
        /// �v���C���[�̕������v���擾
        /// </summary>
        PropertyStatistics GetPropertyStatistics(Player player);
    }

    /// <summary>
    /// �����T�[�r�X�̎���
    /// </summary>
    public class PropertyService : IPropertyService
    {
        /// <summary>
        /// �����w���C�x���g
        /// </summary>
        public event EventHandler<PropertyTransactionEventArgs>? PropertyPurchased;

        /// <summary>
        /// �������p�C�x���g
        /// </summary>
        public event EventHandler<PropertyTransactionEventArgs>? PropertySold;

        /// <summary>
        /// �Ɛ�B���C�x���g
        /// </summary>
        public event EventHandler<MonopolyAchievedEventArgs>? MonopolyAchieved;

        /// <summary>
        /// �������w��
        /// </summary>
        public async Task<PropertyTransactionResult> PurchasePropertyAsync(Player player, Property property)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var result = new PropertyTransactionResult();

            // �w���\�`�F�b�N
            if (property.Owner != null)
            {
                result.Success = false;
                result.Message = $"{property.Name}�͊���{property.Owner.Name}�����L���Ă��܂��B";
                return result;
            }

            // �����`�F�b�N
            if (player.CurrentMoney < property.CurrentPrice)
            {
                result.Success = false;
                result.Message = "���������s�����Ă��܂��B";
                result.RequiredMoney = property.CurrentPrice;
                result.ShortAmount = property.CurrentPrice - player.CurrentMoney;
                return result;
            }

            // �w������
            player.Pay(property.CurrentPrice);
            property.Owner = player;
            player.OwnedProperties.Add(property);

            result.Success = true;
            result.TransactionType = TransactionType.Purchase;
            result.Properties.Add(property);
            result.TotalAmount = property.CurrentPrice;
            result.Message = $"{property.Name}���w�����܂����I";

            // �C�x���g����
            PropertyPurchased?.Invoke(this, new PropertyTransactionEventArgs
            {
                Player = player,
                Property = property,
                Amount = property.CurrentPrice
            });

            // �Ɛ�`�F�b�N
            if (CheckMonopoly(player, property.Location))
            {
                result.AchievedMonopoly = true;
                result.Message += $"\n{property.Location.Name}��Ɛ肵�܂����I";

                MonopolyAchieved?.Invoke(this, new MonopolyAchievedEventArgs
                {
                    Player = player,
                    Station = property.Location
                });
            }

            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// �����̕������w��
        /// </summary>
        public async Task<PropertyTransactionResult> PurchasePropertiesAsync(Player player, List<Property> properties)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (properties == null || properties.Count == 0)
                throw new ArgumentException("�w�����镨�����w�肳��Ă��܂���B", nameof(properties));

            var result = new PropertyTransactionResult
            {
                TransactionType = TransactionType.Purchase
            };

            // �w���\�`�F�b�N
            var availableProperties = properties.Where(p => p.Owner == null).ToList();
            if (availableProperties.Count == 0)
            {
                result.Success = false;
                result.Message = "�w���\�ȕ���������܂���B";
                return result;
            }

            // ���v���z�v�Z
            var totalPrice = availableProperties.Aggregate(Money.Zero, (sum, p) => sum + p.CurrentPrice);

            // �����`�F�b�N
            if (player.CurrentMoney < totalPrice)
            {
                result.Success = false;
                result.Message = "���������s�����Ă��܂��B";
                result.RequiredMoney = totalPrice;
                result.ShortAmount = totalPrice - player.CurrentMoney;
                return result;
            }

            // �w������
            foreach (var property in availableProperties)
            {
                player.Pay(property.CurrentPrice);
                property.Owner = player;
                player.OwnedProperties.Add(property);
                result.Properties.Add(property);

                PropertyPurchased?.Invoke(this, new PropertyTransactionEventArgs
                {
                    Player = player,
                    Property = property,
                    Amount = property.CurrentPrice
                });
            }

            result.Success = true;
            result.TotalAmount = totalPrice;
            result.Message = $"{availableProperties.Count}���̕������w�����܂����I";

            // �Ɛ�`�F�b�N�i�w���ƂɁj
            var stationGroups = availableProperties.GroupBy(p => p.Location);
            foreach (var group in stationGroups)
            {
                if (CheckMonopoly(player, group.Key))
                {
                    result.AchievedMonopoly = true;
                    result.Message += $"\n{group.Key.Name}��Ɛ肵�܂����I";

                    MonopolyAchieved?.Invoke(this, new MonopolyAchievedEventArgs
                    {
                        Player = player,
                        Station = group.Key
                    });
                }
            }

            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// �����𔄋p
        /// </summary>
        public async Task<PropertyTransactionResult> SellPropertyAsync(Player player, Property property)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var result = new PropertyTransactionResult
            {
                TransactionType = TransactionType.Sale
            };

            // ���L�`�F�b�N
            if (property.Owner != player)
            {
                result.Success = false;
                result.Message = "���̕��������L���Ă��܂���B";
                return result;
            }

            // ���p���i�v�Z�i�w�����i��70%�j
            var sellPrice = property.GetSellPrice();

            // ���p����
            property.Owner = null;
            player.OwnedProperties.Remove(property);
            player.Receive(sellPrice);

            result.Success = true;
            result.Properties.Add(property);
            result.TotalAmount = sellPrice;
            result.Message = $"{property.Name}��{sellPrice}�Ŕ��p���܂����B";

            // �C�x���g����
            PropertySold?.Invoke(this, new PropertyTransactionEventArgs
            {
                Player = player,
                Property = property,
                Amount = sellPrice
            });

            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// �Ɛ�`�F�b�N
        /// </summary>
        public bool CheckMonopoly(Player player, Station station)
        {
            if (player == null || station == null)
                return false;

            // �����w�łȂ��ꍇ�͓Ɛ�s��
            if (station.Properties.Count == 0)
                return false;

            // �S�Ă̕��������L���Ă��邩�`�F�b�N
            return station.Properties.All(p => p.Owner == player);
        }

        /// <summary>
        /// �������v���v�Z
        /// </summary>
        public Money CalculateIncome(Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            Money totalIncome = Money.Zero;

            // �w���ƂɃO���[�v��
            var stationGroups = player.OwnedProperties.GroupBy(p => p.Location);

            foreach (var group in stationGroups)
            {
                var station = group.Key;
                var isMonopoly = CheckMonopoly(player, station);

                foreach (var property in group)
                {
                    // �Ɛ�{�[�i�X��K�p
                    property.HasMonopolyBonus = isMonopoly;
                    var income = property.CalculateIncome();
                    totalIncome += income;
                }
            }

            return totalIncome;
        }

        /// <summary>
        /// �������l���X�V
        /// </summary>
        public void UpdatePropertyValues(PropertyMarket market)
        {
            if (market == null)
                throw new ArgumentNullException(nameof(market));

            // �S�����̉��l���s��󋵂ɉ����čX�V
            // �i���ۂ̃Q�[���ł́A�e�w�̕������X�g���擾���čX�V����j
        }

        /// <summary>
        /// �v���C���[�̕������v���擾
        /// </summary>
        public PropertyStatistics GetPropertyStatistics(Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            var stats = new PropertyStatistics
            {
                TotalCount = player.OwnedProperties.Count,
                TotalValue = player.OwnedProperties.Aggregate(Money.Zero, (sum, p) => sum + p.CurrentPrice)
            };

            // �J�e�S���ʏW�v
            var categoryGroups = player.OwnedProperties.GroupBy(p => p.Category);
            foreach (var group in categoryGroups)
            {
                stats.CountByCategory[group.Key] = group.Count();
                stats.ValueByCategory[group.Key] = group.Aggregate(Money.Zero, (sum, p) => sum + p.CurrentPrice);
            }

            // �n��ʏW�v
            var regionGroups = player.OwnedProperties.GroupBy(p => p.Location.Region);
            foreach (var group in regionGroups)
            {
                stats.CountByRegion[group.Key] = group.Count();
            }

            // �Ɛ�w��
            var stationGroups = player.OwnedProperties.GroupBy(p => p.Location);
            stats.MonopolyStationCount = stationGroups.Count(g => CheckMonopoly(player, g.Key));

            // �ō��z����
            stats.MostExpensiveProperty = player.OwnedProperties.OrderByDescending(p => p.CurrentPrice).FirstOrDefault();

            return stats;
        }
    }

    /// <summary>
    /// �����������
    /// </summary>
    public class PropertyTransactionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TransactionType TransactionType { get; set; }
        public List<Property> Properties { get; set; } = new List<Property>();
        public Money TotalAmount { get; set; }
        public Money? RequiredMoney { get; set; }
        public Money? ShortAmount { get; set; }
        public bool AchievedMonopoly { get; set; }
    }

    /// <summary>
    /// ����^�C�v
    /// </summary>
    public enum TransactionType
    {
        Purchase,
        Sale
    }

    /// <summary>
    /// �������v
    /// </summary>
    public class PropertyStatistics
    {
        public int TotalCount { get; set; }
        public Money TotalValue { get; set; }
        public Dictionary<PropertyCategory, int> CountByCategory { get; set; } = new Dictionary<PropertyCategory, int>();
        public Dictionary<PropertyCategory, Money> ValueByCategory { get; set; } = new Dictionary<PropertyCategory, Money>();
        public Dictionary<Region, int> CountByRegion { get; set; } = new Dictionary<Region, int>();
        public int MonopolyStationCount { get; set; }
        public Property? MostExpensiveProperty { get; set; }
    }

    /// <summary>
    /// ��������C�x���g����
    /// </summary>
    public class PropertyTransactionEventArgs : EventArgs
    {
        public Player Player { get; set; } = null!;
        public Property Property { get; set; } = null!;
        public Money Amount { get; set; }
    }

    /// <summary>
    /// �Ɛ�B���C�x���g����
    /// </summary>
    public class MonopolyAchievedEventArgs : EventArgs
    {
        public Player Player { get; set; } = null!;
        public Station Station { get; set; } = null!;
    }
}