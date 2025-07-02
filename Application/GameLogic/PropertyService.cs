using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Application.GameLogic
{
    /// <summary>
    /// �����Ǘ��T�[�r�X
    /// </summary>
    public class PropertyService
    {
        private readonly PropertyMarket _propertyMarket;
        private readonly Random _random;

        public PropertyService(PropertyMarket propertyMarket)
        {
            _propertyMarket = propertyMarket ?? throw new ArgumentNullException(nameof(propertyMarket));
            _random = new Random();
        }

        /// <summary>
        /// �������w��
        /// </summary>
        public async Task<bool> PurchasePropertyAsync(Player player, Property property)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (property == null) throw new ArgumentNullException(nameof(property));

            // �w���\�`�F�b�N
            if (!CanPurchase(player, property))
            {
                return false;
            }

            // �w������
            return await Task.Run(() => player.PurchaseProperty(property));
        }

        /// <summary>
        /// �����������ꊇ�w��
        /// </summary>
        public async Task<PurchaseResult> PurchasePropertiesAsync(Player player, List<Property> properties)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            var result = new PurchaseResult
            {
                RequestedProperties = new List<Property>(properties),
                PurchasedProperties = new List<Property>(),
                FailedProperties = new List<Property>(),
                TotalCost = Money.Zero
            };

            // �w���������œK���i�������j
            var sortedProperties = properties.OrderBy(p => p.BasePrice.Value).ToList();

            foreach (var property in sortedProperties)
            {
                if (await PurchasePropertyAsync(player, property))
                {
                    result.PurchasedProperties.Add(property);
                    result.TotalCost += property.BasePrice;
                }
                else
                {
                    result.FailedProperties.Add(property);
                }
            }

            result.Success = result.PurchasedProperties.Count > 0;
            return result;
        }

        /// <summary>
        /// �����𔄋p
        /// </summary>
        public async Task<bool> SellPropertyAsync(Player player, Property property)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (property == null) throw new ArgumentNullException(nameof(property));

            return await Task.Run(() => player.SellProperty(property));
        }

        /// <summary>
        /// �w���\���`�F�b�N
        /// </summary>
        public bool CanPurchase(Player player, Property property)
        {
            // ���L�҃`�F�b�N
            if (property.Owner != null)
                return false;

            // �����`�F�b�N
            if (player.CurrentMoney < property.BasePrice)
                return false;

            return true;
        }

        /// <summary>
        /// �Ɛ�`�F�b�N
        /// </summary>
        public bool CheckMonopoly(Player player, Station station)
        {
            if (player == null || station == null) return false;
            if (station.Properties.Count == 0) return false;

            return station.IsMonopolizedBy(player);
        }

        /// <summary>
        /// �w�̓Ɛ藦���v�Z
        /// </summary>
        public double CalculateMonopolyRate(Player player, Station station)
        {
            if (player == null || station == null) return 0;
            if (station.Properties.Count == 0) return 0;

            var ownedCount = station.Properties.Count(p => p.Owner == player);
            return (double)ownedCount / station.Properties.Count;
        }

        /// <summary>
        /// �������l���X�V�i���������j
        /// </summary>
        public void UpdatePropertyValues()
        {
            _propertyMarket.UpdateMarket();

            // ���ׂĂ̕����̉��l���X�V
            foreach (var property in GetAllProperties())
            {
                var changeRate = _propertyMarket.CalculatePriceChange(property);
                property.UpdatePrice(changeRate);
            }
        }

        /// <summary>
        /// ���ʃC�x���g�ɂ�镨�����l�ϓ�
        /// </summary>
        public void ApplySpecialEvent(PropertyEventType eventType, PropertyCategory? targetCategory = null)
        {
            var properties = GetAllProperties();

            if (targetCategory.HasValue)
            {
                properties = properties.Where(p => p.Category == targetCategory.Value).ToList();
            }

            foreach (var property in properties)
            {
                var changeRate = eventType switch
                {
                    PropertyEventType.Boom => 0.3m,          // +30%
                    PropertyEventType.Recession => -0.3m,    // -30%
                    PropertyEventType.Disaster => -0.5m,     // -50%
                    PropertyEventType.Development => 0.5m,   // +50%
                    _ => 0m
                };

                property.UpdatePrice(changeRate);
            }
        }

        /// <summary>
        /// �����̃A�b�v�O���[�h
        /// </summary>
        public async Task<UpgradeResult> UpgradePropertyAsync(Player player, Property property)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (property == null) throw new ArgumentNullException(nameof(property));

            var result = new UpgradeResult
            {
                Property = property,
                Success = false
            };

            // ���L�҃`�F�b�N
            if (property.Owner != player)
            {
                result.Message = "�����̕����ł͂���܂���B";
                return result;
            }

            // �A�b�v�O���[�h�\�`�F�b�N
            if (property.TryUpgrade(out var cost))
            {
                // �����`�F�b�N
                if (player.CurrentMoney >= cost)
                {
                    player.CurrentMoney -= cost;
                    result.Success = true;
                    result.Cost = cost;
                    result.Message = $"{property.Name}���A�b�v�O���[�h���܂����I";
                }
                else
                {
                    result.Message = "�������s�����Ă��܂��B";
                }
            }
            else
            {
                result.Message = "����ȏ�A�b�v�O���[�h�ł��܂���B";
            }

            return result;
        }

        /// <summary>
        /// �����������擾�iAI/�q���g�p�j
        /// </summary>
        public List<Property> GetRecommendedProperties(Station station, Player player)
        {
            if (station == null || player == null) return new List<Property>();

            var availableProperties = station.GetAvailableProperties();
            var recommendations = new List<(Property property, double score)>();

            foreach (var property in availableProperties)
            {
                double score = CalculatePropertyScore(property, player, station);
                recommendations.Add((property, score));
            }

            return recommendations
                .OrderByDescending(r => r.score)
                .Select(r => r.property)
                .ToList();
        }

        /// <summary>
        /// �����X�R�A���v�Z
        /// </summary>
        private double CalculatePropertyScore(Property property, Player player, Station station)
        {
            double score = 0;

            // ���v���X�R�A
            score += property.IncomeRate * 100;

            // ���i�����X�R�A
            var priceEfficiency = (double)property.IncomeRate / (property.BasePrice.Value / 1000000000.0);
            score += priceEfficiency * 50;

            // �Ɛ�\���X�R�A
            var monopolyRate = CalculateMonopolyRate(player, station);
            if (monopolyRate > 0.5)
            {
                score += monopolyRate * 100;
            }

            // �J�e�S���{�[�i�X
            score += property.Category switch
            {
                PropertyCategory.Tourism => 20,     // �ό������͍��]��
                PropertyCategory.Industry => 15,    // �H�ƕ��������]��
                PropertyCategory.Commerce => 10,    // ���ƕ����͒��]��
                PropertyCategory.Fishery => 5,      // ���Y�����͒�]��
                PropertyCategory.Agriculture => 0,  // �_�ѕ����͊
                _ => 0
            };

            // �s��g�����h���l��
            var marketTrend = _propertyMarket.CategoryTrends.GetValueOrDefault(property.Category, 0m);
            score += (double)marketTrend * 50;

            // �����ɑ΂���w���\��
            var affordabilityRatio = (double)player.CurrentMoney.Value / property.BasePrice.Value;
            if (affordabilityRatio < 2.0)
            {
                score -= 20; // �������̔����ȏ�͐T�d��
            }

            return score;
        }

        /// <summary>
        /// ���ׂĂ̕������擾�i�����p�j
        /// </summary>
        private List<Property> GetAllProperties()
        {
            // ���ۂ̎����ł́AStationNetwork����S�w�̕������擾
            // �����ł͉�����
            return new List<Property>();
        }
    }

    /// <summary>
    /// �����w������
    /// </summary>
    public class PurchaseResult
    {
        public bool Success { get; set; }
        public List<Property> RequestedProperties { get; set; } = new List<Property>();
        public List<Property> PurchasedProperties { get; set; } = new List<Property>();
        public List<Property> FailedProperties { get; set; } = new List<Property>();
        public Money TotalCost { get; set; } = Money.Zero;
    }

    /// <summary>
    /// �����A�b�v�O���[�h����
    /// </summary>
    public class UpgradeResult
    {
        public bool Success { get; set; }
        public Property Property { get; set; } = null!;
        public Money Cost { get; set; } = Money.Zero;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// �����C�x���g�^�C�v
    /// </summary>
    public enum PropertyEventType
    {
        Boom,         // �D�i�C
        Recession,    // �s�i�C
        Disaster,     // �ЊQ
        Development   // �J��
    }

    /// <summary>
    /// �����]���T�[�r�X�iAI�p�j
    /// </summary>
    public class PropertyEvaluator
    {
        /// <summary>
        /// �����̓������l��]��
        /// </summary>
        public double EvaluateInvestmentValue(Property property, Player player, GameState gameState)
        {
            double value = 0;

            // ��{�I�Ȏ��v��
            var annualIncome = property.CalculateIncome();
            var paybackPeriod = property.CurrentPrice.Value / (double)annualIncome.Value;
            value += 100 / paybackPeriod; // ������Ԃ��Z���قǍ��]��

            // �Q�[���c����Ԃ��l��
            var remainingYears = gameState.MaxYears - gameState.CurrentYear;
            if (paybackPeriod > remainingYears)
            {
                value *= 0.5; // ����ł��Ȃ��ꍇ�͕]����������
            }

            // �Ɛ�ɂ����v�{�����l��
            if (property.Location != null)
            {
                var monopolyPotential = CalculateMonopolyPotential(player, property.Location);
                value *= (1 + monopolyPotential);
            }

            // �A�b�v�O���[�h�\��
            if (property.UpgradeLevel < 3)
            {
                value *= 1.2; // �A�b�v�O���[�h�]�n������Ε]���A�b�v
            }

            return value;
        }

        /// <summary>
        /// �Ɛ�\�����v�Z
        /// </summary>
        private double CalculateMonopolyPotential(Player player, Station station)
        {
            var totalProperties = station.Properties.Count;
            var ownedProperties = station.Properties.Count(p => p.Owner == player);
            var availableProperties = station.Properties.Count(p => p.Owner == null);

            if (totalProperties == 0) return 0;

            // ���ɓƐ肵�Ă���ꍇ
            if (ownedProperties == totalProperties) return 1.0;

            // �Ɛ�\�ȏꍇ�i���v���C���[�����L���Ă��Ȃ��j
            if (ownedProperties + availableProperties == totalProperties)
            {
                return (double)ownedProperties / totalProperties;
            }

            // �Ɛ�s�\�ȏꍇ
            return 0;
        }

        /// <summary>
        /// ���p�����x��]��
        /// </summary>
        public double EvaluateSellRecommendation(Property property, Player player, Money targetAmount)
        {
            double score = 0;

            // ���p���i�ƍw�����i�̔䗦
            var sellPrice = property.GetSellPrice();
            var lossRatio = 1.0 - ((double)sellPrice.Value / property.CurrentPrice.Value);
            score -= lossRatio * 100; // �������傫���قǒ�]��

            // ���v�����Ⴂ�����͔��p����
            if (property.IncomeRate < 0.08m) // 8%����
            {
                score += 20;
            }

            // �Ɛ������ꍇ�͔񐄏�
            if (property.HasMonopolyBonus)
            {
                score -= 50;
            }

            // �ڕW�z�ɑ΂���v���x
            var contributionRatio = (double)sellPrice.Value / targetAmount.Value;
            score += contributionRatio * 30;

            return score;
        }
    }
}