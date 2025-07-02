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
    /// DIコンテナの設定
    /// </summary>
    public static class ServiceContainer
    {
        private static IServiceProvider? _serviceProvider;

        /// <summary>
        /// サービスプロバイダーを取得
        /// </summary>
        public static IServiceProvider ServiceProvider
        {
            get => _serviceProvider ?? throw new InvalidOperationException("ServiceProvider is not initialized");
            private set => _serviceProvider = value;
        }

        /// <summary>
        /// DIコンテナを構成
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
                // デフォルトはバランス型AI
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
        /// サービスを取得
        /// </summary>
        public static T GetService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// サービスを取得（null許容）
        /// </summary>
        public static T? GetServiceOrNull<T>() where T : class
        {
            return ServiceProvider.GetService<T>();
        }
    }

    // 仮のサービス実装（後で適切に実装）
    public class SimpleAudioService : IAudioService
    {
        public bool IsMuted { get; set; }

        public void PlayBgm(string bgmName, bool loop = true)
        {
            // TODO: 実装
        }

        public void PlaySe(string seName)
        {
            // TODO: 実装
        }

        public void SetBgmVolume(float volume)
        {
            // TODO: 実装
        }

        public void SetSeVolume(float volume)
        {
            // TODO: 実装
        }

        public void StopBgm(int fadeOutMs = 1000)
        {
            // TODO: 実装
        }
    }

    public class SimpleSaveDataService : ISaveDataService
    {
        public Task<bool> ExistsAsync(string saveId)
        {
            // TODO: 実装
            return Task.FromResult(false);
        }

        public Task<SaveDataInfo[]> GetSaveDataListAsync()
        {
            // TODO: 実装
            return Task.FromResult(Array.Empty<SaveDataInfo>());
        }

        public Task SaveAsync(string saveId, object gameState)
        {
            // TODO: 実装
            return Task.CompletedTask;
        }

        public Task<T?> LoadAsync<T>(string saveId) where T : class
        {
            // TODO: 実装
            return Task.FromResult<T?>(null);
        }

        public Task DeleteAsync(string saveId)
        {
            // TODO: 実装
            return Task.CompletedTask;
        }
    }

    public class GameStateRepository : IGameStateRepository
    {
        // TODO: 実装
    }

    public class CardEffectProcessor : ICardEffectProcessor
    {
        // TODO: 実装
    }

    public class MasterDataLoader : IMasterDataLoader
    {
        // TODO: 実装
    }

    public class PropertyEvaluator : IPropertyEvaluator
    {
        // TODO: 実装
    }

    public class PathFinder : IPathFinder
    {
        // TODO: 実装
    }

    // 必要なインターフェース定義
    public interface IGameStateRepository { }
    public interface ICardEffectProcessor { }
    public interface IMasterDataLoader { }
    public interface IPropertyEvaluator { }
    public interface IPathFinder { }
}