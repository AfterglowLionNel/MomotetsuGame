using System;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.Interfaces;
using MomotetsuGame.Infrastructure.Data;

namespace MomotetsuGame.Infrastructure.Services
{
    /// <summary>
    /// シンプルなセーブデータサービス実装
    /// </summary>
    public class SimpleSaveDataService : ISaveDataService
    {
        private readonly IGameStateRepository _repository;

        public SimpleSaveDataService()
        {
            _repository = new GameStateRepository();
        }

        /// <summary>
        /// セーブデータの存在確認
        /// </summary>
        public async Task<bool> ExistsAsync(string saveId)
        {
            return await _repository.ExistsAsync(saveId);
        }

        /// <summary>
        /// セーブデータ一覧を取得
        /// </summary>
        public async Task<SaveDataInfo[]> GetSaveDataListAsync()
        {
            var saveDataList = await _repository.GetSaveDataListAsync();
            return saveDataList.ToArray();
        }

        /// <summary>
        /// ゲームをセーブ
        /// </summary>
        public async Task SaveAsync(string saveId, object gameState)
        {
            if (gameState is Core.Entities.GameState gs)
            {
                await _repository.SaveGameStateAsync(saveId, gs);
            }
            else
            {
                throw new ArgumentException("gameState must be of type GameState", nameof(gameState));
            }
        }

        /// <summary>
        /// ゲームをロード
        /// </summary>
        public async Task<T?> LoadAsync<T>(string saveId) where T : class
        {
            if (typeof(T) == typeof(Core.Entities.GameState))
            {
                var gameState = await _repository.LoadGameStateAsync(saveId);
                return gameState as T;
            }

            return null;
        }

        /// <summary>
        /// セーブデータを削除
        /// </summary>
        public async Task DeleteAsync(string saveId)
        {
            await _repository.DeleteSaveDataAsync(saveId);
        }
    }
}