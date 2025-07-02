using System;
using Microsoft.Extensions.DependencyInjection;
using MomotetsuGame.Application.AI;
using MomotetsuGame.Application.CardEffects;
using MomotetsuGame.Application.GameLogic;
using MomotetsuGame.Application.Services;
using MomotetsuGame.Core.Interfaces;
using MomotetsuGame.Infrastructure.Services;
using MomotetsuGame.ViewModels;

namespace MomotetsuGame.Application.DependencyInjection
{
    /// <summary>
    /// DI�R���e�i�̐ݒ�
    /// </summary>
    public static class ServiceContainer
    {
        private static IServiceProvider? _serviceProvider;

        /// <summary>
        /// �T�[�r�X�v���o�C�_�[���擾
        /// </summary>
        public static IServiceProvider ServiceProvider
        {
            get => _serviceProvider ?? throw new InvalidOperationException("ServiceProvider is not initialized");
            private set => _serviceProvider = value;
        }

        /// <summary>
        /// DI�R���e�i���\��
        /// </summary>
        public static void Configure()
        {
            var services = new ServiceCollection();

            // Infrastructure Services
            services.AddSingleton<IEventBus, SimpleEventBus>();
            services.AddSingleton<IDialogService, SimpleDialogService>();
            services.AddSingleton<IMessageService, SimpleMessageService>();
            services.AddSingleton<IAudioService, SimpleAudioService>();
            services.AddSingleton<ISaveDataService, SimpleSaveDataService>();
            services.AddSingleton<INavigationService, SimpleNavigationService>();

            // Core Services
            services.AddSingleton<IDiceService, DiceService>();
            services.AddSingleton<IRouteCalculator, RouteCalculator>();
            services.AddSingleton<IPropertyService, PropertyService>();
            services.AddSingleton<IPropertyEvaluator, PropertyEvaluator>();
            services.AddSingleton<IPathFinder, PathFinder>();

            // Game Services
            services.AddSingleton<IGameManager, GameManager>();
            services.AddSingleton<IGameStateRepository, GameStateRepository>();
            services.AddSingleton<ICardEffectProcessor, CardEffectProcessor>();
            services.AddSingleton<IMasterDataLoader, MasterDataLoader>();

            // AI Services
            services.AddTransient<IComputerAI>(provider =>
            {
                var pathFinder = provider.GetRequiredService<IPathFinder>();
                var propertyEvaluator = provider.GetRequiredService<IPropertyEvaluator>();
                // �f�t�H���g�̓o�����X�^AI
                return new ComputerAI(Core.Enums.AIStrategy.Balanced, pathFinder, propertyEvaluator);
            });

            // ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<ModeSelectionViewModel>();
            services.AddTransient<GameSettingsViewModel>();
            services.AddTransient<PropertyPurchaseViewModel>();
            services.AddTransient<DiceRollViewModel>();

            // Build service provider
            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// �T�[�r�X���擾
        /// </summary>
        public static T GetService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// �T�[�r�X���擾�inull���e�j
        /// </summary>
        public static T? GetServiceOrNull<T>() where T : class
        {
            return ServiceProvider.GetService<T>();
        }
    }

    // ���̃T�[�r�X�����i��œK�؂Ɏ����j
    public class SimpleAudioService : IAudioService
    {
        public bool IsMuted { get; set; }

        public void PlayBgm(string bgmName, bool loop = true)
        {
            // TODO: ����
        }

        public void PlaySe(string seName)
        {
            // TODO: ����
        }

        public void SetBgmVolume(float volume)
        {
            // TODO: ����
        }

        public void SetSeVolume(float volume)
        {
            // TODO: ����
        }

        public void StopBgm(int fadeOutMs = 1000)
        {
            // TODO: ����
        }
    }

    public class SimpleSaveDataService : ISaveDataService
    {
        public Task<bool> ExistsAsync(string saveId)
        {
            // TODO: ����
            return Task.FromResult(false);
        }

        public Task<SaveDataInfo[]> GetSaveDataListAsync()
        {
            // TODO: ����
            return Task.FromResult(Array.Empty<SaveDataInfo>());
        }

        public Task SaveAsync(string saveId, object gameState)
        {
            // TODO: ����
            return Task.CompletedTask;
        }

        public Task<T?> LoadAsync<T>(string saveId) where T : class
        {
            // TODO: ����
            return Task.FromResult<T?>(null);
        }

        public Task DeleteAsync(string saveId)
        {
            // TODO: ����
            return Task.CompletedTask;
        }
    }

    public class GameStateRepository : IGameStateRepository
    {
        // TODO: ����
    }

    public class CardEffectProcessor : ICardEffectProcessor
    {
        // TODO: ����
    }

    public class MasterDataLoader : IMasterDataLoader
    {
        // TODO: ����
    }

    public class PropertyEvaluator : IPropertyEvaluator
    {
        // TODO: ����
    }

    public class PathFinder : IPathFinder
    {
        // TODO: ����
    }

    // �K�v�ȃC���^�[�t�F�[�X��`
    public interface IGameStateRepository { }
    public interface ICardEffectProcessor { }
    public interface IMasterDataLoader { }
    public interface IPropertyEvaluator { }
    public interface IPathFinder { }
}