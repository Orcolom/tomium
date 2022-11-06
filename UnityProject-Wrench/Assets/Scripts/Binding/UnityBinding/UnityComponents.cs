using UnityEngine;
using Wrench;
using Wrench.Builder;

namespace Binding.UnityBinding
{
	[WrenchClass(typeof(UnityModule), nameof(Transform), typeof(Transform), UnityComponentBinding.WrenName)]
	public class TransformBinding : Class
	{
		[WrenchMethod(MethodType.FieldGetter)]
		private void position(Vm vm, ForeignObject<Transform> self)
		{
			if (UnityModule.ExpectId(vm, typeof(Vector3), out var type) == false) return;
			var position = self.Value.position;
			UnityModule.SetNewForeign(vm, vm.Slot0, type, position);
		}
		
		[WrenchMethod(MethodType.FieldSetter)]
		private void position(Vm vm, ForeignObject<Transform> self, ForeignObject<Vector3> position)
		{
			self.Value.position = position.Value;
		}
	}
}
