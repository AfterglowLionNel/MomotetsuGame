using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MomotetsuGame.ViewModels
{
    /// <summary>
    /// ���ׂĂ�ViewModel�̊��N���X
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// �v���p�e�B�ύX�ʒm�C�x���g
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// �v���p�e�B�ύX�ʒm�𔭍s
        /// </summary>
        /// <param name="propertyName">�v���p�e�B��</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// �v���p�e�B�̒l��ݒ肵�A�ύX������Βʒm�𔭍s
        /// </summary>
        /// <typeparam name="T">�v���p�e�B�̌^</typeparam>
        /// <param name="field">�o�b�L���O�t�B�[���h</param>
        /// <param name="value">�V�����l</param>
        /// <param name="propertyName">�v���p�e�B��</param>
        /// <returns>�l���ύX���ꂽ���ǂ���</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// �����̃v���p�e�B�ύX�ʒm�𔭍s
        /// </summary>
        /// <param name="propertyNames">�v���p�e�B���̃��X�g</param>
        protected void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// ���ׂẴv���p�e�B�̕ύX�ʒm�𔭍s
        /// </summary>
        protected void OnAllPropertiesChanged()
        {
            OnPropertyChanged(string.Empty);
        }

        private bool _isBusy;
        /// <summary>
        /// �r�W�[��Ԃ��ǂ���
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string? _busyMessage;
        /// <summary>
        /// �r�W�[���̃��b�Z�[�W
        /// </summary>
        public string? BusyMessage
        {
            get => _busyMessage;
            set => SetProperty(ref _busyMessage, value);
        }

        /// <summary>
        /// �r�W�[��Ԃ�ݒ�
        /// </summary>
        /// <param name="message">�\�����b�Z�[�W</param>
        protected void SetBusy(string? message = null)
        {
            BusyMessage = message;
            IsBusy = true;
        }

        /// <summary>
        /// �r�W�[��Ԃ�����
        /// </summary>
        protected void ClearBusy()
        {
            IsBusy = false;
            BusyMessage = null;
        }

        /// <summary>
        /// �f�U�C�����[�h���ǂ���
        /// </summary>
        protected bool IsInDesignMode => DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());
    }
}