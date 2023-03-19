using System;
using System.Collections.Generic;
using Binding;
using Tomium.Builder;
using Tomium.Builder.Tokens;
using Module = Tomium.Builder.Module;
using Object = UnityEngine.Object;

namespace Tomium.Samples.UnityBinding
{
	public class UnityModule : Module
	{
		public class Class : global::Tomium.Builder.Class
		{
			public readonly Type ValueType;

			protected Class(string name, string inherits = null, Type type = null, ForeignClass @class = default, ClassBody body = null,
				Attributes attributes = null)
				: base(name, inherits, @class, body, attributes)
			{
				ValueType = type;
			}
		}
		
		public class Class<T> : Class
		{
			
			protected Class(string name, string inherits = null, ClassBody body = null,
				Attributes attributes = null)
				: base(name, inherits, typeof(T), ForeignClass.DefaultObjectAlloc<T>(), body, attributes)
			{
				if (ValueType == null) return;
				TypesById.Add(name, this);
				IdByType.Add(ValueType, this);
			}
		}
		
		public static readonly BasicToken f_AddComponentToken = Token.DangerousInsert(@$"
var isWren = {UtilityBinding.WrenName}.{UtilityBinding.MetaClassDerivesFrom__MetaClass_MetaClass}(arg0, {WrenComponentClass.WrenName})
if (isWren) {{
	var instance = arg0.New()
	instance.SetGameObject_(this)
	f_RegisterAddComponent_(""%(arg0)"", instance)
	instance.Awake()
	return instance
}} else {{
	return f_AddComponent(""%(arg0)"")
}}");

		public static bool ExpectObject<T>(Slot slot, out ForeignObject<T> value) where T : Object
		{
			if (ExpectValue.ExpectForeign(slot, out value, true) == false) return false;
			// TODO: unity null check?
			return true;
		}

		private static readonly Dictionary<string, UnityModule.Class> TypesById = new Dictionary<string, UnityModule.Class>();
		private static readonly Dictionary<Type, UnityModule.Class> IdByType = new Dictionary<Type, UnityModule.Class>();

		public UnityModule() : base("Unity")
		{
			Add(new Import(UtilityBinding.WrenName, new ImportVariable(UtilityBinding.WrenName)));
			Add(new GameObjectClass());
			Add(new UnityGameObjectClass());
			Add(new WrenGameObjectClass());
			Add(new ComponentClass());
			Add(new UnityComponentClass());
			Add(new WrenComponentClass());
			Add(new Vector3Class());
			Add(new QuaternionClass());
			Add(new TransformClass());
		}

		public static bool TryGetId(Vm vm, Type type, out UnityModule.Class id)
		{
			if (IdByType.TryGetValue(type, out id)) return true;

			vm.Slot0.SetString($"{type.Name} is not a component");
			vm.Abort(vm.Slot0);
			return false;
		}

		public static bool ExpectType(Vm vm, string id, out UnityModule.Class type)
		{
			if (TypesById.TryGetValue(id, out type)) return true;
			
			vm.Slot0.SetString($"{id} is not a component");
			vm.Abort(vm.Slot0);
			return false;
		}

		public static void SetNewForeignObject<T>(Vm vm, Slot slot, UnityModule.Class type, T data = default)
		{
			slot.GetVariable(type.Module.Path, type.Name.Text); // TODO: does this work for external types? 
			slot.SetNewForeignObject(slot, data);
		}
	}

}