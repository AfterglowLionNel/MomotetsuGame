using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.Enums;
using MomotetsuGame.Core.Interfaces;

namespace MomotetsuGame.Application.CardEffects
{
    /// <summary>
    /// 牛歩カード効果（3-5ターン移動制限）
    /// </summary>
    public class CowWalkCardEffect : CardEffectBase
    {
        public override string EffectType => "CowWalk";
        private const int SUCCESS_RATE = 88;

        public override async Task<CardEffectResult> Execute(CardEffectContext context)
        {
            if (context.EventBus == null)
                return CardEffectResult.CreateFailure("必要なサービスが設定されていません。");

            var eventBus = (IEventBus)context.EventBus;
            var random = new Random();

            // 対象プレイヤーを選択
            var targetPlayers = context.GameState.Players
                .Where(p => p.Id != context.Player.Id && p.Status != PlayerStatus.Cow)
                .ToList();

            if (!targetPlayers.Any())
                return CardEffectResult.CreateFailure("対象となるプレイヤーがいません。");

            // プレイヤー選択イベントを発火
            var selectionEvent = new PlayerSelectionRequestEvent
            {
                SelectablePlayers = targetPlayers,
                Message = "牛歩状態にするプレイヤーを選んでください",
                SelectionType = SelectionType.Single
            };

            eventBus.Publish(selectionEvent);

            // 選択結果を待つ
            if (!context.Parameters.TryGetValue("SelectedPlayer", out var playerObj) ||
                playerObj is not Player targetPlayer)
            {
                return CardEffectResult.CreateFailure("プレイヤーが選択されませんでした。");
            }

            // 成功判定
            if (random.Next(100) >= SUCCESS_RATE)
            {
                return CardEffectResult.CreateFailure($"{targetPlayer.Name}への攻撃は失敗しました。");
            }

            // 効果期間を決定（3-5ターン）
            var duration = random.Next(3, 6);
            targetPlayer.SetStatus(PlayerStatus.Cow, duration);

            await Task.CompletedTask;
            return CardEffectResult.CreateSuccess($"{targetPlayer.Name}を{duration}ターンの間、牛歩状態にしました！");
        }

        public override string GetDescription()
        {
            return $"相手を牛歩状態（1マスしか進めない）にします。（成功率{SUCCESS_RATE}%）";
        }
    }

    /// <summary>
    /// 豪速球カード効果（全員のカード2-8枚破壊）
    /// </summary>
    public class FastBallCardEffect : CardEffectBase
    {
        public override string EffectType => "FastBall";
        private const int SUCCESS_RATE = 88;

        public override async Task<CardEffectResult> Execute(CardEffectContext context)
        {
            var random = new Random();

            // 成功判定
            if (random.Next(100) >= SUCCESS_RATE)
            {
                return CardEffectResult.CreateFailure("豪速球は外れました！");
            }

            var totalDestroyed = 0;
            var affectedPlayers = new List<Player>();

            // 全プレイヤーのカードを破壊
            foreach (var player in context.GameState.Players)
            {
                if (!player.Cards.Any())
                    continue;

                // 破壊枚数を決定（2-8枚、ただし所持数を超えない）
                var destroyCount = Math.Min(player.Cards.Count, random.Next(2, 9));

                // ランダムにカードを選んで破壊
                var cardsToDestroy = player.Cards
                    .OrderBy(_ => random.Next())
                    .Take(destroyCount)
                    .ToList();

                foreach (var card in cardsToDestroy)
                {
                    player.Cards.Remove(card);
                    totalDestroyed++;
                }

                if (destroyCount > 0)
                {
                    affectedPlayers.Add(player);
                }
            }

            var result = CardEffectResult.CreateSuccess($"豪速球で合計{totalDestroyed}枚のカードを破壊しました！");
            result.AffectedPlayers = affectedPlayers;

            await Task.CompletedTask;
            return result;
        }

        public override string GetDescription()
        {
            return $"全員のカードを2〜8枚破壊します。（成功率{SUCCESS_RATE}%）";
        }
    }

    /// <summary>
    /// ふういんカード効果（数ターンカード使用不可）
    /// </summary>
    public class SealCardEffect : CardEffectBase
    {
        public override string EffectType => "Seal";
        private const int SUCCESS_RATE = 90;

        public override async Task<CardEffectResult> Execute(CardEffectContext context)
        {
            if (context.EventBus == null)
                return CardEffectResult.CreateFailure("必要なサービスが設定されていません。");

            var eventBus = (IEventBus)context.EventBus;
            var random = new Random();

            // 対象プレイヤーを選択
            var targetPlayers = context.GameState.Players
                .Where(p => p.Id != context.Player.Id && p.Status != PlayerStatus.Sealed)
                .ToList();

            if (!targetPlayers.Any())
                return CardEffectResult.CreateFailure("対象となるプレイヤーがいません。");

            // プレイヤー選択イベントを発火
            var selectionEvent = new PlayerSelectionRequestEvent
            {
                SelectablePlayers = targetPlayers,
                Message = "封印するプレイヤーを選んでください",
                SelectionType = SelectionType.Single
            };

            eventBus.Publish(selectionEvent);

            // 選択結果を待つ
            if (!context.Parameters.TryGetValue("SelectedPlayer", out var playerObj) ||
                playerObj is not Player targetPlayer)
            {
                return CardEffectResult.CreateFailure("プレイヤーが選択されませんでした。");
            }

            // 成功判定
            if (random.Next(100) >= SUCCESS_RATE)
            {
                return CardEffectResult.CreateFailure($"{targetPlayer.Name}への封印は失敗しました。");
            }

            // 効果期間を決定（3-5ターン）
            var duration = random.Next(3, 6);
            targetPlayer.SetStatus(PlayerStatus.Sealed, duration);

            await Task.CompletedTask;
            return CardEffectResult.CreateSuccess($"{targetPlayer.Name}を{duration}ターンの間、封印状態にしました！");
        }

        public override string GetDescription()
        {
            return $"相手を封印状態（カード使用不可）にします。（成功率{SUCCESS_RATE}%）";
        }
    }

    /// <summary>
    /// 絶不調カード効果（6ヶ月間サイコロ1-2固定）
    /// </summary>
    public class UnluckyCardEffect : CardEffectBase
    {
        public override string EffectType => "Unlucky";
        private const int SUCCESS_RATE = 85;

        public override async Task<CardEffectResult> Execute(CardEffectContext context)
        {
            if (context.EventBus == null)
                return CardEffectResult.CreateFailure("必要なサービスが設定されていません。");

            var eventBus = (IEventBus)context.EventBus;
            var random = new Random();

            // 対象プレイヤーを選択
            var targetPlayers = context.GameState.Players
                .Where(p => p.Id != context.Player.Id && p.Status != PlayerStatus.Unlucky)
                .ToList();

            if (!targetPlayers.Any())
                return CardEffectResult.CreateFailure("対象となるプレイヤーがいません。");

            // プレイヤー選択イベントを発火
            var selectionEvent = new PlayerSelectionRequestEvent
            {
                SelectablePlayers = targetPlayers,
                Message = "絶不調にするプレイヤーを選んでください",
                SelectionType = SelectionType.Single
            };

            eventBus.Publish(selectionEvent);

            // 選択結果を待つ
            if (!context.Parameters.TryGetValue("SelectedPlayer", out var playerObj) ||
                playerObj is not Player targetPlayer)
            {
                return CardEffectResult.CreateFailure("プレイヤーが選択されませんでした。");
            }

            // 成功判定
            if (random.Next(100) >= SUCCESS_RATE)
            {
                return CardEffectResult.CreateFailure($"{targetPlayer.Name}への攻撃は失敗しました。");
            }

            // 6ヶ月（6ターン）の絶不調状態
            targetPlayer.SetStatus(PlayerStatus.Unlucky, 6);

            await Task.CompletedTask;
            return CardEffectResult.CreateSuccess($"{targetPlayer.Name}を6ヶ月間、絶不調状態にしました！");
        }

        public override string GetDescription()
        {
            return $"相手を6ヶ月間絶不調（サイコロが1-2しか出ない）にします。（成功率{SUCCESS_RATE}%）";
        }
    }

    /// <summary>
    /// 場所がえカード効果（他プレイヤーと位置交換）
    /// </summary>
    public class SwapPositionCardEffect : CardEffectBase
    {
        public override string EffectType => "SwapPosition";

        public override async Task<CardEffectResult> Execute(CardEffectContext context)
        {
            if (context.EventBus == null)
                return CardEffectResult.CreateFailure("必要なサービスが設定されていません。");

            var eventBus = (IEventBus)context.EventBus;

            // 対象プレイヤーを選択
            var targetPlayers = context.GameState.Players
                .Where(p => p.Id != context.Player.Id)
                .ToList();

            if (!targetPlayers.Any())
                return CardEffectResult.CreateFailure("対象となるプレイヤーがいません。");

            // プレイヤー選択イベントを発火
            var selectionEvent = new PlayerSelectionRequestEvent
            {
                SelectablePlayers = targetPlayers,
                Message = "位置を交換するプレイヤーを選んでください",
                SelectionType = SelectionType.Single
            };

            eventBus.Publish(selectionEvent);

            // 選択結果を待つ
            if (!context.Parameters.TryGetValue("SelectedPlayer", out var playerObj) ||
                playerObj is not Player targetPlayer)
            {
                return CardEffectResult.CreateFailure("プレイヤーが選択されませんでした。");
            }

            // 位置を交換
            var tempStation = context.Player.CurrentStation;
            context.Player.CurrentStation = targetPlayer.CurrentStation;
            targetPlayer.CurrentStation = tempStation;

            await Task.CompletedTask;
            return CardEffectResult.CreateSuccess($"{targetPlayer.Name}と位置を交換しました！");
        }

        public override string GetDescription()
        {
            return "他のプレイヤーと位置を交換します。";
        }
    }

    /// <summary>
    /// ボンビー呼びカード効果（ボンビーを呼び出す）
    /// </summary>
    public class CallBonbyCardEffect : CardEffectBase
    {
        public override string EffectType => "CallBonby";

        public override async Task<bool> CanExecute(CardEffectContext context)
        {
            if (!await base.CanExecute(context))
                return false;

            // ボンビーが既に存在しない場合のみ使用可能
            return context.GameState.ActiveBonby == null;
        }

        public override async Task<CardEffectResult> Execute(CardEffectContext context)
        {
            // 新しいボンビーを生成
            var bonby = new Bonby { Type = BonbyType.Mini };
            context.GameState.ActiveBonby = bonby;

            // 最下位プレイヤーに付着
            var lastPlayer = context.GameState.Players
                .OrderByDescending(p => p.Rank)
                .First();

            lastPlayer.AttachedBonby = bonby;

            await Task.CompletedTask;
            return CardEffectResult.CreateSuccess($"ミニボンビーが現れて{lastPlayer.Name}に取り憑きました！");
        }

        public override string GetDescription()
        {
            return "ミニボンビーを呼び出し、最下位のプレイヤーに取り憑かせます。";
        }
    }

    /// <summary>
    /// 時限爆弾カード効果（数ターン後に爆発）
    /// </summary>
    public class TimeBombCardEffect : CardEffectBase
    {
        public override string EffectType => "TimeBomb";

        public override async Task<CardEffectResult> Execute(CardEffectContext context)
        {
            if (context.EventBus == null)
                return CardEffectResult.CreateFailure("必要なサービスが設定されていません。");

            var eventBus = (IEventBus)context.EventBus;
            var random = new Random();

            // 爆発までのターン数（3-5ターン）
            var countdown = random.Next(3, 6);

            // 時限爆弾を設置
            var bombEvent = new TimeBombPlacedEvent
            {
                PlacedBy = context.Player,
                Countdown = countdown,
                InitialHolder = context.Player
            };

            eventBus.Publish(bombEvent);

            // プレイヤーの拡張データに爆弾情報を保存
            context.Parameters["TimeBomb"] = new TimeBombInfo
            {
                Countdown = countdown,
                CurrentHolder = context.Player
            };

            await Task.CompletedTask;
            return CardEffectResult.CreateSuccess($"時限爆弾を設置しました！{countdown}ターン後に爆発します。");
        }

        public override string GetDescription()
        {
            return "時限爆弾を設置します。数ターン後に爆発し、持っているプレイヤーに大ダメージを与えます。";
        }
    }

    #region イベントクラス

    /// <summary>
    /// 時限爆弾設置イベント
    /// </summary>
    public class TimeBombPlacedEvent
    {
        public Player PlacedBy { get; set; } = null!;
        public int Countdown { get; set; }
        public Player InitialHolder { get; set; } = null!;
    }

    /// <summary>
    /// 時限爆弾情報
    /// </summary>
    public class TimeBombInfo
    {
        public int Countdown { get; set; }
        public Player CurrentHolder { get; set; } = null!;
    }

    #endregion
}