using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2_RandomVIP;

public class RandomVIPConfig : BasePluginConfig
{
    [JsonPropertyName("RandomVIPFlag")] public string RandomVIPFlag { get; set; } = "@css/randomvip";
    [JsonPropertyName("RandomVIPRound")] public int RandomVIPRound { get; set; } = 4;
    [JsonPropertyName("RandomVIPMinPlayers")] public int RandomVIPMinPlayers { get; set; } = 1;
}

public class CS2_RandomVIP : BasePlugin, IPluginConfig<RandomVIPConfig> 
{
    public override string ModuleName => "[CS2] D3X - [ Random VIP ]";
    public override string ModuleAuthor => "D3X";
    public override string ModuleDescription => "After x rounds from the map start, select random VIP.";
    public override string ModuleVersion => "1.0.0";
    public RandomVIPConfig Config { get; set; } = new RandomVIPConfig();
    public CCSPlayerController RandomVIP;
    public int g_iRound;

    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
    }

    public void OnConfigParsed(RandomVIPConfig config)
    {
        Config = config;
    }

    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        int Round = Config.RandomVIPRound;
	    int MinPlayers = Config.RandomVIPMinPlayers;

        g_iRound++;

        if (g_iRound == Round) {	
            if (Utilities.GetPlayers().Count >= MinPlayers)
            {
                GetRandomVIP();
            }
            else
            {
                Server.PrintToChatAll($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} RANDOM VIP {ChatColors.Green}] {ChatColors.Lime}There are too few players to draw  {ChatColors.Purple}VIP{ChatColors.Lime}!");
            }
        }

        return HookResult.Continue;
    }

    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var client = @event.Userid;

        if(client == RandomVIP)
            AdminManager.RemovePlayerPermissions(RandomVIP, Config.RandomVIPFlag);

        return HookResult.Continue;
    }

    public void OnMapStart(string mapName)
    {   
        g_iRound = 0;
    }

    public void GetRandomVIP()
    {
        GetRandomPlayer();
        Server.PrintToChatAll($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} RANDOM VIP {ChatColors.Green}] {ChatColors.Lime}Drawing {ChatColors.Purple}VIP{ChatColors.Lime} is in progress...");
        Server.PrintToChatAll($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} RANDOM VIP {ChatColors.Green}] {ChatColors.Lime}----");
        Server.PrintToChatAll($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} RANDOM VIP {ChatColors.Green}] {ChatColors.Lime}----");
        Server.PrintToChatAll($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} RANDOM VIP {ChatColors.Green}] {ChatColors.Lime}----");
        if (RandomVIP != null && RandomVIP.IsValid)
        {
            Server.PrintToChatAll($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} RANDOM VIP {ChatColors.Green}] {ChatColors.Lime}Random {ChatColors.Purple}VIP {ChatColors.Lime}is awarded to {ChatColors.LightRed}{RandomVIP.PlayerName}{ChatColors.Lime}.");
            AdminManager.AddPlayerPermissions(RandomVIP, Config.RandomVIPFlag);
        }
    }

    public CCSPlayerController GetRandomPlayer()
    {
        List<CCSPlayerController> players;
        getPlayers(out players);

        var rand = new Random();
        int playersCount = players.Count;
        int sortedPlayerIndex = rand.Next(playersCount);
        RandomVIP = players[sortedPlayerIndex];

        return RandomVIP;
    }

    private static void getPlayers(out List<CCSPlayerController> players)
    {
        players = Utilities.GetPlayers().Where(s => s.IsValid).ToList();
    }
}
