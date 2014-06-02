using Mono.CSharp;
using ScriptCs.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.Engine.Mono
{
    public class MonoHostBuilder
    {
        public static MonoHostBuilderResult Build(ScriptHost scriptHost)
        {
            var asmName = new AssemblyName("ScriptCs.Engine.Mono.DynamicMonoHost");
            var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
            var module = asm.DefineDynamicModule(asmName.Name);
            var hostType = module.DefineType("DynamicMonoHost", TypeAttributes.Public, typeof(InteractiveBase));
            var scriptHostType = scriptHost.GetType();

            // Add our required properties
            var scriptHostField = hostType.DefineField("_scriptHost", scriptHostType, FieldAttributes.Static | FieldAttributes.Public);

            var scriptMethods = scriptHostType.GetMethods();
            foreach (var scriptMethod in scriptMethods.Where(x => !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_")))))
            {
                var method = hostType.DefineMethod(scriptMethod.Name, MethodAttributes.Public | MethodAttributes.Static, scriptMethod.ReturnType, null);
                var callMethod = scriptHostType.GetMethod(scriptMethod.Name, scriptMethod.GetParameters().Select(x => x.ParameterType).ToArray());
                if (callMethod.IsGenericMethodDefinition)
                {
                    var argNames = new List<string>();
                    var args = callMethod.GetGenericArguments();
                    var argCount = args.Count();
                    for (int i = 0; i < argCount; i++)
                    {
			            argNames.Add("T" + i);
                    }

                    var genParams = method.DefineGenericParameters(argNames.ToArray());
                    method.SetParameters(genParams);
                }

                var methodIL = method.GetILGenerator();
                if (scriptMethod.ReturnType == typeof(void))
                {
                    methodIL.Emit(OpCodes.Ret);
                    continue;
                }

                methodIL.Emit(OpCodes.Ldsfld, scriptHostField);
                methodIL.Emit(OpCodes.Callvirt, callMethod);
                methodIL.Emit(OpCodes.Ret);
            }

            var scriptProperties = scriptHostType.GetProperties();
            foreach (var scriptProperty in scriptProperties)
            {
                var property = hostType.DefineProperty(scriptProperty.Name, scriptProperty.Attributes, scriptProperty.PropertyType, null);
                if (scriptProperty.CanRead && scriptProperty.GetGetMethod(true).IsPublic)
                {
                    var getter = hostType.DefineMethod(string.Format("get_{0}", scriptProperty.Name), MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Static, scriptProperty.PropertyType, null);
                    var getterIL = getter.GetILGenerator();
                    getterIL.Emit(OpCodes.Ldsfld, scriptHostField);
                    getterIL.Emit(OpCodes.Callvirt, scriptHostType.GetMethod(string.Format("get_{0}", scriptProperty.Name), Type.EmptyTypes));
                    getterIL.Emit(OpCodes.Ret);
                    property.SetGetMethod(getter);
                }

                if (scriptProperty.CanWrite && scriptProperty.GetSetMethod(true).IsPublic)
                {
                    var setter = hostType.DefineMethod(string.Format("set_{0}", scriptProperty.Name), MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Static, null, new Type[] { scriptProperty.PropertyType });
                    var setterIL = setter.GetILGenerator();
                    setterIL.Emit(OpCodes.Ldsfld, scriptHostField);
                    setterIL.Emit(OpCodes.Ldarg_0);
                    setterIL.Emit(OpCodes.Call, scriptHostType.GetMethod(string.Format("set_{0}", scriptProperty.Name), new[] { scriptProperty.PropertyType }));
                    setterIL.Emit(OpCodes.Ret);
                    property.SetSetMethod(setter);
                }
            }

            var builtType = hostType.CreateType();
            var host = Activator.CreateInstance(builtType);
            host.GetType().GetField("_scriptHost").SetValue(host, scriptHost);

            return new MonoHostBuilderResult
            {
                Assembly = asm,
                Type = builtType
            };
        }
    }

    public class MonoHostBuilderResult
    {
        public Assembly Assembly { get; set; }
        public Type Type { get; set; }
    }
}
