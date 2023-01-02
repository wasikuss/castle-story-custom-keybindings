using System.Collections.Generic;
using Mono.Cecil;
using System.Linq;
using Mono.Cecil.Cil;

namespace CastleStory_CustomKeybindingsPatcher
{
    public class Patcher
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Rewired_Core.dll" };

        public static void Patch(AssemblyDefinition assembly)
        {
            var InputManager_BaseType = assembly.MainModule.GetType("Rewired.InputManager_Base");
            var voidType = assembly.MainModule.TypeSystem.Void;
            var stringType = assembly.MainModule.TypeSystem.String;
            var intType = assembly.MainModule.TypeSystem.Int32;

            var ilProcessor = GetMethod(InputManager_BaseType, "Awake").Body.GetILProcessor();
            ilProcessor.Body.Instructions.Clear();
            ilProcessor.Emit(OpCodes.Ret);

            var methodDefinition = new MethodDefinition("Awake2", MethodAttributes.Public, voidType);
            ilProcessor = methodDefinition.Body.GetILProcessor();
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Call, GetMethod(InputManager_BaseType, "Initialize"));
            ilProcessor.Emit(OpCodes.Ret);
            InputManager_BaseType.Methods.Add(methodDefinition);

            GenerateReplaceValueMethod(assembly, "Rewired.InputAction", "ReplaceName", "set_name", stringType);
            GenerateReplaceValueMethod(assembly, "Rewired.InputAction", "ReplaceDescriptiveName", "set_descriptiveName", stringType);
            
            GenerateReplaceValueMethod(assembly, "Rewired.ActionElementMap", "ReplaceActionId", "set_actionId", intType);
            var keyCodeType = assembly.MainModule.GetType("Rewired.KeyboardKeyCode");
            GenerateReplaceValueMethod(assembly, "Rewired.ActionElementMap", "ReplaceKeyboardKeyCode", "set_keyboardKeyCode", keyCodeType);
            var modifierKeyType = assembly.MainModule.GetType("Rewired.KeyboardKeyCode");
            GenerateReplaceValueMethod(assembly, "Rewired.ActionElementMap", "ReplaceModifierKey1", "set_modifierKey1", modifierKeyType);
        }

        private static void GenerateReplaceValueMethod(AssemblyDefinition assembly, string typeName, string name, string setter, TypeReference paramType)
        {
            var typeDefinition = assembly.MainModule.GetType(typeName);
            var methodDefinition = new MethodDefinition(name, MethodAttributes.Public, assembly.MainModule.TypeSystem.Void);
            var ilProcessor = methodDefinition.Body.GetILProcessor();
            ilProcessor.Emit(OpCodes.Ldarg_0);
            ilProcessor.Emit(OpCodes.Ldarg_1);
            ilProcessor.Emit(OpCodes.Call, GetMethod(typeDefinition, setter));
            ilProcessor.Emit(OpCodes.Ret);
            typeDefinition.Methods.Add(methodDefinition);

            var parameterDefinition = new ParameterDefinition(paramType);
            methodDefinition.Parameters.Add(parameterDefinition);
        }

        private static MethodDefinition GetMethod(TypeDefinition typeDefinition, string name)
        {
            return typeDefinition.Methods.Where(m => m.Name.Equals(name)).First().Resolve();
        }
    }
}