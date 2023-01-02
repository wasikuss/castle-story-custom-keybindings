using System.Collections.Generic;
using System.IO;
using System.Reflection;

using BepInEx;
using Brix.Lua;
using MoonSharp.Interpreter;
using Rewired;
using Rewired.Data.Mapping;


namespace CastleStory_CustomKeybindingsPlugin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]

    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            var im = UnityEngine.GameObject.Find("Rewired Input Manager").GetComponent<InputManager>();

            Directory.CreateDirectory("Info/Lua/modconf");

            Script script;
            DynValue result = LuaLoader.Load(out script, "modconf/keybindings.lua", null, null);
            ParseLuaKeybindings(im, result.Table);
            Script.Kill(ref script);

            InvokeMethod(im, "Awake2", new object[0]);
        }

        private void ParseLuaKeybindings(InputManager im, Table table)
        {
            for (int i = 0; i != table.Length; i++)
            {
                Table bind = (Table)table[i + 1];
                string category = (string)bind[1];
                string action = (string)bind[2];
                string descriptiveName = (string)bind[3];

                Table primaryKeys = (Table)bind[4];
                List<KeyboardKeyCode> primaryKeyList = new List<KeyboardKeyCode>();
                for (int j = 0; j != primaryKeys.Length; j++)
                {
                    string entryString = (string)primaryKeys[j + 1];
                    primaryKeyList.Add((KeyboardKeyCode)System.Enum.Parse(typeof(KeyboardKeyCode), entryString));
                }
                AddInput(im, category, action, descriptiveName, primaryKeyList);
            }
        }

        private void AddInput(InputManager im, string category, string name, string descriptiveName, List<KeyboardKeyCode> keyCodes)
        {
            var categoryId = im.userData.IndexOfActionCategory(category);
            var actionIdx = im.userData.DuplicateAction_FromButton(categoryId, 0);
            var action = im.userData.GetAction(actionIdx);

            InvokeMethod(action, "ReplaceName", new object[] { name });
            InvokeMethod(action, "ReplaceDescriptiveName", new object[] { descriptiveName });

            var layoutId = im.userData.GetKeyboardLayoutId("QWERTY");
            var keyboardMap = im.userData.GetKeyboardMap(categoryId, layoutId);

            BindKeyCode(keyboardMap, action.id, keyCodes);
            BindKeyCode(keyboardMap, action.id, keyCodes);
        }

        private void BindKeyCode(ControllerMap_Editor keyboardMap, int actionId, List<KeyboardKeyCode> keyCodes)
        {
            keyboardMap.InsertActionElementMap(0);
            var actionElementMap = keyboardMap.GetActionElementMap(0);

            InvokeMethod(actionElementMap, "ReplaceActionId", new object[] { actionId });
            if (keyCodes.Count == 1)
            {
                InvokeMethod(actionElementMap, "ReplaceKeyboardKeyCode", new object[] { keyCodes[0] });
            }
            else
            {
                InvokeMethod(actionElementMap, "ReplaceKeyboardKeyCode", new object[] { keyCodes[1] });
                InvokeMethod(actionElementMap, "ReplaceModifierKey1", new object[] { ToModifier(keyCodes[0]) });
            }
        }

        private void InvokeMethod<T>(T obj, string method, object[] _params)
        {
            typeof(T).GetMethod(method, BindingFlags.Instance | BindingFlags.Public).Invoke(obj, _params);
        }

        private ModifierKey ToModifier(KeyboardKeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyboardKeyCode.LeftAlt:
                case KeyboardKeyCode.RightAlt:
                    return ModifierKey.Alt;
                case KeyboardKeyCode.LeftControl:
                case KeyboardKeyCode.RightControl:
                    return ModifierKey.Control;
                case KeyboardKeyCode.LeftShift:
                case KeyboardKeyCode.RightShift:
                    return ModifierKey.Shift;
                case KeyboardKeyCode.LeftCommand:
                case KeyboardKeyCode.RightCommand:
                    return ModifierKey.Command;
                default:
                    return ModifierKey.None;
            }
        }
    }
}
