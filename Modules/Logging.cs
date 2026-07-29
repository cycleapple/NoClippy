using System;
using Dalamud.Game.Text;
using Dalamud.Bindings.ImGui;

namespace NoClippy
{
    public partial class Configuration
    {
        public bool LogToChat = false;
        public XivChatType LogChatType = XivChatType.None;
    }
}

// This is a module because why not
namespace NoClippy.Modules
{
    public class Logging : Module
    {
        public override int DrawOrder => 8;

        public override void DrawConfig()
        {
            if (ImGui.Checkbox("輸出至聊天記錄", ref NoClippy.Config.LogToChat))
                NoClippy.Config.Save();
            PluginUI.SetItemTooltip("改為將記錄輸出至聊天記錄。");

            if (!NoClippy.Config.LogToChat) return;

            if (ImGui.BeginCombo("記錄聊天頻道", NoClippy.Config.LogChatType.ToString()))
            {
                foreach (var chatType in Enum.GetValues<XivChatType>())
                {
                    if (!ImGui.Selectable(chatType.ToString())) continue;

                    NoClippy.Config.LogChatType = chatType;
                    NoClippy.Config.Save();
                }

                ImGui.EndCombo();
            }

            PluginUI.SetItemTooltip("覆寫預設的 Dalamud 聊天頻道。");
        }
    }
}
