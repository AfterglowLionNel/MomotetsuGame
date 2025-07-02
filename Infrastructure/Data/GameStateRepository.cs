using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.Interfaces;
using MomotetsuGame.Core.ValueObjects;

namespace MomotetsuGame.Infrastructure.Data
{
    /// <summary>
    /// ゲーム状態リポジトリ
    /// </summary>
    public class GameStateRepository : IGameStateRepository
    {
        private readonly string _saveDirectory;
        private readonly JsonSerializerSettings _jsonSettings;

        public GameStateRepository()
        {
            _saveDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MomotetsuGame",
                "SaveData"
            );

            // セーブディレクトリが存在しない場合は作成
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }

            // JSON設定
            _jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Converters = new List<JsonConverter>
                {
                    new MoneyJsonConverter(),
                    new CoordinateJsonConverter()
                }
            };
        }

        /// <summary>
        /// ゲーム状態を保存
        /// </summary>
        public async Task<bool> SaveGameStateAsync(string saveId, GameState gameState)
        {
            try
            {
                var saveData = new SaveData
                {
                    Id = saveId,
                    SavedAt = DateTime.Now,
                    GameState = gameState,
                    Version = "1.0.0"
                };

                var json = JsonConvert.SerializeObject(saveData, _jsonSettings);
                var filePath = GetSaveFilePath(saveId);

                await File.WriteAllTextAsync(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"セーブに失敗しました: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ゲーム状態を読み込み
        /// </summary>
        public async Task<GameState?> LoadGameStateAsync(string saveId)
        {
            try
            {
                var filePath = GetSaveFilePath(saveId);
                if (!File.Exists(filePath))
                {
                    return null;
                }

                var json = await File.ReadAllTextAsync(filePath);
                var saveData = JsonConvert.DeserializeObject<SaveData>(json, _jsonSettings);

                return saveData?.GameState;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ロードに失敗しました: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// セーブデータの存在確認
        /// </summary>
        public Task<bool> ExistsAsync(string saveId)
        {
            var filePath = GetSaveFilePath(saveId);
            return Task.FromResult(File.Exists(filePath));
        }

        /// <summary>
        /// セーブデータ一覧を取得
        /// </summary>
        public async Task<List<SaveDataInfo>> GetSaveDataListAsync()
        {
            var saveDataList = new List<SaveDataInfo>();

            try
            {
                var files = Directory.GetFiles(_saveDirectory, "*.save");

                foreach (var file in files)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var saveData = JsonConvert.DeserializeObject<SaveData>(json, _jsonSettings);

                        if (saveData?.GameState != null)
                        {
                            var info = new SaveDataInfo
                            {
                                Id = saveData.Id,
                                Name = Path.GetFileNameWithoutExtension(file),
                                SavedAt = saveData.SavedAt,
                                Year = saveData.GameState.CurrentYear,
                                Month = saveData.GameState.CurrentMonth,
                                PlayerName = saveData.GameState.Players.FirstOrDefault()?.Name ?? "Unknown",
                                TotalAssets = saveData.GameState.Players.FirstOrDefault()?.TotalAssets.Value ?? 0
                            };

                            saveDataList.Add(info);
                        }
                    }
                    catch
                    {
                        // 個別のファイル読み込みエラーは無視
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"セーブデータ一覧の取得に失敗しました: {ex.Message}");
            }

            return saveDataList.OrderByDescending(s => s.SavedAt).ToList();
        }

        /// <summary>
        /// セーブデータを削除
        /// </summary>
        public Task<bool> DeleteSaveDataAsync(string saveId)
        {
            try
            {
                var filePath = GetSaveFilePath(saveId);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"セーブデータの削除に失敗しました: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// 自動セーブ
        /// </summary>
        public async Task<bool> AutoSaveAsync(GameState gameState)
        {
            return await SaveGameStateAsync("autosave", gameState);
        }

        /// <summary>
        /// セーブファイルのパスを取得
        /// </summary>
        private string GetSaveFilePath(string saveId)
        {
            return Path.Combine(_saveDirectory, $"{saveId}.save");
        }
    }

    /// <summary>
    /// ゲーム状態リポジトリインターフェース
    /// </summary>
    public interface IGameStateRepository
    {
        Task<bool> SaveGameStateAsync(string saveId, GameState gameState);
        Task<GameState?> LoadGameStateAsync(string saveId);
        Task<bool> ExistsAsync(string saveId);
        Task<List<SaveDataInfo>> GetSaveDataListAsync();
        Task<bool> DeleteSaveDataAsync(string saveId);
        Task<bool> AutoSaveAsync(GameState gameState);
    }

    /// <summary>
    /// セーブデータ
    /// </summary>
    public class SaveData
    {
        public string Id { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; }
        public GameState GameState { get; set; } = null!;
        public string Version { get; set; } = string.Empty;
    }

    #region JsonConverters

    /// <summary>
    /// Money型のJsonConverter
    /// </summary>
    public class MoneyJsonConverter : JsonConverter<Money>
    {
        public override Money ReadJson(JsonReader reader, Type objectType, Money existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
            {
                return new Money(Convert.ToInt64(reader.Value));
            }
            return Money.Zero;
        }

        public override void WriteJson(JsonWriter writer, Money value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Value);
        }
    }

    /// <summary>
    /// Coordinate型のJsonConverter
    /// </summary>
    public class CoordinateJsonConverter : JsonConverter<Coordinate>
    {
        public override Coordinate ReadJson(JsonReader reader, Type objectType, Coordinate existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var obj = serializer.Deserialize<dynamic>(reader);
                return new Coordinate((double)obj.X, (double)obj.Y);
            }
            return new Coordinate(0, 0);
        }

        public override void WriteJson(JsonWriter writer, Coordinate value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            writer.WriteValue(value.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(value.Y);
            writer.WriteEndObject();
        }
    }

    #endregion
}