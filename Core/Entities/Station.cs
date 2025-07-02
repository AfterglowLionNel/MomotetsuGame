using System;
using System.Collections.Generic;
using System.Linq;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Core.Enums;
using Newtonsoft.Json.Linq;

namespace MomotetsuGame.Core.Entities
{
    /// <summary>
    /// �w�G���e�B�e�B
    /// </summary>
    public class Station
    {
        private List<Station> _connectedStations;
        private List<Property> _properties;

        /// <summary>
        /// �wID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// �w��
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// �w���i�Z�k�`�j- UI�\���p
        /// </summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// �w�̈ʒu�i�}�b�v��̍��W�j
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
        /// �w�ōw���\�ȕ������X�g
        /// </summary>
        public List<Property> Properties
        {
            get => _properties;
            set => _properties = value ?? new List<Property>();
        }

        /// <summary>
        /// �ڑ�����Ă���w�̃��X�g
        /// </summary>
        public List<Station> ConnectedStations
        {
            get => _connectedStations;
            set => _connectedStations = value ?? new List<Station>();
        }

        /// <summary>
        /// �J�[�h����ꂪ���邩�ǂ���
        /// </summary>
        public bool HasCardShop { get; set; }

        /// <summary>
        /// �ړI�n�Ƃ��Đݒ肳��Ă��邩�ǂ���
        /// </summary>
        public bool IsDestination { get; set; }

        /// <summary>
        /// ����_���ǂ����i3�ȏ�̉w�Ɛڑ��j
        /// </summary>
        public bool IsBranchPoint => ConnectedStations.Count >= 3;

        /// <summary>
        /// �w�̐F�iUI�\���p�j
        /// </summary>
        public string TypeColor => GetStationTypeColor();

        /// <summary>
        /// �}�b�v���X���W
        /// </summary>
        public double X => Location.X;

        /// <summary>
        /// �}�b�v���Y���W
        /// </summary>
        public double Y => Location.Y;

        /// <summary>
        /// ����w�p�̒ǉ��p�����[�^
        /// </summary>
        public Dictionary<string, object> SpecialParameters { get; set; }

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public Station()
        {
            _connectedStations = new List<Station>();
            _properties = new List<Property>();
            SpecialParameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// �w��ID�̉w���쐬�i�t�@�N�g�����\�b�h�j
        /// </summary>
        public static Station Create(int id, string name, double x, double y, StationType type, Region region)
        {
            return new Station
            {
                Id = id,
                Name = name,
                ShortName = name.Length > 3 ? name.Substring(0, 3) : name,
                Location = new Coordinate(x, y),
                Type = type,
                Region = region
            };
        }

        /// <summary>
        /// �w��ڑ�
        /// </summary>
        public void ConnectTo(Station other)
        {
            if (other == null || other == this) return;

            if (!ConnectedStations.Contains(other))
            {
                ConnectedStations.Add(other);
            }

            if (!other.ConnectedStations.Contains(this))
            {
                other.ConnectedStations.Add(this);
            }
        }

        /// <summary>
        /// �w�̐ڑ�������
        /// </summary>
        public void DisconnectFrom(Station other)
        {
            if (other == null) return;

            ConnectedStations.Remove(other);
            other.ConnectedStations.Remove(this);
        }

        /// <summary>
        /// �w��w�ւ̋������v�Z
        /// </summary>
        public double DistanceTo(Station other)
        {
            if (other == null) return double.MaxValue;
            return Location.DistanceTo(other.Location);
        }

        /// <summary>
        /// �w��w�ƒ��ڐڑ�����Ă��邩
        /// </summary>
        public bool IsConnectedTo(Station other)
        {
            return ConnectedStations.Contains(other);
        }

        /// <summary>
        /// �w���\�ȕ������擾
        /// </summary>
        public List<Property> GetAvailableProperties()
        {
            return Properties.Where(p => p.Owner == null).ToList();
        }

        /// <summary>
        /// �w��v���C���[���Ɛ肵�Ă��邩�`�F�b�N
        /// </summary>
        public bool IsMonopolizedBy(Player player)
        {
            if (Properties.Count == 0) return false;
            return Properties.All(p => p.Owner == player);
        }

        /// <summary>
        /// �w��ʂɉ������F���擾
        /// </summary>
        private string GetStationTypeColor()
        {
            return Type switch
            {
                StationType.Property => "#4169E1",      // RoyalBlue
                StationType.CardShop => "#FF8C00",      // DarkOrange
                StationType.Plus => "#32CD32",          // LimeGreen
                StationType.Minus => "#DC143C",         // Crimson
                StationType.NiceCard => "#FFD700",      // Gold
                StationType.SuperCard => "#FF1493",     // DeepPink
                StationType.CardExchange => "#9370DB",  // MediumPurple
                StationType.Lottery => "#FF69B4",       // HotPink
                _ => "#808080"                          // Gray
            };
        }

        /// <summary>
        /// �v���X�w�̃{�[�i�X���z���擾
        /// </summary>
        public Money GetPlusBonus()
        {
            if (Type != StationType.Plus) return Money.Zero;

            // ��{���z�i1000���~�`1���~�j
            var random = new Random();
            var baseAmount = random.Next(10, 101) * 1000000L;

            // �n��{�[�i�X
            var regionBonus = Region switch
            {
                Region.Hokkaido => 1.2,
                Region.Kanto => 1.5,
                Region.Kinki => 1.4,
                _ => 1.0
            };

            return new Money((long)(baseAmount * regionBonus));
        }

        /// <summary>
        /// �}�C�i�X�w�̃y�i���e�B���z���擾
        /// </summary>
        public Money GetMinusPenalty()
        {
            if (Type != StationType.Minus) return Money.Zero;

            // ��{���z�i1000���~�`5000���~�j
            var random = new Random();
            var baseAmount = random.Next(10, 51) * 1000000L;

            return new Money(baseAmount);
        }

        /// <summary>
        /// ������\��
        /// </summary>
        public override string ToString()
        {
            return $"{Name}�w ({Type})";
        }
    }

    /// <summary>
    /// �w�l�b�g���[�N�Ǘ��N���X
    /// </summary>
    public class StationNetwork
    {
        private readonly List<Station> _stations;
        private readonly Dictionary<int, Station> _stationMap;

        public StationNetwork()
        {
            _stations = new List<Station>();
            _stationMap = new Dictionary<int, Station>();
        }

        /// <summary>
        /// �w��ǉ�
        /// </summary>
        public void AddStation(Station station)
        {
            if (!_stationMap.ContainsKey(station.Id))
            {
                _stations.Add(station);
                _stationMap[station.Id] = station;
            }
        }

        /// <summary>
        /// ID�ŉw���擾
        /// </summary>
        public Station? GetStation(int id)
        {
            return _stationMap.TryGetValue(id, out var station) ? station : null;
        }

        /// <summary>
        /// �S�w���擾
        /// </summary>
        public IReadOnlyList<Station> AllStations => _stations;

        /// <summary>
        /// �n��ŉw������
        /// </summary>
        public List<Station> GetStationsByRegion(Region region)
        {
            return _stations.Where(s => s.Region == region).ToList();
        }

        /// <summary>
        /// ��ʂŉw������
        /// </summary>
        public List<Station> GetStationsByType(StationType type)
        {
            return _stations.Where(s => s.Type == type).ToList();
        }
    }
}