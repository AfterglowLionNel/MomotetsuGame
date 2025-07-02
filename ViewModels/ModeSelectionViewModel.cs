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
    /// ���[�h�I����ʂ�ViewModel
    /// </summary>
    public class ModeSelectionViewModel : INotifyPropertyChanged
    {
        private bool _hasSaveData;

        /// <summary>
        /// �Z�[�u�f�[�^�����݂��邩
        /// </summary>
        public bool HasSaveData
        {
            get => _hasSaveData;
            set => SetProperty(ref _hasSaveData, value);
        }

        /// <summary>
        /// �Â�����R�}���h
        /// </summary>
        public ICommand ContinueGameCommand { get; }

        /// <summary>
        /// �ЂƂ�œ��S�R�}���h
        /// </summary>
        public ICommand SinglePlayCommand { get; }

        /// <summary>
        /// ���܂��R�}���h
        /// </summary>
        public ICommand ShowExtrasCommand { get; }

        /// <summary>
        /// �V�т����R�}���h
        /// </summary>
        public ICommand ShowTutorialCommand { get; }

        /// <summary>
        /// �E�B���h�E�����v���C�x���g
        /// </summary>
        public event Action? CloseRequested;

        /// <summary>
        /// ��ʑJ�ڗv���C�x���g
        /// </summary>
        public event Action<string, object?>? NavigateRequested;

        public ModeSelectionViewModel()
        {
            // �R�}���h�̏�����
            ContinueGameCommand = new RelayCommand(ContinueGame, () => HasSaveData);
            SinglePlayCommand = new RelayCommand(StartSinglePlay);
            ShowExtrasCommand = new RelayCommand(ShowExtras);
            ShowTutorialCommand = new RelayCommand(ShowTutorial);

            // �Z�[�u�f�[�^�̑��݃`�F�b�N
            CheckSaveData();
        }

        /// <summary>
        /// �Z�[�u�f�[�^�̑��݂��`�F�b�N
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
        /// �Â�����
        /// </summary>
        private void ContinueGame()
        {
            NavigateRequested?.Invoke("LoadGame", null);
        }

        /// <summary>
        /// �ЂƂ�œ��S
        /// </summary>
        private void StartSinglePlay()
        {
            NavigateRequested?.Invoke("GameSettings", new { Mode = "Single" });
        }

        /// <summary>
        /// ���܂�
        /// </summary>
        private void ShowExtras()
        {
            NavigateRequested?.Invoke("Extras", null);
        }

        /// <summary>
        /// �V�т���
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