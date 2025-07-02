using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MomotetsuGame.Application.Commands;
using MomotetsuGame.Services;

namespace MomotetsuGame.ViewModels
{
    /// <summary>
    /// モード選択画面のViewModel
    /// </summary>
    public class ModeSelectionViewModel : INotifyPropertyChanged
    {
        private bool _hasSaveData;

        /// <summary>
        /// セーブデータが存在するか
        /// </summary>
        public bool HasSaveData
        {
            get => _hasSaveData;
            set => SetProperty(ref _hasSaveData, value);
        }

        /// <summary>
        /// つづきからコマンド
        /// </summary>
        public ICommand ContinueGameCommand { get; }

        /// <summary>
        /// ひとりで桃鉄コマンド
        /// </summary>
        public ICommand SinglePlayCommand { get; }

        /// <summary>
        /// おまけコマンド
        /// </summary>
        public ICommand ShowExtrasCommand { get; }

        /// <summary>
        /// 遊びかたコマンド
        /// </summary>
        public ICommand ShowTutorialCommand { get; }

        /// <summary>
        /// ウィンドウを閉じる要求イベント
        /// </summary>
        public event Action? CloseRequested;

        /// <summary>
        /// 画面遷移要求イベント
        /// </summary>
        public event Action<string, object?>? NavigateRequested;

        public ModeSelectionViewModel()
        {
            // コマンドの初期化
            ContinueGameCommand = new RelayCommand(ContinueGame, () => HasSaveData);
            SinglePlayCommand = new RelayCommand(StartSinglePlay);
            ShowExtrasCommand = new RelayCommand(ShowExtras);
            ShowTutorialCommand = new RelayCommand(ShowTutorial);

            // セーブデータの存在チェック
            CheckSaveData();
        }

        /// <summary>
        /// セーブデータの存在をチェック
        /// </summary>
        private void CheckSaveData()
        {
            try
            {
                var saveDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MomotetsuGame",
                    "SaveData");

                if (Directory.Exists(saveDirectory))
                {
                    var saveFiles = Directory.GetFiles(saveDirectory, "*.save");
                    HasSaveData = saveFiles.Length > 0;
                }
                else
                {
                    HasSaveData = false;
                }
            }
            catch
            {
                HasSaveData = false;
            }
        }

        /// <summary>
        /// つづきから
        /// </summary>
        private void ContinueGame()
        {
            NavigateRequested?.Invoke("LoadGame", null);
        }

        /// <summary>
        /// ひとりで桃鉄
        /// </summary>
        private void StartSinglePlay()
        {
            NavigateRequested?.Invoke("GameSettings", new { Mode = "Single" });
        }

        /// <summary>
        /// おまけ
        /// </summary>
        private void ShowExtras()
        {
            NavigateRequested?.Invoke("Extras", null);
        }

        /// <summary>
        /// 遊びかた
        /// </summary>
        private void ShowTutorial()
        {
            NavigateRequested?.Invoke("Tutorial", null);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}