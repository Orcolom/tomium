using System;
using System.Collections.Generic;
using UnityEngine;
using Wrench;
using Wrench.Builder;

namespace Binding
{
	public class WrenComponentData
	{
		public WrenGameObject GameObject;
		public Handle Handle;
		public string TypeId;
		
		public bool HasAwake;
		public bool HasOnDestroy;

		public bool HasOnEnable;
		public bool HasOnDisable;
		
		public bool HasStart;
		public bool HasUpdate;
		public bool HasLateUpdate;
		public bool HasFixedUpdate;
	}
	
	public class WrenGameObject : MonoBehaviour
	{
		public List<WrenComponentData> Components;
		private void Awake()
		{
			throw new NotImplementedException();
		}

		public void f_GetComponent(Vm vm, string typeId)
		{
			var component = Components.Find(data => data.TypeId == typeId);
			if (component == null) vm.Slot0.SetNull();
			else
			{
				
			}
		}
	}
}
