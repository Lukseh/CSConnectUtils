using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API;

namespace CSConnectUtils;

public class CSConnetUtilsConfig : BasePluginConfig
{
  [JsonPropertyName("base_password")] public string basepassword { get; set; } = "password";
  [JsonPropertyName("hostname")] public string hostname { get; set; } = "";
}

public class CSConnectUtils : BasePlugin, IPluginConfig<CSConnetUtilsConfig>
{

  // Thanks MatchZy
  private bool IsPlayerAdmin(CCSPlayerController? player, string command = "", params string[] permissions)
  {
    string[] updatedPermissions = permissions.Concat(new[] { "@css/root" }).ToArray();
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

  public override string ModuleDescription => "Simple plugin for managin passwords and making fast connect strings for matches and such.";

  required public CSConnetUtilsConfig Config { get; set; }
  public void OnConfigParsed(CSConnetUtilsConfig CONFIG)
  {
    Config = CONFIG;
    Console.WriteLine("Loaded config file.");
  }

  // Main password variable
  public string CurrentPassword = "";

  public override void Load(bool hotReload)
  {
    Console.WriteLine("---CSConnectUtils---\nLukseh wishes great day!");
    CurrentPassword = Config.basepassword;
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
  public void onSetPasswordCommand(CCSPlayerController? player, CommandInfo info)
  {
    if (!IsPlayerAdmin(player))
    {
      CurrentPassword = info.ArgByIndex(1);
      Server.ExecuteCommand($"sv_password {CurrentPassword}");
    }
    else info.ReplyToCommand("You don't have permissions to change command of this server.");
  }

  [ConsoleCommand("resetpassword", "Resets password to base password from Config.")]
  public void onResetPassword(CCSPlayerController? player, CommandInfo info)
  {
    if (!IsPlayerAdmin(player))
    {
      CurrentPassword = Config.basepassword;
      Server.ExecuteCommand($"sv_password {CurrentPassword}");
    }
    else info.ReplyToCommand("You don't have permissions to change command of this server.");
  }

}