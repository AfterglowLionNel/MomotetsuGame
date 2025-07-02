using System;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;

namespace MomotetsuGame.Core.Interfaces
{
    /// <summary>
    /// カード効果のインターフェース
    /// </summary>
    public interface ICardEffect
    {
        /// <summary>
        /// 効果の種類
        /// </summary>
        string EffectType { get; }

        /// <summary>
        /// 効果が実行可能かチェック
        /// </summary>
        /// <param name="context">ゲームコンテキスト</param>
        /// <returns>実行可能な場合true</returns>
        Task<bool> CanExecute(CardEffectContext context);

        /// <summary>
        /// 効果を実行
        /// </summary>
        /// <param name="context">ゲームコンテキスト</param>
        /// <returns>実行結果</returns>
        Task<EffectResult> Execute(CardEffectContext context);

        /// <summary>
        /// 効果の説明を取得
        /// </summary>
        /// <returns>説明文</returns>
        string GetDescription();
    }

    /// <summary>
    /// カード効果実行時のコンテキスト
    /// </summary>
    public class CardEffectContext
    {
        /// <summary>
        /// 現在のゲーム状態
        /// </summary>
        public GameState GameState { get; set; } = null!;

        /// <summary>
        /// カードを使用したプレイヤー
        /// </summary>
        public Player Player { get; set; } = null!;

        /// <summary>
        /// 使用されたカード
        /// </summary>
        public Card Card { get; set; } = null!;

        /// <summary>
        /// ゲームマネージャー
        /// </summary>
        public IGameManager GameManager { get; set; } = null!;

        /// <summary>
        /// ダイスサービス
        /// </summary>
        public IDiceService? DiceService { get; set; }

        /// <summary>
        /// イベントバス
        /// </summary>
        public IEventBus? EventBus { get; set; }

        /// <summary>
        /// UI対話サービス
        /// </summary>
        public IDialogService? DialogService { get; set; }

        /// <summary>
        /// 追加パラメータ
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// カード効果の実行結果
    /// </summary>
    public class EffectResult
    {
        /// <summary>
        /// 成功したかどうか
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 結果メッセージ
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 連続行動可能かどうか
        /// </summary>
        public bool AllowContinuation { get; set; }

        /// <summary>
        /// 追加データ
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// サイコロサービスインターフェース
    /// </summary>
    public interface IDiceService
    {
        /// <summary>
        /// サイコロを振る
        /// </summary>
        /// <param name="count">サイコロの数</param>
        /// <returns>サイコロの結果</returns>
        DiceResult Roll(int count);

        /// <summary>
        /// 特殊サイコロを振る
        /// </summary>
        /// <param name="count">サイコロの数</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns>サイコロの結果</returns>
        DiceResult RollSpecial(int count, int min, int max);
    }

    /// <summary>
    /// イベントバスインターフェース
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// イベントを発行
        /// </summary>
        void Publish<TEvent>(TEvent eventData) where TEvent : class;

        /// <summary>
        /// イベントを購読
        /// </summary>
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

        /// <summary>
        /// イベントの購読を解除
        /// </summary>
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
    }

    /// <summary>
    /// ダイアログサービスインターフェース
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// 確認ダイアログを表示
        /// </summary>
        Task<bool> ShowConfirmationAsync(string message, string title);

        /// <summary>
        /// 選択ダイアログを表示
        /// </summary>
        Task<T?> ShowSelectionAsync<T>(IEnumerable<T> items, string title);

        /// <summary>
        /// メッセージダイアログを表示
        /// </summary>
        Task ShowMessageAsync(string message, string title);

        /// <summary>
        /// エラーダイアログを表示
        /// </summary>
        Task ShowErrorAsync(string message, string title);
    }

    /// <summary>
    /// UI更新要求イベント（カード選択など）
    /// </summary>
    public class CardSelectionRequestEvent
    {
        public Player Player { get; set; } = null!;
        public List<Card> AvailableCards { get; set; } = new List<Card>();
        public string Message { get; set; } = string.Empty;
        public Card? SelectedCard { get; set; }
        public TaskCompletionSource<Card?> CompletionSource { get; set; } = new TaskCompletionSource<Card?>();

        public async Task<Card?> WaitForCompletionAsync()
        {
            return await CompletionSource.Task;
        }
    }

    /// <summary>
    /// プレイヤー選択要求イベント
    /// </summary>
    public class PlayerSelectionRequestEvent
    {
        public List<Player> AvailablePlayers { get; set; } = new List<Player>();
        public string Message { get; set; } = string.Empty;
        public Player? SelectedPlayer { get; set; }
        public TaskCompletionSource<Player?> CompletionSource { get; set; } = new TaskCompletionSource<Player?>();

        public async Task<Player?> WaitForCompletionAsync()
        {
            return await CompletionSource.Task;
        }
    }

    /// <summary>
    /// 物件購入要求イベント
    /// </summary>
    public class PropertyPurchaseRequestEvent
    {
        public Player Player { get; set; } = null!;
        public Station Station { get; set; } = null!;
        public List<Property> AvailableProperties { get; set; } = new List<Property>();
        public List<Property> PurchasedProperties { get; set; } = new List<Property>();
        public TaskCompletionSource<List<Property>> CompletionSource { get; set; } = new TaskCompletionSource<List<Property>>();

        public async Task<List<Property>> WaitForCompletionAsync()
        {
            return await CompletionSource.Task;
        }
    }
}