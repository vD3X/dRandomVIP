using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using Microsoft.Extensions.Logging;

namespace dRandomVIP;

public class dRandomVIP : BasePlugin
{
    public override string ModuleName => "[CS2] D3X - [ Random VIP ]";
    public override string ModuleAuthor => "D3X";
    public override string ModuleDescription => "Po x rundach od startu mapy, losuje randomowego vipa.";
    public override string ModuleVersion => "1.0.1";

    private static readonly Random _random = new Random();
    private CCSPlayerController _randomVIP;
    private int _roundCounter;
    public static dRandomVIP Instance { get; private set; } = new dRandomVIP();

    public override void Load(bool hotReload)
    {
        Instance = this;
        
        Config.Initialize();
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
    }

    private string FormatMessage(string message, string playerName = "")
    {
        var prefix = Config.config.Settings.Prefix;

        var colorMapping = new Dictionary<string, string>
        {
            { "DEFAULT", "\x01" },
            { "WHITE", "\x01" },
            { "DARKRED", "\x02" },
            { "GREEN", "\x04" },
            { "LIGHTYELLOW", "\x09" },
            { "LIGHTBLUE", "\x0B" },
            { "OLIVE", "\x05" },
            { "LIME", "\x06" },
            { "RED", "\x07" },
            { "LIGHTPURPLE", "\x03" },
            { "PURPLE", "\x0E" },
            { "GREY", "\x08" },
            { "YELLOW", "\x09" },
            { "GOLD", "\x10" },
            { "SILVER", "\x0A" },
            { "BLUE", "\x0B" },
            { "DARKBLUE", "\x0C" },
            { "BLUEGREY", "\x0A" },
            { "MAGENTA", "\x0E" },
            { "LIGHTRED", "\x0F" },
            { "ORANGE", "\x10" },
            { "PLAYERNAME", playerName }
        };

        foreach (var entry in colorMapping)
        {
            prefix = prefix.Replace($"{{{entry.Key}}}", entry.Value);
        }

        foreach (var entry in colorMapping)
        {
            message = message.Replace($"{{{entry.Key}}}", entry.Value);
        }

        return $" {prefix} {message}";
    }

    private string GetMessage(string key, string playerName = "")
    {
        var messages = Config.LoadedConfig.Messages;
        var message = key switch
        {
            "NotEnoughPlayers" => messages.NotEnoughPlayers,
            "AllPlayersAreVIP" => messages.AllPlayersAreVIP,
            "DrawingStarted" => messages.DrawingStarted,
            "DrawingSeparator" => messages.DrawingSeparator,
            "VIPSelected" => messages.VIPSelected,
            _ => $"[Missing Message: {key}]"
        };

        return FormatMessage(message, playerName);
    }

    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        int requiredRounds = Config.config.Settings.AfterXRounds;
        int minPlayers = Config.config.Settings.MinPlayers;

        _roundCounter++;

        if (_roundCounter == requiredRounds)
        {	
            if (Utilities.GetPlayers().Count >= minPlayers)
            {
                SelectRandomVIP();
            }
            else
            {
                Server.PrintToChatAll(GetMessage("NotEnoughPlayers"));
            }
        }

        return HookResult.Continue;
    }

    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (_randomVIP != null && @event.Userid == _randomVIP)
        {
            AdminManager.RemovePlayerPermissions(_randomVIP, Config.config.Settings.Flag);
        }

        return HookResult.Continue;
    }

    public void OnMapStart(string mapName)
    {   
        _roundCounter = 0;
    }

    public void SelectRandomVIP()
    {
        var eligiblePlayers = GetEligiblePlayers();

        if (!eligiblePlayers.Any())
        {
            Server.PrintToChatAll(GetMessage("AllPlayersAreVIP"));
            return;
        }

        Server.PrintToChatAll(GetMessage("DrawingStarted"));
        Server.PrintToChatAll(GetMessage("DrawingSeparator"));
        Server.PrintToChatAll(GetMessage("DrawingSeparator"));
        Server.PrintToChatAll(GetMessage("DrawingSeparator"));

        _randomVIP = eligiblePlayers[_random.Next(eligiblePlayers.Count)];

        if (_randomVIP != null && _randomVIP.IsValid)
        {
            var message = GetMessage("VIPSelected", _randomVIP.PlayerName);
            Server.PrintToChatAll(message);
            AdminManager.AddPlayerPermissions(_randomVIP, Config.config.Settings.Flag);
        }
    }

    private List<CCSPlayerController> GetEligiblePlayers()
    {
        return Utilities.GetPlayers()
            .Where(p => p != null && p.IsValid && p.PlayerPawn != null && p.PlayerPawn.IsValid && !p.IsBot && !p.IsHLTV && p.Connected == PlayerConnectedState.PlayerConnected)
            .Where(p => !AdminManager.PlayerHasPermissions(p, Config.LoadedConfig.Settings.Flag))
            .ToList();
    }
}