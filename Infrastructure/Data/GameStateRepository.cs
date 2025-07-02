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
    /// �Q�[����ԃ��|�W�g��
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

            // �Z�[�u�f�B���N�g�������݂��Ȃ��ꍇ�͍쐬
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }

            // JSON�ݒ�
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
        /// �Q�[����Ԃ�ۑ�
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
                Console.WriteLine($"�Z�[�u�Ɏ��s���܂���: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// �Q�[����Ԃ�ǂݍ���
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
                Console.WriteLine($"���[�h�Ɏ��s���܂���: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// �Z�[�u�f�[�^�̑��݊m�F
        /// </summary>
        public Task<bool> ExistsAsync(string saveId)
        {
            var filePath = GetSaveFilePath(saveId);
            return Task.FromResult(File.Exists(filePath));
        }

        /// <summary>
        /// �Z�[�u�f�[�^�ꗗ���擾
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
                        // �ʂ̃t�@�C���ǂݍ��݃G���[�͖���
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"�Z�[�u�f�[�^�ꗗ�̎擾�Ɏ��s���܂���: {ex.Message}");
            }

            return saveDataList.OrderByDescending(s => s.SavedAt).ToList();
        }

        /// <summary>
        /// �Z�[�u�f�[�^���폜
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
                Console.WriteLine($"�Z�[�u�f�[�^�̍폜�Ɏ��s���܂���: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// �����Z�[�u
        /// </summary>
        public async Task<bool> AutoSaveAsync(GameState gameState)
        {
            return await SaveGameStateAsync("autosave", gameState);
        }

        /// <summary>
        /// �Z�[�u�t�@�C���̃p�X���擾
        /// </summary>
        private string GetSaveFilePath(string saveId)
        {
            return Path.Combine(_saveDirectory, $"{saveId}.save");
        }
    }

    /// <summary>
    /// �Q�[����ԃ��|�W�g���C���^�[�t�F�[�X
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
    /// �Z�[�u�f�[�^
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
    /// Money�^��JsonConverter
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
    /// Coordinate�^��JsonConverter
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