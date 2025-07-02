using System;
using System.Threading.Tasks;

namespace MomotetsuGame.Core.Interfaces
{
    /// <summary>
    /// イベントバスのインターフェース
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
        IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

        /// <summary>
        /// 非同期イベントを購読
        /// </summary>
        IDisposable SubscribeAsync<TEvent>(Func<TEvent, Task> handler) where TEvent : class;
    }

    /// <summary>
    /// ダイアログサービスのインターフェース
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// 確認ダイアログを表示
        /// </summary>
        Task<bool> ShowConfirmationAsync(string message, string title = "確認");

        /// <summary>
        /// 情報ダイアログを表示
        /// </summary>
        Task ShowInformationAsync(string message, string title = "情報");

        /// <summary>
        /// エラーダイアログを表示
        /// </summary>
        Task ShowErrorAsync(string message, string title = "エラー");

        /// <summary>
        /// 選択ダイアログを表示
        /// </summary>
        Task<T?> ShowSelectionAsync<T>(string message, T[] options, string title = "選択");

        /// <summary>
        /// カスタムダイアログを表示
        /// </summary>
        Task<TResult?> ShowDialogAsync<TResult>(object viewModel);
    }

    /// <summary>
    /// メッセージサービスのインターフェース
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// 情報メッセージを表示
        /// </summary>
        void ShowInfo(string message);

        /// <summary>
        /// 警告メッセージを表示
        /// </summary>
        void ShowWarning(string message);

        /// <summary>
        /// エラーメッセージを表示
        /// </summary>
        void ShowError(string message);

        /// <summary>
        /// 成功メッセージを表示
        /// </summary>
        void ShowSuccess(string message);
    }

    /// <summary>
    /// ナビゲーションサービスのインターフェース
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// 指定されたビューに遷移
        /// </summary>
        Task NavigateToAsync(string viewName, object? parameter = null);

        /// <summary>
        /// 前の画面に戻る
        /// </summary>
        Task GoBackAsync();

        /// <summary>
        /// ナビゲーション可能かチェック
        /// </summary>
        bool CanGoBack { get; }
    }

    /// <summary>
    /// オーディオサービスのインターフェース
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// BGMを再生
        /// </summary>
        void PlayBgm(string bgmName, bool loop = true);

        /// <summary>
        /// BGMを停止
        /// </summary>
        void StopBgm(int fadeOutMs = 1000);

        /// <summary>
        /// 効果音を再生
        /// </summary>
        void PlaySe(string seName);

        /// <summary>
        /// BGM音量を設定
        /// </summary>
        void SetBgmVolume(float volume);

        /// <summary>
        /// 効果音音量を設定
        /// </summary>
        void SetSeVolume(float volume);

        /// <summary>
        /// ミュート設定
        /// </summary>
        bool IsMuted { get; set; }
    }

    /// <summary>
    /// セーブデータサービスのインターフェース
    /// </summary>
    public interface ISaveDataService
    {
        /// <summary>
        /// セーブデータの存在確認
        /// </summary>
        Task<bool> ExistsAsync(string saveId);

        /// <summary>
        /// セーブデータ一覧を取得
        /// </summary>
        Task<SaveDataInfo[]> GetSaveDataListAsync();

        /// <summary>
        /// ゲームをセーブ
        /// </summary>
        Task SaveAsync(string saveId, object gameState);

        /// <summary>
        /// ゲームをロード
        /// </summary>
        Task<T?> LoadAsync<T>(string saveId) where T : class;

        /// <summary>
        /// セーブデータを削除
        /// </summary>
        Task DeleteAsync(string saveId);
    }

    /// <summary>
    /// セーブデータ情報
    /// </summary>
    public class SaveDataInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public long TotalAssets { get; set; }
    }
}