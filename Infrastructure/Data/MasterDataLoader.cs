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
    /// �}�X�^�[�f�[�^���[�_�[
    /// </summary>
    public class MasterDataLoader : IMasterDataLoader
    {
        private readonly string _dataPath;

        public MasterDataLoader()
        {
            _dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Data");
        }

        /// <summary>
        /// �w�f�[�^��ǂݍ���
        /// </summary>
        public async Task<StationNetwork> LoadStationNetworkAsync()
        {
            var network = new StationNetwork();

            try
            {
                // �w�f�[�^�̓ǂݍ���
                var stationsFile = Path.Combine(_dataPath, "stations.json");
                if (File.Exists(stationsFile))
                {
                    var json = await File.ReadAllTextAsync(stationsFile);
                    var stationDtos = JsonConvert.DeserializeObject<List<StationDto>>(json);

                    if (stationDtos != null)
                    {
                        var stationMap = new Dictionary<int, Station>();

                        // �܂��S�w���쐬
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

                        // ���ɐڑ���ݒ�
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
                // ���O�o�́i���ۂ�ILogger���g�p�j
                Console.WriteLine($"�w�f�[�^�̓ǂݍ��݂Ɏ��s���܂���: {ex.Message}");
            }

            return network;
        }

        /// <summary>
        /// �����f�[�^��ǂݍ���
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
                Console.WriteLine($"�����f�[�^�̓ǂݍ��݂Ɏ��s���܂���: {ex.Message}");
            }

            return properties;
        }

        /// <summary>
        /// �J�[�h�}�X�^�[�f�[�^��ǂݍ���
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
                    // cards.json���Ȃ��ꍇ�́ACardMaster���璼�ڐ���
                    // ��v�ȃJ�[�h�̂ݐ���
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
                Console.WriteLine($"�J�[�h�f�[�^�̓ǂݍ��݂Ɏ��s���܂���: {ex.Message}");
            }

            return cardMaster;
        }

        /// <summary>
        /// �w�ƕ������֘A�t����
        /// </summary>
        public void AssociatePropertiesWithStations(StationNetwork network, List<Property> properties)
        {
            // properties.json��stationId���g���Ċ֘A�t��
            var propertyGroups = properties.GroupBy(p => p.Description); // �{����stationId���g���ׂ�

            foreach (var station in network.GetAllStations())
            {
                if (station.Type == StationType.Property)
                {
                    // �������F�w���Ɋ�Â��ĕ������֘A�t��
                    var stationProperties = properties
                        .Where(p => p.Description?.Contains(station.Name) ?? false)
                        .Take(3) // �e�w�ő�3����
                        .ToList();

                    foreach (var property in stationProperties)
                    {
                        property.Location = station;
                        station.Properties.Add(property);
                    }

                    // �������Ȃ��ꍇ�̓f�t�H���g�����𐶐�
                    if (!station.Properties.Any())
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            var defaultProperty = Property.Create(
                                $"{station.Name}����{i + 1}",
                                PropertyCategory.Commerce,
                                (i + 1) * 10000000, // 1000���A2000���A3000��
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
    /// �}�X�^�[�f�[�^���[�_�[�C���^�[�t�F�[�X
    /// </summary>
    public interface IMasterDataLoader
    {
        Task<StationNetwork> LoadStationNetworkAsync();
        Task<List<Property>> LoadPropertiesAsync();
        Task<Dictionary<int, Card>> LoadCardMasterAsync();
        void AssociatePropertiesWithStations(StationNetwork network, List<Property> properties);
    }

    #region DTO�N���X

    /// <summary>
    /// �w�f�[�^DTO
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
    /// �����f�[�^DTO
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
    /// �J�[�h�f�[�^DTO
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