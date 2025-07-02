using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MomotetsuGame.Core.Interfaces;

namespace MomotetsuGame.Services
{
    /// <summary>
    /// ダイアログ表示サービスの実装
    /// </summary>
    public class DialogService : IDialogService
    {
        /// <summary>
        /// 確認ダイアログを表示
        /// </summary>
        public async Task<bool> ShowConfirmationAsync(string message, string title)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var result = MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                return result == MessageBoxResult.Yes;
            });
        }

        /// <summary>
        /// 選択ダイアログを表示
        /// </summary>
        public async Task<T?> ShowSelectionAsync<T>(IEnumerable<T> items, string title)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var window = new SelectionDialog<T>(items, title);
                window.Owner = Application.Current.MainWindow;

                if (window.ShowDialog() == true)
                {
                    return window.SelectedItem;
                }

                return default(T);
            });
        }

        /// <summary>
        /// メッセージダイアログを表示
        /// </summary>
        public async Task ShowMessageAsync(string message, string title)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            });
        }

        /// <summary>
        /// エラーダイアログを表示
        /// </summary>
        public async Task ShowErrorAsync(string message, string title)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            });
        }

        /// <summary>
        /// カスタムダイアログを表示
        /// </summary>
        public async Task<TResult?> ShowCustomDialogAsync<TDialog, TResult>(object? parameter = null)
            where TDialog : Window, new()
            where TResult : class
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new TDialog();
                dialog.Owner = Application.Current.MainWindow;

                if (parameter != null && dialog.DataContext != null)
                {
                    // パラメータをDataContextに設定（ViewModelがIDialogAwareを実装している場合）
                    if (dialog.DataContext is IDialogAware dialogAware)
                    {
                        dialogAware.OnDialogOpened(parameter);
                    }
                }

                if (dialog.ShowDialog() == true)
                {
                    // 結果を取得
                    if (dialog.DataContext is IDialogAware dialogAware)
                    {
                        return dialogAware.GetDialogResult() as TResult;
                    }
                }

                return default(TResult);
            });
        }
    }

    /// <summary>
    /// ダイアログ対応インターフェース
    /// </summary>
    public interface IDialogAware
    {
        /// <summary>
        /// ダイアログが開かれた時の処理
        /// </summary>
        void OnDialogOpened(object parameter);

        /// <summary>
        /// ダイアログの結果を取得
        /// </summary>
        object? GetDialogResult();
    }

    /// <summary>
    /// 汎用選択ダイアログ（仮実装）
    /// </summary>
    /// <typeparam name="T">選択アイテムの型</typeparam>
    internal class SelectionDialog<T> : Window
    {
        public T? SelectedItem { get; private set; }

        public SelectionDialog(IEnumerable<T> items, string title)
        {
            Title = title;
            Width = 400;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // 仮実装：実際のUIは後で作成
            var listBox = new System.Windows.Controls.ListBox
            {
                ItemsSource = items.ToList(),
                Margin = new Thickness(10)
            };

            listBox.MouseDoubleClick += (s, e) =>
            {
                SelectedItem = (T?)listBox.SelectedItem;
                DialogResult = true;
            };

            var grid = new System.Windows.Controls.Grid();
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });

            grid.Children.Add(listBox);
            System.Windows.Controls.Grid.SetRow(listBox, 0);

            var buttonPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            var okButton = new System.Windows.Controls.Button
            {
                Content = "OK",
                Width = 80,
                Margin = new Thickness(5),
                IsDefault = true
            };
            okButton.Click += (s, e) =>
            {
                SelectedItem = (T?)listBox.SelectedItem;
                DialogResult = true;
            };

            var cancelButton = new System.Windows.Controls.Button
            {
                Content = "キャンセル",
                Width = 80,
                Margin = new Thickness(5),
                IsCancel = true
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            grid.Children.Add(buttonPanel);
            System.Windows.Controls.Grid.SetRow(buttonPanel, 1);

            Content = grid;
        }
    }
}