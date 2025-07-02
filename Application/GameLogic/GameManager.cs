using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MomotetsuGame.Core.Entities;
using MomotetsuGame.Core.Enums;
using MomotetsuGame.Core.Interfaces;
using MomotetsuGame.Core.ValueObjects;
using MomotetsuGame.Application.Services;

namespace MomotetsuGame.Application.GameLogic
{
    /// <summary>
    /// �Q�[���Ǘ��N���X
    /// </summary>
    public class GameManager : IGameManager
    {
        private readonly IDiceService _diceService;
        private readonly IRouteCalculator _routeCalculator;
        private readonly IPropertyService _propertyService;
        private readonly IComputerAI? _computerAI;
        private GameState? _currentGameState;

        // �C�x���g
        public event EventHandler<GameEventArgs>? GameEvent;
        public event EventHandler<TurnChangedEventArgs>? TurnChanged;
        public event EventHandler<PlayerMovedEventArgs>? PlayerMoved;
        public event EventHandler<PropertyTransactionEventArgs>? PropertyPurchased;
        public event EventHandler<PropertyTransactionEventArgs>? PropertySold;
        public event EventHandler<CardUsedEventArgs>? CardUsed;
        public event EventHandler<GameMessageEventArgs>? GameMessage;

        /// <summary>
        /// ���݂̃Q�[�����
        /// </summary>
        public GameState CurrentGameState => _currentGameState ?? throw new InvalidOperationException("�Q�[�����J�n����Ă��܂���B");

        /// <summary>
        /// ���݂̃v���C���[
        /// </summary>
        public Player CurrentPlayer => CurrentGameState.GetCurrentPlayer();

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public GameManager(
            IDiceService diceService,
            IRouteCalculator routeCalculator,
            IPropertyService propertyService,
            IComputerAI? computerAI = null)
        {
            _diceService = diceService ?? throw new ArgumentNullException(nameof(diceService));
            _routeCalculator = routeCalculator ?? throw new ArgumentNullException(nameof(routeCalculator));
            _propertyService = propertyService ?? throw new ArgumentNullException(nameof(propertyService));
            _computerAI = computerAI;

            // PropertyService�̃C�x���g���w��
            _propertyService.PropertyPurchased += OnPropertyServicePurchased;
            _propertyService.PropertySold += OnPropertyServiceSold;
        }

        /// <summary>
        /// �V�����Q�[�����J�n
        /// </summary>
        public async Task<GameState> StartNewGameAsync(GameSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            // �Q�[����Ԃ�������
            _currentGameState = new GameState
            {
                GameMode = settings.GameMode,
                MaxYears = settings.MaxYears,
                Settings = settings
            };

            // �v���C���[���쐬
            CreatePlayers(settings);

            // �X�e�[�V�����l�b�g���[�N���������i�������j
            InitializeStationNetwork();

            // �����ړI�n��ݒ�
            SetInitialDestination();

            // �Q�[���J�n�C�x���g
            RaiseGameEvent("�Q�[�����J�n���܂����I");

            // �ŏ��̃^�[�����J�n
            await StartTurnAsync();

            return _currentGameState;
        }

        /// <summary>
        /// �Q�[�������[�h
        /// </summary>
        public async Task<GameState> LoadGameAsync(string saveId)
        {
            // TODO: �����iRepository���g�p�j
            throw new NotImplementedException("�Z�[�u/���[�h�@�\�͖������ł��B");
        }

        /// <summary>
        /// �Q�[�����Z�[�u
        /// </summary>
        public async Task SaveGameAsync(string saveId)
        {
            // TODO: �����iRepository���g�p�j
            throw new NotImplementedException("�Z�[�u/���[�h�@�\�͖������ł��B");
        }

        /// <summary>
        /// �T�C�R����U��
        /// </summary>
        public async Task<DiceResult> RollDiceAsync()
        {
            var player = CurrentPlayer;

            // �s���ς݃`�F�b�N
            if (CurrentGameState.CurrentPhase != GamePhase.Action)
            {
                throw new InvalidOperationException("���݂̓T�C�R����U�邱�Ƃ��ł��܂���B");
            }

            // �T�C�R����U��
            var result = await _diceService.RollForPlayerAsync(player);

            RaiseGameEvent($"{player.Name}�̓T�C�R����U����{result.Total}���o�܂����I");

            // �t�F�[�Y���ړ��ɕύX
            CurrentGameState.CurrentPhase = GamePhase.Movement;

            return result;
        }

        /// <summary>
        /// �v���C���[���ړ�
        /// </summary>
        public async Task<MoveResult> MovePlayerAsync(Player player, int steps)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (player.CurrentStation == null)
                throw new InvalidOperationException("�v���C���[�̌��݈ʒu���ݒ肳��Ă��܂���B");

            var startStation = player.CurrentStation;
            var route = _routeCalculator.CalculateRoute(startStation, steps);

            // ����_�`�F�b�N
            if (route.Contains(null!))
            {
                // ����_������ꍇ��UI�ɑI�����ς˂�
                var branchIndex = route.IndexOf(null!);
                var partialRoute = route.Take(branchIndex).ToList();

                return new MoveResult
                {
                    Player = player,
                    Route = partialRoute,
                    FinalStation = partialRoute.Last(),
                    RequiresBranchSelection = true,
                    RemainingSteps = steps - partialRoute.Count + 1
                };
            }

            // �ړ����s
            var finalStation = route.Last();
            player.CurrentStation = finalStation;

            var moveResult = new MoveResult
            {
                Player = player,
                Route = route,
                FinalStation = finalStation,
                ReachedDestination = finalStation.IsDestination
            };

            // �ړ��C�x���g����
            PlayerMoved?.Invoke(this, new PlayerMovedEventArgs
            {
                Player = player,
                FromStation = startStation,
                ToStation = finalStation,
                Route = route,
                Message = $"{player.Name}��{finalStation.Name}�ɓ������܂����B"
            });

            // ��������
            await ProcessArrivalAsync(player, finalStation);

            return moveResult;
        }

        /// <summary>
        /// �J�[�h���g�p
        /// </summary>
        public async Task<CardEffectResult> UseCardAsync(Card card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            var player = CurrentPlayer;

            if (!player.Cards.Contains(card))
            {
                return CardEffectResult.CreateFailure("���̃J�[�h���������Ă��܂���B");
            }

            if (!card.CanUse(player, CurrentGameState))
            {
                return CardEffectResult.CreateFailure("���̃J�[�h�͌��ݎg�p�ł��܂���B");
            }

            // �J�[�h���ʂ̎��s�i�������j
            var result = new CardEffectResult
            {
                Success = true,
                Message = $"{card.Name}���g�p���܂����I"
            };

            // �J�[�h�g�p����
            player.UseCard(card);

            // �C�x���g����
            CardUsed?.Invoke(this, new CardUsedEventArgs
            {
                Player = player,
                Card = card,
                Result = result
            });

            return result;
        }

        /// <summary>
        /// �������w��
        /// </summary>
        public async Task<bool> PurchasePropertyAsync(Player player, Property property)
        {
            var result = await _propertyService.PurchasePropertyAsync(player, property);

            if (result.Success)
            {
                RaiseGameEvent(result.Message);
            }

            return result.Success;
        }

        /// <summary>
        /// �����𔄋p
        /// </summary>
        public async Task<bool> SellPropertyAsync(Player player, Property property)
        {
            var result = await _propertyService.SellPropertyAsync(player, property);

            if (result.Success)
            {
                RaiseGameEvent(result.Message);
            }

            return result.Success;
        }

        /// <summary>
        /// �^�[�����I��
        /// </summary>
        public async Task EndTurnAsync()
        {
            if (CurrentGameState.CurrentPhase != GamePhase.TurnEnd &&
                CurrentGameState.CurrentPhase != GamePhase.Arrival)
            {
                throw new InvalidOperationException("�܂��^�[�����I���ł��܂���B");
            }

            // �v���C���[�̃^�[���I������
            CurrentPlayer.ProcessTurnEnd();

            // ���̃v���C���[��
            CurrentGameState.MoveToNextPlayer();

            // ���̃^�[�����J�n
            await StartTurnAsync();
        }

        /// <summary>
        /// �ړI�n��������
        /// </summary>
        public async Task ProcessDestinationArrivalAsync(Player player)
        {
            CurrentGameState.ProcessDestinationArrival(player);
            await Task.CompletedTask;
        }

        /// <summary>
        /// �ړ��\�ȉw���擾
        /// </summary>
        public List<Station> GetReachableStations(Station from, int steps)
        {
            return _routeCalculator.GetReachableStations(from, steps);
        }

        /// <summary>
        /// �Q�[���I���`�F�b�N
        /// </summary>
        public bool IsGameOver()
        {
            return CurrentGameState.IsGameOver();
        }

        /// <summary>
        /// �ŏI���ʂ��擾
        /// </summary>
        public GameResult GetGameResult()
        {
            var result = new GameResult
            {
                TotalTurns = CurrentGameState.CurrentTurn,
                Years = CurrentGameState.CurrentYear
            };

            // �v���C���[���ʂ��W�v
            foreach (var player in CurrentGameState.Players.OrderBy(p => p.Rank))
            {
                result.PlayerResults.Add(new PlayerResult
                {
                    Player = player,
                    FinalRank = player.Rank,
                    FinalAssets = player.TotalAssets,
                    PropertyCount = player.OwnedProperties.Count
                });
            }

            result.Winner = result.PlayerResults.First().Player;

            return result;
        }

        #region Private Methods

        /// <summary>
        /// �v���C���[���쐬
        /// </summary>
        private void CreatePlayers(GameSettings settings)
        {
            // �l�ԃv���C���[
            var humanPlayer = Player.Create(settings.PlayerName, true, settings.PlayerColor);
            CurrentGameState.Players.Add(humanPlayer);

            // COM�v���C���[
            var comColors = new[] { PlayerColor.Red, PlayerColor.Green, PlayerColor.Yellow, PlayerColor.Purple };
            for (int i = 0; i < settings.ComPlayerCount; i++)
            {
                var comPlayer = Player.Create($"COM{i + 1}", false, comColors[i]);
                CurrentGameState.Players.Add(comPlayer);
            }
        }

        /// <summary>
        /// �X�e�[�V�����l�b�g���[�N���������i�������j
        /// </summary>
        private void InitializeStationNetwork()
        {
            // ���̉w���쐬
            var tokyo = Station.CreatePropertyStation(1, "����", 400, 300, Region.Kanto);
            var shinjuku = Station.CreatePropertyStation(2, "�V�h", 350, 280, Region.Kanto);
            var shinagawa = Station.CreatePropertyStation(3, "�i��", 450, 320, Region.Kanto);
            var ikebukuro = Station.CreatePropertyStation(4, "�r��", 300, 250, Region.Kanto);
            var yokohama = Station.CreatePropertyStation(5, "���l", 500, 350, Region.Kanto);

            // �w��ǉ�
            CurrentGameState.StationNetwork.AddStation(tokyo);
            CurrentGameState.StationNetwork.AddStation(shinjuku);
            CurrentGameState.StationNetwork.AddStation(shinagawa);
            CurrentGameState.StationNetwork.AddStation(ikebukuro);
            CurrentGameState.StationNetwork.AddStation(yokohama);

            // �ڑ���ǉ�
            CurrentGameState.StationNetwork.AddConnection(1, 2); // ����-�V�h
            CurrentGameState.StationNetwork.AddConnection(1, 3); // ����-�i��
            CurrentGameState.StationNetwork.AddConnection(2, 4); // �V�h-�r��
            CurrentGameState.StationNetwork.AddConnection(3, 5); // �i��-���l

            // ���̕�����ǉ�
            tokyo.Properties.Add(Property.Create("�����^���[", PropertyCategory.Tourism, 10000000000, 0.15m));
            tokyo.Properties.Add(Property.Create("�c��", PropertyCategory.Tourism, 20000000000, 0.12m));

            shinjuku.Properties.Add(Property.Create("�V�h�w�r��", PropertyCategory.Commerce, 8000000000, 0.10m));

            // �e�v���C���[�̏����ʒu��ݒ�
            foreach (var player in CurrentGameState.Players)
            {
                player.CurrentStation = tokyo;
            }
        }

        /// <summary>
        /// �����ړI�n��ݒ�
        /// </summary>
        private void SetInitialDestination()
        {
            var yokohama = CurrentGameState.StationNetwork.GetStation(5);
            if (yokohama != null)
            {
                CurrentGameState.Destination = yokohama;
                yokohama.IsDestination = true;
                RaiseGameEvent($"�ŏ��̖ړI�n��{yokohama.Name}�ł��I");
            }
        }

        /// <summary>
        /// �^�[�����J�n
        /// </summary>
        private async Task StartTurnAsync()
        {
            CurrentGameState.CurrentPhase = GamePhase.TurnStart;

            // �^�[���ύX�C�x���g
            TurnChanged?.Invoke(this, new TurnChangedEventArgs
            {
                CurrentPlayer = CurrentPlayer,
                Turn = CurrentGameState.CurrentTurn,
                Year = CurrentGameState.CurrentYear,
                Month = CurrentGameState.CurrentMonth,
                Message = $"{CurrentPlayer.Name}�̃^�[���ł��B"
            });

            CurrentGameState.CurrentPhase = GamePhase.Action;

            // COM�̏ꍇ�͎����v���C
            if (!CurrentPlayer.IsHuman && _computerAI != null)
            {
                await ProcessComputerTurnAsync();
            }
        }

        /// <summary>
        /// COM�̃^�[��������
        /// </summary>
        private async Task ProcessComputerTurnAsync()
        {
            await Task.Delay(1000); // �v�l���Ԃ̃V�~�����[�g

            // �T�C�R����U��
            var diceResult = await RollDiceAsync();

            await Task.Delay(500);

            // �ړ�
            var moveResult = await MovePlayerAsync(CurrentPlayer, diceResult.Total);

            // ����I�����K�v�ȏꍇ
            if (moveResult.RequiresBranchSelection)
            {
                // TODO: AI������ɕ���I�����W�b�N��ǉ�
            }

            await Task.Delay(1000);

            // �^�[���I��
            await EndTurnAsync();
        }

        /// <summary>
        /// ��������
        /// </summary>
        private async Task ProcessArrivalAsync(Player player, Station station)
        {
            CurrentGameState.CurrentPhase = GamePhase.Arrival;

            switch (station.Type)
            {
                case StationType.Property:
                    await ProcessPropertyStationAsync(player, station);
                    break;

                case StationType.Plus:
                    ProcessPlusStation(player);
                    break;

                case StationType.Minus:
                    ProcessMinusStation(player);
                    break;

                    // TODO: ���̉w�^�C�v�̏���
            }

            // �ړI�n�`�F�b�N
            if (station.IsDestination)
            {
                await ProcessDestinationArrivalAsync(player);
            }

            CurrentGameState.CurrentPhase = GamePhase.TurnEnd;
        }

        /// <summary>
        /// �����w�̏���
        /// </summary>
        private async Task ProcessPropertyStationAsync(Player player, Station station)
        {
            if (player.IsHuman)
            {
                // UI�ŕ����w���_�C�A���O��\���i�C�x���g�o�R�j
                RaiseGameEvent($"{station.Name}�ɓ������܂����B�������w���ł��܂��B");
            }
            else if (_computerAI != null)
            {
                // COM�̕����w�����f
                var availableProperties = station.Properties.Where(p => p.Owner == null).ToList();
                if (availableProperties.Any())
                {
                    // TODO: AI������ɍw�����f���W�b�N��ǉ�
                    var targetProperty = availableProperties.First();
                    if (player.CurrentMoney >= targetProperty.CurrentPrice)
                    {
                        await PurchasePropertyAsync(player, targetProperty);
                    }
                }
            }
        }

        /// <summary>
        /// �v���X�w�̏���
        /// </summary>
        private void ProcessPlusStation(Player player)
        {
            var amount = new Money(10000000); // 1000���~
            player.Receive(amount);
            RaiseGameEvent($"{player.Name}��{amount}���l�����܂����I");
        }

        /// <summary>
        /// �}�C�i�X�w�̏���
        /// </summary>
        private void ProcessMinusStation(Player player)
        {
            var amount = new Money(10000000); // 1000���~
            player.Pay(amount);
            RaiseGameEvent($"{player.Name}��{amount}�������܂����I");
        }

        /// <summary>
        /// �Q�[���C�x���g�𔭐�
        /// </summary>
        private void RaiseGameEvent(string message)
        {
            GameEvent?.Invoke(this, new GameEventArgs { Message = message });
            GameMessage?.Invoke(this, new GameMessageEventArgs { Message = message });
        }

        /// <summary>
        /// PropertyService�̃C�x���g�n���h��
        /// </summary>
        private void OnPropertyServicePurchased(object? sender, PropertyTransactionEventArgs e)
        {
            PropertyPurchased?.Invoke(this, e);
        }

        private void OnPropertyServiceSold(object? sender, PropertyTransactionEventArgs e)
        {
            PropertySold?.Invoke(this, e);
        }

        #endregion
    }

    /// <summary>
    /// �J�[�h�g�p�C�x���g����
    /// </summary>
    public class CardUsedEventArgs : GameEventArgs
    {
        public Player Player { get; set; } = null!;
        public Card Card { get; set; } = null!;
        public CardEffectResult Result { get; set; } = null!;
    }

    /// <summary>
    /// �Q�[�����b�Z�[�W�C�x���g����
    /// </summary>
    public class GameMessageEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// �ړ����ʁi�g���j
    /// </summary>
    public partial class MoveResult
    {
        public bool RequiresBranchSelection { get; set; }
        public int RemainingSteps { get; set; }
    }
}