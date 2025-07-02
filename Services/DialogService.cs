using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MomotetsuGame.Core.Interfaces;

namespace MomotetsuGame.Services
{
    /// <summary>
    /// �_�C�A���O�\���T�[�r�X�̎���
    /// </summary>
    public class DialogService : IDialogService
    {
        /// <summary>
        /// �m�F�_�C�A���O��\��
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
        /// �I���_�C�A���O��\��
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
        /// ���b�Z�[�W�_�C�A���O��\��
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
        /// �G���[�_�C�A���O��\��
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
        /// �J�X�^���_�C�A���O��\��
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
                    // �p�����[�^��DataContext�ɐݒ�iViewModel��IDialogAware���������Ă���ꍇ�j
                    if (dialog.DataContext is IDialogAware dialogAware)
                    {
                        dialogAware.OnDialogOpened(parameter);
                    }
                }

                if (dialog.ShowDialog() == true)
                {
                    // ���ʂ��擾
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
    /// �_�C�A���O�Ή��C���^�[�t�F�[�X
    /// </summary>
    public interface IDialogAware
    {
        /// <summary>
        /// �_�C�A���O���J���ꂽ���̏���
        /// </summary>
        void OnDialogOpened(object parameter);

        /// <summary>
        /// �_�C�A���O�̌��ʂ��擾
        /// </summary>
        object? GetDialogResult();
    }

    /// <summary>
    /// �ėp�I���_�C�A���O�i�������j
    /// </summary>
    /// <typeparam name="T">�I���A�C�e���̌^</typeparam>
    internal class SelectionDialog<T> : Window
    {
        public T? SelectedItem { get; private set; }

        public SelectionDialog(IEnumerable<T> items, string title)
        {
            Title = title;
            Width = 400;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // �������F���ۂ�UI�͌�ō쐬
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
                Content = "�L�����Z��",
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