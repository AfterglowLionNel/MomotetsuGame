using System;
using System.Collections.Generic;
using System.Linq;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Core.Entities
{
    /// <summary>
    /// �w�G���e�B�e�B
    /// </summary>
    public class Station
    {
        /// <summary>
        /// �wID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// �w��
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// �w�̏ꏊ�i���W�j
        /// </summary>
        public Coordinate Location { get; set; }

        /// <summary>
        /// �w�̎��
        /// </summary>
        public StationType Type { get; set; }

        /// <summary>
        /// �n��
        /// </summary>
        public Region Region { get; set; }

        /// <summary>
        /// ���̉w�̕������X�g
        /// </summary>
        public List<Property> Properties { get; set; }

        /// <summary>
        /// �ڑ�����Ă���w�̃��X�g
        /// </summary>
        public List<Station> ConnectedStations { get; set; }

        /// <summary>
        /// �J�[�h����ꂪ���邩
        /// </summary>
        public bool HasCardShop { get; set; }

        /// <summary>
        /// �ړI�n���ǂ���
        /// </summary>
        public bool IsDestination { get; set; }

        /// <summary>
        /// ����_���ǂ���
        /// </summary>
        public bool IsBranchPoint => ConnectedStations.Count > 2;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public Station()
        {
            Properties = new List<Property>();
            ConnectedStations = new List<Station>();
        }

        /// <summary>
        /// �w���쐬�i�t�@�N�g�����\�b�h�j
        /// </summary>
        public static Station Create(int id, string name, double x, double y, StationType type, Region region)
        {
            return new Station
            {
                Id = id,
                Name = name,
                Location = new Coordinate(x, y),
                Type = type,
                Region = region
            };
        }

        /// <summary>
        /// �����w���쐬
        /// </summary>
        public static Station CreatePropertyStation(int id, string name, double x, double y, Region region)
        {
            return Create(id, name, x, y, StationType.Property, region);
        }

        /// <summary>
        /// �ʂ̉w�܂ł̋������v�Z
        /// </summary>
        public double DistanceTo(Station other)
        {
            if (other == null) return double.MaxValue;
            return Location.DistanceTo(other.Location);
        }

        /// <summary>
        /// �w������̃v���C���[�ɓƐ肳��Ă��邩�`�F�b�N
        /// </summary>
        public bool IsMonopolizedBy(Player player)
        {
            if (Properties.Count == 0) return false;
            return Properties.All(p => p.Owner == player);
        }

        /// <summary>
        /// �w���\�ȕ��������擾
        /// </summary>
        public int GetAvailablePropertyCount()
        {
            return Properties.Count(p => p.Owner == null);
        }

        /// <summary>
        /// ����v���C���[�����L���镨�������擾
        /// </summary>
        public int GetPropertyCountOwnedBy(Player player)
        {
            return Properties.Count(p => p.Owner == player);
        }

        /// <summary>
        /// �w�̑��������l���擾
        /// </summary>
        public Money GetTotalPropertyValue()
        {
            return Properties.Aggregate(Money.Zero, (total, prop) => total + prop.CurrentPrice);
        }

        /// <summary>
        /// �w�̐F���擾�iUI�\���p�j
        /// </summary>
        public string GetStationColor()
        {
            return Type switch
            {
                StationType.Property => "#4169E1",     // RoyalBlue
                StationType.CardShop => "#FF6347",     // Tomato
                StationType.Plus => "#32CD32",         // LimeGreen
                StationType.Minus => "#DC143C",        // Crimson
                StationType.NiceCard => "#FFD700",     // Gold
                StationType.SuperCard => "#FF1493",    // DeepPink
                StationType.CardExchange => "#9370DB", // MediumPurple
                StationType.Lottery => "#FFA500",      // Orange
                _ => "#808080"                         // Gray
            };
        }

        /// <summary>
        /// �Z�k�����擾�i�n�}�\���p�j
        /// </summary>
        public string GetShortName()
        {
            // �����w���͒Z�k
            if (Name.Length > 3)
            {
                // �u�w�v������
                var shortName = Name.Replace("�w", "");

                // ����ł������ꍇ�͍ŏ���3����
                if (shortName.Length > 3)
                {
                    return shortName.Substring(0, 3);
                }

                return shortName;
            }

            return Name;
        }

        /// <summary>
        /// ������\��
        /// </summary>
        public override string ToString()
        {
            return $"{Name} ({Type}) - ������: {Properties.Count}";
        }
    }

    /// <summary>
    /// �w�l�b�g���[�N�i�S�w���Ǘ��j
    /// </summary>
    public class StationNetwork
    {
        private readonly Dictionary<int, Station> _stations;
        private readonly List<(int from, int to)> _connections;

        public StationNetwork()
        {
            _stations = new Dictionary<int, Station>();
            _connections = new List<(int, int)>();
        }

        /// <summary>
        /// �w��ǉ�
        /// </summary>
        public void AddStation(Station station)
        {
            _stations[station.Id] = station;
        }

        /// <summary>
        /// �w�Ԃ̐ڑ���ǉ�
        /// </summary>
        public void AddConnection(int fromId, int toId)
        {
            if (_stations.TryGetValue(fromId, out var fromStation) &&
                _stations.TryGetValue(toId, out var toStation))
            {
                if (!fromStation.ConnectedStations.Contains(toStation))
                {
                    fromStation.ConnectedStations.Add(toStation);
                    toStation.ConnectedStations.Add(fromStation);
                    _connections.Add((fromId, toId));
                }
            }
        }

        /// <summary>
        /// ID����w���擾
        /// </summary>
        public Station? GetStation(int id)
        {
            return _stations.TryGetValue(id, out var station) ? station : null;
        }

        /// <summary>
        /// �S�w���擾
        /// </summary>
        public List<Station> GetAllStations()
        {
            return _stations.Values.ToList();
        }

        /// <summary>
        /// ����^�C�v�̉w���擾
        /// </summary>
        public List<Station> GetStationsByType(StationType type)
        {
            return _stations.Values.Where(s => s.Type == type).ToList();
        }

        /// <summary>
        /// ����n��̉w���擾
        /// </summary>
        public List<Station> GetStationsByRegion(Region region)
        {
            return _stations.Values.Where(s => s.Region == region).ToList();
        }

        /// <summary>
        /// �����w�������_���Ɏ擾
        /// </summary>
        public Station? GetRandomPropertyStation(Random? random = null)
        {
            random ??= new Random();
            var propertyStations = GetStationsByType(StationType.Property);

            if (propertyStations.Count == 0)
                return null;

            return propertyStations[random.Next(propertyStations.Count)];
        }
    }
}