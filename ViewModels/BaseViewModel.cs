using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MomotetsuGame.ViewModels
{
    /// <summary>
    /// すべてのViewModelの基底クラス
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティ変更通知イベント
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// プロパティ変更通知を発行
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// プロパティの値を設定し、変更があれば通知を発行
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="field">バッキングフィールド</param>
        /// <param name="value">新しい値</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <returns>値が変更されたかどうか</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 複数のプロパティ変更通知を発行
        /// </summary>
        /// <param name="propertyNames">プロパティ名のリスト</param>
        protected void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// すべてのプロパティの変更通知を発行
        /// </summary>
        protected void OnAllPropertiesChanged()
        {
            OnPropertyChanged(string.Empty);
        }

        private bool _isBusy;
        /// <summary>
        /// ビジー状態かどうか
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string? _busyMessage;
        /// <summary>
        /// ビジー時のメッセージ
        /// </summary>
        public string? BusyMessage
        {
            get => _busyMessage;
            set => SetProperty(ref _busyMessage, value);
        }

        /// <summary>
        /// ビジー状態を設定
        /// </summary>
        /// <param name="message">表示メッセージ</param>
        protected void SetBusy(string? message = null)
        {
            BusyMessage = message;
            IsBusy = true;
        }

        /// <summary>
        /// ビジー状態を解除
        /// </summary>
        protected void ClearBusy()
        {
            IsBusy = false;
            BusyMessage = null;
        }

        /// <summary>
        /// デザインモードかどうか
        /// </summary>
        protected bool IsInDesignMode => DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());
    }
}