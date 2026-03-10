using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API;

namespace CSConnectUtils;

public class CSConnectUtilsConfig : BasePluginConfig
{
  [JsonPropertyName("base_password")] public string basepassword { get; set; } = "password";
  [JsonPropertyName("hostname")] public string hostname { get; set; } = "";
}

public class CSConnectUtils : BasePlugin, IPluginConfig<CSConnectUtilsConfig>
{

  // Thanks MatchZy
  private bool IsPlayerAdmin(CCSPlayerController? player, string command = "", params string[] permissions)
  {
    string[] updatedPermissions = permissions.Append("@css/root").ToArray();
    RequiresPermissionsOr attr = new(updatedPermissions)
    {
      Command = command
    };
    if (attr.CanExecuteCommand(player)) return true;
    if (player == null) return true; // Sent via server, hence should be treated as an admin.
    return false;
  }

  public override string ModuleName => "CSConnectUtils";

  public override string ModuleVersion => "0.0.1";

  public override string ModuleAuthor => "Lukseh";

  public override string ModuleDescription => "Simple plugin for managing passwords and making fast connect strings for matches and such.";

  required public CSConnectUtilsConfig Config { get; set; }
  public void OnConfigParsed(CSConnectUtilsConfig CONFIG)
  {
    Config = CONFIG;
    Console.WriteLine("Loaded config file.");
    if (string.IsNullOrEmpty(Config.hostname))
    {
      Console.WriteLine("Hostname is not configured.");
      Unload(false);
      return;
    }
  }

  // Main password variable
  private string CurrentPassword = "";

  public override void Load(bool hotReload)
  {
    Console.WriteLine("---CSConnectUtils---\nLukseh wishes great day!");
    CurrentPassword = Config.basepassword;
    Server.ExecuteCommand($"sv_password {CurrentPassword}");
  }

  [ConsoleCommand("connstring", "Returns ready connection string.")]
  public void onCommandConnString(CCSPlayerController? player, CommandInfo info)
  {
    if (CurrentPassword == "")
    {
      info.ReplyToCommand($"connect {Config.hostname}");
    }
    else
    {
      info.ReplyToCommand($"connect {Config.hostname}; password {CurrentPassword}");
    }
  }

  [ConsoleCommand("setpassword", "Changes password of server.")]
  [CommandHelper(1, "setpassword [new_password]")]
  public void onCommandSetPassword(CCSPlayerController? player, CommandInfo info)
  {
    if (info.ArgCount < 1)
    {
      info.ReplyToCommand("NO PASSWORD SET");
      CurrentPassword = "";
      Server.ExecuteCommand($"sv_password {CurrentPassword}");
    }
    if (IsPlayerAdmin(player))
    {
      CurrentPassword = info.ArgByIndex(1);
      Server.ExecuteCommand($"sv_password {CurrentPassword}");
    }
    else info.ReplyToCommand("You don't have permissions to change command of this server.");
  }

  [ConsoleCommand("resetpassword", "Resets password to base password from Config.")]
  public void onCommandResetPassword(CCSPlayerController? player, CommandInfo info)
  {
    if (IsPlayerAdmin(player))
    {
      CurrentPassword = Config.basepassword;
      Server.ExecuteCommand($"sv_password {CurrentPassword}");
    }
    else info.ReplyToCommand("You don't have permissions to change command of this server.");
  }
  [ConsoleCommand("genpassword", "Generates random password")]
  [CommandHelper(0, "genpassword [print_conn_string] [lenght]")]
  public void OnCommandGenPassword(CCSPlayerController? player, CommandInfo info)
  {
    int len = 10;
    if (info.ArgByIndex(2) != null && int.TryParse(info.ArgByIndex(2), out int parsedLength))
    {
      len = parsedLength;
    }

    bool print = false;

    if (info.ArgByIndex(1) != null && !bool.TryParse(info.ArgByIndex(1), out print)) CurrentPassword = RandomString(len);
    if (print) info.ReplyToCommand($"Generated new password.\nconnect {Config.hostname}; password {CurrentPassword}");
  }
  // Helpers for generation. Thanks stackoverflow // https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
  private static Random random = new Random();
  public static string RandomString(int length)
  {
    const string chars = "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ!@#$%&0123456789";
    return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
  }

}