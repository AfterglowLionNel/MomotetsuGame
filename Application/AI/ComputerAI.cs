using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.Enums;
using MomotetsuGame.Core.Interfaces;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Application.Services;
using SkiaSharp;

namespace MomotetsuGame.Application.AI
{
    /// <summary>
    /// �R���s���[�^AI�̎���
    /// </summary>
    public class ComputerAI : IComputerAI
    {
        private readonly IRouteCalculator _routeCalculator;
        private readonly IAIStrategy _strategy;
        private readonly Random _random;

        /// <summary>
        /// AI��Փx
        /// </summary>
        public ComDifficulty Difficulty { get; set; }

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public ComputerAI(
            IRouteCalculator routeCalculator,
            ComDifficulty difficulty = ComDifficulty.Normal,
            IAIStrategy? strategy = null)
        {
            _routeCalculator = routeCalculator ?? throw new ArgumentNullException(nameof(routeCalculator));
            Difficulty = difficulty;
            _strategy = strategy ?? CreateDefaultStrategy(difficulty);
            _random = new Random();
        }

        /// <summary>
        /// �s��������
        /// </summary>
        public async Task<ActionDecision> DecideAction(AIContext context, Player player)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (player == null) throw new ArgumentNullException(nameof(player));

            // �󋵕���
            var analysis = AnalyzeGameState(context, player);

            // �J�[�h�g�p����
            if (player.Cards.Any() && ShouldUseCard(analysis, player))
            {
                var cardToUse = await SelectCardToUse(player.Cards, player, context);
                if (cardToUse != null)
                {
                    return new ActionDecision
                    {
                        Type = ActionType.UseCard,
                        SelectedCard = cardToUse,
                        Priority = 80,
                        Reason = "�헪�I�ɃJ�[�h���g�p"
                    };
                }
            }

            // �ʏ�̓T�C�R����U��
            return new ActionDecision
            {
                Type = ActionType.RollDice,
                Priority = 100,
                Reason = "�ʏ�ړ�"
            };
        }

        /// <summary>
        /// ����_�ł̕�����I��
        /// </summary>
        public async Task<Station> SelectDestination(List<Station> options, Player player, AIContext context)
        {
            if (options == null || options.Count == 0)
                throw new ArgumentException("�I���\�ȉw������܂���B", nameof(options));

            var evaluations = new List<(Station station, double score)>();

            foreach (var option in options)
            {
                var score = EvaluateRoute(option, player, context);
                evaluations.Add((option, score));
            }

            // ��Փx�ɂ��I��
            switch (Difficulty)
            {
                case ComDifficulty.Weak:
                    // �アAI�̓����_����������
                    if (_random.Next(100) < 50)
                    {
                        return options[_random.Next(options.Count)];
                    }
                    break;

                case ComDifficulty.Strong:
                    // ����AI�͏�ɍœK�ȑI��
                    return evaluations.OrderByDescending(e => e.score).First().station;
            }

            // �ʏ�͏�ʂ̒����烉���_���ɑI��
            var topChoices = evaluations
                .OrderByDescending(e => e.score)
                .Take(Math.Max(2, options.Count / 2))
                .ToList();

            var selected = topChoices[_random.Next(topChoices.Count)].station;

            await Task.CompletedTask;
            return selected;
        }

        /// <summary>
        /// �w�����镨����I��
        /// </summary>
        public async Task<List<Property>> SelectPropertiesToBuy(List<Property> available, Player player, AIContext context)
        {
            if (available == null || available.Count == 0)
                return new List<Property>();

            var selected = new List<Property>();
            var remainingMoney = player.CurrentMoney;
            var analysis = AnalyzeGameState(context, player);

            // ������]�����ă\�[�g
            var evaluated = available
                .Select(p => new
                {
                    Property = p,
                    Score = _strategy.EvaluateProperty(p, analysis)
                })
                .OrderByDescending(e => e.Score)
                .ToList();

            // ��Փx�ɂ�钲��
            var purchaseThreshold = Difficulty switch
            {
                ComDifficulty.Weak => 0.3,    // 30%�̊m���ōw��
                ComDifficulty.Normal => 0.6,  // 60%�̊m���ōw��
                ComDifficulty.Strong => 0.9,  // 90%�̊m���ōw��
                _ => 0.6
            };

            // �w������
            foreach (var item in evaluated)
            {
                if (item.Score < 50) break; // �X�R�A���Ⴂ�����͍w�����Ȃ�

                if (_random.NextDouble() < purchaseThreshold)
                {
                    if (remainingMoney >= item.Property.CurrentPrice)
                    {
                        selected.Add(item.Property);
                        remainingMoney -= item.Property.CurrentPrice;

                        // �Ɛ�\���`�F�b�N
                        if (WouldAchieveMonopoly(player, item.Property))
                        {
                            // �Ɛ�̂��߂ɓ����w�̑��̕������w��
                            var stationProperties = available
                                .Where(p => p.Location == item.Property.Location && !selected.Contains(p))
                                .ToList();

                            foreach (var prop in stationProperties)
                            {
                                if (remainingMoney >= prop.CurrentPrice)
                                {
                                    selected.Add(prop);
                                    remainingMoney -= prop.CurrentPrice;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            await Task.CompletedTask;
            return selected;
        }

        /// <summary>
        /// �g�p����J�[�h��I��
        /// </summary>
        public async Task<Card?> SelectCardToUse(List<Card> available, Player player, AIContext context)
        {
            if (available == null || available.Count == 0)
                return null;

            var analysis = AnalyzeGameState(context, player);
            var usableCards = available.Where(c => c.CanUse(player, context.GameState)).ToList();

            if (!usableCards.Any())
                return null;

            // �J�[�h��]��
            var evaluated = usableCards
                .Select(c => new
                {
                    Card = c,
                    Score = _strategy.EvaluateCard(c, analysis)
                })
                .OrderByDescending(e => e.Score)
                .ToList();

            // �ō��X�R�A�̃J�[�h��I���i��Փx�ɂ�钲������j
            var bestCard = evaluated.FirstOrDefault();
            if (bestCard != null && bestCard.Score > 60)
            {
                if (Difficulty == ComDifficulty.Weak && _random.Next(100) < 30)
                {
                    // �アAI��30%�̊m���Ŏg�p����߂�
                    return null;
                }

                await Task.CompletedTask;
                return bestCard.Card;
            }

            return null;
        }

        /// <summary>
        /// �J�[�h�����ōw������J�[�h��I��
        /// </summary>
        public async Task<Card?> SelectCardToBuy(List<Card> available, Player player, AIContext context)
        {
            if (available == null || available.Count == 0)
                return null;

            var analysis = AnalyzeGameState(context, player);

            // �����J�[�h���`�F�b�N
            if (player.Cards.Count >= player.MaxCardCount)
                return null;

            // �J�[�h��]��
            var evaluated = available
                .Where(c => player.CurrentMoney >= c.BasePrice)
                .Select(c => new
                {
                    Card = c,
                    Score = _strategy.EvaluateCard(c, analysis)
                })
                .OrderByDescending(e => e.Score)
                .ToList();

            var bestCard = evaluated.FirstOrDefault();
            if (bestCard != null && bestCard.Score > 70)
            {
                await Task.CompletedTask;
                return bestCard.Card;
            }

            return null;
        }

        /// <summary>
        /// ���p���镨����I���i�����s�����j
        /// </summary>
        public async Task<List<Property>> SelectPropertiesToSell(List<Property> properties, Money targetAmount, Player player)
        {
            if (properties == null || properties.Count == 0)
                return new List<Property>();

            var toSell = new List<Property>();
            var currentAmount = Money.Zero;

            // ���v���̒Ⴂ�������甄�p
            var sorted = properties
                .OrderBy(p => p.IncomeRate)
                .ThenBy(p => p.CurrentPrice)
                .ToList();

            foreach (var property in sorted)
            {
                if (currentAmount >= targetAmount)
                    break;

                // �Ɛ������Ȃ��悤�Ƀ`�F�b�N
                if (!WouldBreakMonopoly(player, property))
                {
                    toSell.Add(property);
                    currentAmount += property.GetSellPrice();
                }
            }

            // �K�v�z�ɒB���Ȃ��ꍇ�͓Ɛ������
            if (currentAmount < targetAmount)
            {
                foreach (var property in sorted.Where(p => !toSell.Contains(p)))
                {
                    if (currentAmount >= targetAmount)
                        break;

                    toSell.Add(property);
                    currentAmount += property.GetSellPrice();
                }
            }

            await Task.CompletedTask;
            return toSell;
        }

        #region Private Methods

        /// <summary>
        /// �Q�[����Ԃ𕪐�
        /// </summary>
        private AIAnalysis AnalyzeGameState(AIContext context, Player player)
        {
            var analysis = new AIAnalysis
            {
                PlayerRank = player.Rank,
                DistanceToDestination = _routeCalculator.GetDistanceToDestination(
                    player.CurrentStation!,
                    context.Destination),
                TurnProgress = context.GameProgress,
                HasBonby = player.AttachedBonby != null
            };

            // �����Y�䗦���v�Z
            var totalAssets = context.AllPlayers.Sum(p => p.TotalAssets.Value);
            if (totalAssets > 0)
            {
                analysis.MoneyRatio = (double)player.TotalAssets.Value / totalAssets;
            }

            // 1�ʂƂ̍����v�Z
            var firstPlayer = context.AllPlayers.OrderByDescending(p => p.TotalAssets).First();
            if (firstPlayer != player)
            {
                analysis.TopPlayerLead = firstPlayer.TotalAssets - player.TotalAssets;
            }

            // �Ɛ�\�w�����o
            var stations = context.StationNetwork.GetAllStations();
            foreach (var station in stations.Where(s => s.Properties.Any()))
            {
                var ownedCount = station.Properties.Count(p => p.Owner == player);
                var totalCount = station.Properties.Count;

                if (ownedCount > 0 && ownedCount < totalCount)
                {
                    analysis.MonopolizableStations.Add(station);
                }
            }

            // ���Ѓv���C���[�����
            analysis.ThreateningPlayers = context.AllPlayers
                .Where(p => p != player && p.TotalAssets > player.TotalAssets)
                .OrderByDescending(p => p.TotalAssets)
                .ToList();

            return analysis;
        }

        /// <summary>
        /// �J�[�h�g�p���ׂ�������
        /// </summary>
        private bool ShouldUseCard(AIAnalysis analysis, Player player)
        {
            // �ړI�n���߂��ꍇ
            if (analysis.DistanceToDestination <= 6)
                return true;

            // �򐨂̏ꍇ
            if (analysis.PlayerRank > 2 && analysis.TurnProgress > 0.5)
                return true;

            // �{���r�[���t���Ă���ꍇ
            if (analysis.HasBonby)
                return true;

            // ��D���̏ꍇ
            if (player.Status == PlayerStatus.SuperLucky)
                return true;

            return false;
        }

        /// <summary>
        /// ���[�g��]��
        /// </summary>
        private double EvaluateRoute(Station destination, Player player, AIContext context)
        {
            var score = 100.0;

            // �ړI�n�ւ̋���
            var distanceToGoal = _routeCalculator.GetDistanceToDestination(
                destination,
                context.Destination);
            score -= distanceToGoal * 10;

            // �w�̎�ނɂ��]��
            switch (destination.Type)
            {
                case StationType.Property:
                    if (destination.GetAvailablePropertyCount() > 0)
                        score += 30;
                    if (destination.GetPropertyCountOwnedBy(player) > 0)
                        score += 20; // ���ɕ����������Ă���w�͓Ɛ�̃`�����X
                    break;

                case StationType.Plus:
                    score += 40;
                    break;

                case StationType.Minus:
                    score -= 40;
                    break;

                case StationType.CardShop:
                    if (player.Cards.Count < player.MaxCardCount)
                        score += 25;
                    break;
            }

            return score;
        }

        /// <summary>
        /// �Ɛ��B�����邩����
        /// </summary>
        private bool WouldAchieveMonopoly(Player player, Property property)
        {
            var station = property.Location;
            var ownedCount = station.Properties.Count(p => p.Owner == player);
            var totalCount = station.Properties.Count;

            return ownedCount + 1 == totalCount;
        }

        /// <summary>
        /// �Ɛ�����������
        /// </summary>
        private bool WouldBreakMonopoly(Player player, Property property)
        {
            var station = property.Location;
            return station.IsMonopolizedBy(player);
        }

        /// <summary>
        /// �f�t�H���g�헪���쐬
        /// </summary>
        private IAIStrategy CreateDefaultStrategy(ComDifficulty difficulty)
        {
            return difficulty switch
            {
                ComDifficulty.Weak => new ConservativeAIStrategy(),
                ComDifficulty.Strong => new AggressiveAIStrategy(),
                _ => new BalancedAIStrategy()
            };
        }

        #endregion
    }
}