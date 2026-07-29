using System;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace NoClippy
{
    public class NoClippy : IDalamudPlugin
    {
        public static NoClippy Plugin { get; private set; }
        public static Configuration Config { get; private set; }

        public NoClippy(IDalamudPluginInterface pluginInterface)
        {
            Plugin = this;
            DalamudApi.Initialize(this, pluginInterface);

            Config = (Configuration)DalamudApi.PluginInterface.GetPluginConfig() ?? new();
            Config.Initialize();

            try
            {
                Game.Initialize();

                DalamudApi.Framework.Update += Update;
                DalamudApi.PluginInterface.UiBuilder.Draw += PluginUI.Draw;
                DalamudApi.PluginInterface.UiBuilder.OpenConfigUi += ConfigUI.ToggleVisible;

                Modules.Modules.Initialize();
            }
            catch (Exception e)
            {
                PrintError("載入失敗！");
                DalamudApi.LogError(e.ToString());
            }
        }

        [Command("/noclippy")]
        [HelpMessage("/noclippy [on|off|toggle|dry|help]－未指定選項時開關設定視窗。")]
        private void OnNoClippy(string command, string argument)
        {
            switch (argument)
            {
                case "on":
                case "toggle" when !Config.EnableAnimLockComp:
                case "t" when !Config.EnableAnimLockComp:
                    Config.EnableAnimLockComp = true;
                    Config.Save();
                    PrintEcho("已啟用技能後搖補償！");
                    break;
                case "off":
                case "toggle" when Config.EnableAnimLockComp:
                case "t" when Config.EnableAnimLockComp:
                    Config.EnableAnimLockComp = false;
                    Config.Save();
                    PrintEcho("已停用技能後搖補償！");
                    break;
                case "dry":
                case "d":
                    PrintEcho($"模擬執行已{((Config.EnableDryRun = !Config.EnableDryRun) ? "啟用" : "停用")}。");
                    Config.Save();
                    break;
                case "":
                    ConfigUI.ToggleVisible();
                    break;
                default:
                    PrintEcho("用法錯誤：指令格式為「/noclippy <選項>」。" +
                        "\non / off / toggle－啟用或停用技能後搖補償。" +
                        "\ndry－切換模擬執行（不覆寫技能後搖）。");
                    break;
            }
        }

        public static void PrintEcho(string message) => DalamudApi.ChatGui.Print($"[NoClippy] {message}");
        public static void PrintError(string message) => DalamudApi.ChatGui.PrintError($"[NoClippy] {message}");

        public static void PrintLog(string message)
        {
            if (Config.LogToChat)
            {
                if (Config.LogChatType != XivChatType.None)
                {
                    DalamudApi.ChatGui.Print(new XivChatEntry
                    {
                        Message = $"[NoClippy] {message}",
                        Type = Config.LogChatType
                    });
                }
                else
                {
                    PrintEcho(message);
                }
            }
            else
            {
                DalamudApi.LogInfo(message);
            }
        }

        public static int F2MS(float f) => (int)Math.Round(f * 1000);

        private static void Update(IFramework framework) => Game.Update();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            Config.Save();

            DalamudApi.Framework.Update -= Update;
            DalamudApi.PluginInterface.UiBuilder.Draw -= PluginUI.Draw;
            DalamudApi.PluginInterface.UiBuilder.OpenConfigUi -= ConfigUI.ToggleVisible;

            Modules.Modules.Dispose();
            Game.Dispose();

            DalamudApi.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
