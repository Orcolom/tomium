using System;
using System.Collections.Generic;
using UnityEngine;
using Wrench;
using Wrench.Builder;

namespace Binding
{
	public class WrenComponentType
	{
		public string Id;
		public bool? HasStart;
		public bool? HasUpdate;
	}
	
	public class WrenComponentData
	{
		public WrenGameObject GameObject;
		public Handle Handle;
		public WrenComponentType Type;
		public bool HasDoneInit;
		
		// public bool HasOnDestroy;

		// public bool HasOnEnable;
		// public bool HasOnDisable;
		
		// public bool? HasStart;
		// public bool? HasUpdate;
		// public bool HasLateUpdate;
		// public bool HasFixedUpdate;
	}
	
	public class WrenGameObject : MonoBehaviour
	{
		private static readonly Dictionary<string, WrenComponentType> ComponentTypes = new Dictionary<string, WrenComponentType>(32);
		private List<WrenComponentData> _components = new List<WrenComponentData>();
		
		private Vm _vm;
		private Handle _startHandle;
		private Handle _updateHandle;

		public void Init(Vm vm)
		{
			_vm = vm;
			_startHandle = vm.MakeCallHandle("Start()");
			_updateHandle = vm.MakeCallHandle("Update()");
		}

		public void f_GetComponent(Vm vm, string typeId)
		{
			var index = _components.FindIndex(data => data.Type.Id == typeId);
			if (index == -1)
			{
				vm.Slot0.SetNull();
				return;
			}

			var component = _components[index];
			vm.Slot0.SetHandle(component.Handle);
		}

		public void RegisterAddComponent(string typeId, Handle instance)
		{
			if (ComponentTypes.TryGetValue(typeId, out var type) == false)
			{
				type = new WrenComponentType {Id = typeId};
				ComponentTypes.Add(typeId, type);
			}
			
			_components.Add(new WrenComponentData
			{
				Handle = instance,
				GameObject = this,
				Type = type,
			});
		}

		private void Update()
		{
			for (int i = 0; i < _components.Count; i++)
			{
				var component = _components[i];
				var type = component.Type;
				
				if ((type.HasStart.HasValue == false || type.HasStart.Value) && component.HasDoneInit == false)
				{
					_vm.EnsureSlots(1);
					_vm.Slot0.SetHandle(component.Handle);
					var result = _vm.Call(_startHandle);
					if (type.HasStart.HasValue == false) type.HasStart = result == InterpretResult.Success;
					component.HasDoneInit = true;
				}

				if (type.HasUpdate.HasValue == false || type.HasUpdate.Value)
				{
					_vm.EnsureSlots(1);
					_vm.Slot0.SetHandle(component.Handle);
					var result = _vm.Call(_updateHandle);
					if (type.HasUpdate.HasValue == false) type.HasUpdate = result == InterpretResult.Success;
				}
			}
		}
	}
}
