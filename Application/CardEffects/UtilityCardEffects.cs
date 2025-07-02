using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.Interfaces;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Application.CardEffects
{
    /// <summary>
    /// ダビングカード効果（カード複製+即再行動）
    /// </summary>
    public class DuplicateCardEffect : CardEffectBase
    {
        public override string EffectType => "Duplicate";

        public override async Task<bool> CanExecute(CardEffectContext context)
        {
            if (!await base.CanExecute(context))
                return false;

            // 他にカードを持っているかチェック
            return context.Player.Cards.Count > 1;
        }

        public override async Task<CardEffectResult> Execute(CardEffectContext context)
        {
            if (context.EventBus == null)
                return CardEffectResult.CreateFailure("必要なサービスが設定されていません。");

            var eventBus = (IEventBus)context.EventBus;
            var player = context.Player;

            // 複製対象のカードを選択（自分自身を除く）
            var selectableCards = player.Cards.Where(c => c.Id != context.Card.Id).ToList();
            if (!selectableCards.Any())
                return CardEffectResult.CreateFailure("複製できるカードがありません。");

            // カード選択イベントを発火
            var selectionEvent = new CardSelectionRequestEvent
            {
                Player = player,
                SelectableCards = selectableCards,
                Message = "複製するカードを選んでください",
                SelectionType = SelectionType.Single
            };

            eventBus.Publish(selectionEvent);

            // 選択結果を待つ（実際にはイベント駆動で処理される）
            if (!context.Parameters.TryGetValue("SelectedCard", out var selectedObj) ||
                selectedObj is not Card selectedCard)
            {
                return CardEffectResult.CreateFailure("カードが選択されませんでした。");
            }

            // カードを複製
            var duplicatedCard = selectedCard.Clone();

            // 所持数チェック
            if (player.Cards.Count >= player.MaxCardCount)
            {
                return CardEffectResult.CreateFailure("カードの所持数が上限に達しています。");
            }

            player.AddCard(duplicatedCard);

            var result = CardEffectResult.CreateSuccess($"{selectedCard.Name}を複製しました！");
            result.AllowContinuation = true; // 即座に行動可能

            await Task.CompletedTask;
            return result;
        }

        public override string GetDescription()
        {
            return "手持ちのカードを1枚選んで複製し、すぐに行動できます。";
        }
    }

    /// <summary>
    /// シンデレラカード効果（物件1件無料取得）
    /// </summary>
    public class CinderellaCardEffect : CardEffectBase
    {
        public override string EffectType => "Cinderella";

        public override async Task<bool> CanExecute(CardEffectContext context)
        {
            if (!await base.CanExecute(context))
                return false;

            // 12月は使用不可（消滅）
            return context.GameState.CurrentMonth != 12;
        }

        public override async Task<CardEffectResult> Execute(CardEffectContext context)
        {
            if (context.EventBus == null)
                return CardEffectResult.CreateFailure("必要なサービスが設定されていません。");

            var eventBus = (IEventBus)context.EventBus;

            // 物件選択イベントを発火
            var selectionEvent = new PropertySelectionRequestEvent
            {
                Player = context.Player,
                Message = "無料で手に入れる物件を選んでください",
                SelectionType = SelectionType.Single,
                FreeSelection = true // 無料フラグ
            };

            eventBus.Publish(selectionEvent);

            // 選択結果を待つ
            if (!context.Parameters.TryGetValue("SelectedProperty", out var selectedObj) ||
                selectedObj is not Property selectedProperty)
            {
                return CardEffectResult.CreateFailure("物件が選択されませんでした。");
            }

            // 所有者チェック
            if (selectedProperty.Owner != null)
            {
                return CardEffectResult.CreateFailure("この物件は既に所有されています。");
            }

            // 無料で物件を取得
            selectedProperty.Owner = context.Player;
            context.Player.OwnedProperties.Add(selectedProperty);

            // 独占チェック
            var station = selectedProperty.Location;
            var isMonopoly = station.Properties.All(p => p.Owner == context.Player);

            var message = $"{selectedProperty.Name}を無料で手に入れました！";
            if (isMonopoly)
            {
                message += $"\n{station.Name}を独占しました！";
            }

            await Task.CompletedTask;
            return CardEffectResult.CreateSuccess(message);
        }

        public override string GetDescription()
        {
            return "好きな物件を1件無料で手に入れます。（12月になると消滅）";
        }
    }

    /// <summary>
    /// ゴールドカード効果（現在駅全物件10%で購入）
    /// </summary>
    public class GoldCardEffect : CardEffectBase
    {
        public override string EffectType => "Gold";

        public override async Task<bool> CanExecute(CardEffectContext context)
        {
            if (!await base.CanExecute(context))
                return false;

            var station = context.Player.CurrentStation;
            if (station == null || station.Type != StationType.Property)
                return false;

            // 購入可能な物件があるかチェック
            return station.Properties.Any(p => p.Owner == null);
        }

        public override async Task<CardEffectResult> Execute(CardEffectContext context)
        {
            var station = context.Player.CurrentStation;
            if (station == null)
                return CardEffectResult.CreateFailure("現在位置が不明です。");

            var availableProperties = station.Properties.Where(p => p.Owner == null).ToList();
            if (!availableProperties.Any())
                return CardEffectResult.CreateFailure("購入可能な物件がありません。");

            // 全物件の10%の価格を計算
            var totalOriginalPrice = availableProperties.Aggregate(Money.Zero, (sum, p) => sum + p.CurrentPrice);
            var discountedPrice = totalOriginalPrice * 0.1m;

            // 資金チェック
            if (context.Player.CurrentMoney < discountedPrice)
            {
                return CardEffectResult.CreateFailure($"資金が不足しています。必要額: {discountedPrice}");
            }

            // 購入処理
            context.Player.Pay(discountedPrice);
            foreach (var property in availableProperties)
            {
                property.Owner = context.Player;
                context.Player.OwnedProperties.Add(property);
            }

            var message = $"{station.Name}の全物件を{discountedPrice}（通常の10%）で購入しました！";

            // 独占達成
            if (station.Properties.All(p => p.Owner == context.Player))
            {
                message += "\n独占を達成しました！";
            }

            await Task.CompletedTask;
            return CardEffectResult.CreateSuccess(message);
        }

        public override string GetDescription()
        {
            return "今いる駅の全物件を10%の価格で購入できます。";
        }
    }

    /// <summary>
    /// 刀狩りカード効果（他プレイヤーからカード1枚奪取）
    /// </summary>
    public class StealCardEffect : CardEffectBase
    {
        public override string EffectType => "StealCard";

        public override async Task<bool> CanExecute(CardEffectContext context)
        {
            if (!await base.CanExecute(context))
                return false;

            // 他プレイヤーがカードを持っているかチェック
            return context.GameState.Players
                .Where(p => p.Id != context.Player.Id && p.Cards.Any())
                .Any();
        }

        public override async Task<CardEffectResult> Execute(CardEffectContext context)
        {
            if (context.EventBus == null)
                return CardEffectResult.CreateFailure("必要なサービスが設定されていません。");

            var eventBus = (IEventBus)context.EventBus;

            // 対象プレイヤーを選択
            var targetPlayers = context.GameState.Players
                .Where(p => p.Id != context.Player.Id && p.Cards.Any())
                .ToList();

            if (!targetPlayers.Any())
                return CardEffectResult.CreateFailure("カードを持っているプレイヤーがいません。");

            // プレイヤー選択イベントを発火
            var playerSelectionEvent = new PlayerSelectionRequestEvent
            {
                SelectablePlayers = targetPlayers,
                Message = "カードを奪うプレイヤーを選んでください",
                SelectionType = SelectionType.Single
            };

            eventBus.Publish(playerSelectionEvent);

            // 選択結果を待つ
            if (!context.Parameters.TryGetValue("SelectedPlayer", out var playerObj) ||
                playerObj is not Player targetPlayer)
            {
                return CardEffectResult.CreateFailure("プレイヤーが選択されませんでした。");
            }

            // ランダムにカードを1枚選択
            var random = new Random();
            var stolenCard = targetPlayer.Cards[random.Next(targetPlayer.Cards.Count)];

            // カードを移動
            targetPlayer.Cards.Remove(stolenCard);

            if (context.Player.Cards.Count >= context.Player.MaxCardCount)
            {
                return CardEffectResult.CreateFailure("カードの所持数が上限に達しています。");
            }

            context.Player.AddCard(stolenCard);

            await Task.CompletedTask;
            return CardEffectResult.CreateSuccess($"{targetPlayer.Name}から{stolenCard.Name}を奪いました！");
        }

        public override string GetDescription()
        {
            return "他のプレイヤーからカードを1枚奪います。";
        }
    }

    /// <summary>
    /// カードバンクカード効果（カードバンクアクセス）
    /// </summary>
    public class CardBankCardEffect : CardEffectBase
    {
        public override string EffectType => "CardBank";

        public override async Task<bool> CanExecute(CardEffectContext context)
        {
            if (!await base.CanExecute(context))
                return false;

            // 16年目以降のみ使用可能
            return context.GameState.CurrentYear >= 16;
        }

        public override async Task<CardEffectResult> Execute(CardEffectContext context)
        {
            if (context.EventBus == null)
                return CardEffectResult.CreateFailure("必要なサービスが設定されていません。");

            var eventBus = (IEventBus)context.EventBus;

            // カードバンクアクセスイベントを発火
            eventBus.Publish(new CardBankAccessEvent
            {
                Player = context.Player
            });

            await Task.CompletedTask;
            return CardEffectResult.CreateSuccess("カードバンクにアクセスしました。カードの預け入れ・引き出しができます。");
        }

        public override string GetDescription()
        {
            return "カードバンクにアクセスし、カードの預け入れ・引き出しができます。（16年目以降）";
        }
    }

    /// <summary>
    /// 物件飛びカード効果（物件駅のみ移動）
    /// </summary>
    public class PropertyJumpCardEffect : CardEffectBase
    {
        public override string EffectType => "PropertyJump";

        public override async Task<CardEffectResult> Execute(CardEffectContext context)
        {
            if (context.DiceService == null || context.GameManager == null)
                return CardEffectResult.CreateFailure("必要なサービスが設定されていません。");

            var diceService = (Services.IDiceService)context.DiceService;
            var routeCalculator = context.Parameters["RouteCalculator"] as Services.IRouteCalculator;

            if (routeCalculator == null)
                return CardEffectResult.CreateFailure("経路計算サービスが設定されていません。");

            // サイコロを振る
            var diceResult = await diceService.RollForPlayerAsync(context.Player, 2);
            var steps = diceResult.Total;

            // 物件駅のみを経由する特殊移動
            var currentStation = context.Player.CurrentStation!;
            var propertyStationCount = 0;
            var nextStation = currentStation;

            while (propertyStationCount < steps && nextStation != null)
            {
                // 次の物件駅を探す
                var candidates = nextStation.ConnectedStations
                    .Where(s => s.Type == StationType.Property)
                    .ToList();

                if (!candidates.Any())
                    break;

                // ランダムに選択（実際のゲームではプレイヤーが選択）
                var random = new Random();
                nextStation = candidates[random.Next(candidates.Count)];
                propertyStationCount++;
            }

            if (nextStation != null && nextStation != currentStation)
            {
                context.Player.CurrentStation = nextStation;

                var gameManager = (IGameManager)context.GameManager;
                if (nextStation.IsDestination)
                {
                    await gameManager.ProcessDestinationArrivalAsync(context.Player);
                }

                return CardEffectResult.CreateSuccess($"物件駅を{propertyStationCount}駅移動して{nextStation.Name}に到着しました！");
            }

            await Task.CompletedTask;
            return CardEffectResult.CreateFailure("移動できる物件駅がありません。");
        }

        public override string GetDescription()
        {
            return "サイコロを2個振り、出た数だけ物件駅のみを移動します。";
        }
    }

    #region イベントクラス

    /// <summary>
    /// カード選択要求イベント
    /// </summary>
    public class CardSelectionRequestEvent
    {
        public Player Player { get; set; } = null!;
        public List<Card> SelectableCards { get; set; } = new List<Card>();
        public string Message { get; set; } = string.Empty;
        public SelectionType SelectionType { get; set; }
    }

    /// <summary>
    /// 物件選択要求イベント
    /// </summary>
    public class PropertySelectionRequestEvent
    {
        public Player Player { get; set; } = null!;
        public string Message { get; set; } = string.Empty;
        public SelectionType SelectionType { get; set; }
        public bool FreeSelection { get; set; }
    }

    /// <summary>
    /// プレイヤー選択要求イベント
    /// </summary>
    public class PlayerSelectionRequestEvent
    {
        public List<Player> SelectablePlayers { get; set; } = new List<Player>();
        public string Message { get; set; } = string.Empty;
        public SelectionType SelectionType { get; set; }
    }

    /// <summary>
    /// カードバンクアクセスイベント
    /// </summary>
    public class CardBankAccessEvent
    {
        public Player Player { get; set; } = null!;
    }

    /// <summary>
    /// 選択タイプ
    /// </summary>
    public enum SelectionType
    {
        Single,
        Multiple
    }

    #endregion
}