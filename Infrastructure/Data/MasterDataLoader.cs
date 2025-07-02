using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;
using MomotetsuGame.Core.Interfaces;

namespace MomotetsuGame.Infrastructure.Data
{
    /// <summary>
    /// マスターデータローダー
    /// </summary>
    public class MasterDataLoader : IMasterDataLoader
    {
        private readonly string _dataPath;

        public MasterDataLoader()
        {
            _dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Data");
        }

        /// <summary>
        /// 駅データを読み込む
        /// </summary>
        public async Task<StationNetwork> LoadStationNetworkAsync()
        {
            var network = new StationNetwork();

            try
            {
                // 駅データの読み込み
                var stationsFile = Path.Combine(_dataPath, "stations.json");
                if (File.Exists(stationsFile))
                {
                    var json = await File.ReadAllTextAsync(stationsFile);
                    var stationDtos = JsonConvert.DeserializeObject<List<StationDto>>(json);

                    if (stationDtos != null)
                    {
                        var stationMap = new Dictionary<int, Station>();

                        // まず全駅を作成
                        foreach (var dto in stationDtos)
                        {
                            var station = new Station
                            {
                                Id = dto.Id,
                                Name = dto.Name,
                                Type = Enum.Parse<StationType>(dto.Type),
                                Coordinate = new Coordinate(dto.X, dto.Y),
                                Region = dto.Region
                            };

                            stationMap[dto.Id] = station;
                            network.AddStation(station);
                        }

                        // 次に接続を設定
                        foreach (var dto in stationDtos)
                        {
                            var station = stationMap[dto.Id];
                            foreach (var connectedId in dto.ConnectedStationIds)
                            {
                                if (stationMap.TryGetValue(connectedId, out var connectedStation))
                                {
                                    station.AddConnection(connectedStation);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ログ出力（実際はILoggerを使用）
                Console.WriteLine($"駅データの読み込みに失敗しました: {ex.Message}");
            }

            return network;
        }

        /// <summary>
        /// 物件データを読み込む
        /// </summary>
        public async Task<List<Property>> LoadPropertiesAsync()
        {
            var properties = new List<Property>();

            try
            {
                var propertiesFile = Path.Combine(_dataPath, "properties.json");
                if (File.Exists(propertiesFile))
                {
                    var json = await File.ReadAllTextAsync(propertiesFile);
                    var propertyDtos = JsonConvert.DeserializeObject<List<PropertyDto>>(json);

                    if (propertyDtos != null)
                    {
                        foreach (var dto in propertyDtos)
                        {
                            var property = Property.Create(
                                dto.Name,
                                Enum.Parse<PropertyCategory>(dto.Category),
                                dto.BasePrice,
                                dto.IncomeRate
                            );

                            property.Id = Guid.Parse(dto.Id);
                            property.Description = dto.Description;

                            properties.Add(property);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"物件データの読み込みに失敗しました: {ex.Message}");
            }

            return properties;
        }

        /// <summary>
        /// カードマスターデータを読み込む
        /// </summary>
        public async Task<Dictionary<int, Card>> LoadCardMasterAsync()
        {
            var cardMaster = new Dictionary<int, Card>();

            try
            {
                var cardsFile = Path.Combine(_dataPath, "cards.json");
                if (File.Exists(cardsFile))
                {
                    var json = await File.ReadAllTextAsync(cardsFile);
                    var cardDtos = JsonConvert.DeserializeObject<List<CardDto>>(json);

                    if (cardDtos != null)
                    {
                        foreach (var dto in cardDtos)
                        {
                            var card = CardMaster.CreateCard(dto.Id);
                            if (card != null)
                            {
                                cardMaster[dto.Id] = card;
                            }
                        }
                    }
                }
                else
                {
                    // cards.jsonがない場合は、CardMasterから直接生成
                    // 主要なカードのみ生成
                    var defaultCardIds = new[] { 1, 2, 3, 20, 21, 22, 40, 41 };
                    foreach (var id in defaultCardIds)
                    {
                        var card = CardMaster.CreateCard(id);
                        if (card != null)
                        {
                            cardMaster[id] = card;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"カードデータの読み込みに失敗しました: {ex.Message}");
            }

            return cardMaster;
        }

        /// <summary>
        /// 駅と物件を関連付ける
        /// </summary>
        public void AssociatePropertiesWithStations(StationNetwork network, List<Property> properties)
        {
            // properties.jsonのstationIdを使って関連付け
            var propertyGroups = properties.GroupBy(p => p.Description); // 本来はstationIdを使うべき

            foreach (var station in network.GetAllStations())
            {
                if (station.Type == StationType.Property)
                {
                    // 仮実装：駅名に基づいて物件を関連付け
                    var stationProperties = properties
                        .Where(p => p.Description?.Contains(station.Name) ?? false)
                        .Take(3) // 各駅最大3物件
                        .ToList();

                    foreach (var property in stationProperties)
                    {
                        property.Location = station;
                        station.Properties.Add(property);
                    }

                    // 物件がない場合はデフォルト物件を生成
                    if (!station.Properties.Any())
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            var defaultProperty = Property.Create(
                                $"{station.Name}物件{i + 1}",
                                PropertyCategory.Commerce,
                                (i + 1) * 10000000, // 1000万、2000万、3000万
                                0.10m // 10%
                            );
                            defaultProperty.Location = station;
                            station.Properties.Add(defaultProperty);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// マスターデータローダーインターフェース
    /// </summary>
    public interface IMasterDataLoader
    {
        Task<StationNetwork> LoadStationNetworkAsync();
        Task<List<Property>> LoadPropertiesAsync();
        Task<Dictionary<int, Card>> LoadCardMasterAsync();
        void AssociatePropertiesWithStations(StationNetwork network, List<Property> properties);
    }

    #region DTOクラス

    /// <summary>
    /// 駅データDTO
    /// </summary>
    public class StationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public string Region { get; set; } = string.Empty;
        public List<int> ConnectedStationIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// 物件データDTO
    /// </summary>
    public class PropertyDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public long BasePrice { get; set; }
        public decimal IncomeRate { get; set; }
        public string Description { get; set; } = string.Empty;
        public int StationId { get; set; }
    }

    /// <summary>
    /// カードデータDTO
    /// </summary>
    public class CardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public long Price { get; set; }
    }

    #endregion
}