using Binding;
using UnityEngine;
using Tomium.Builder;
using MethodBody = Tomium.Builder.MethodBody;
// ReSharper disable InconsistentNaming

namespace Tomium.Samples.UnityBinding
{
    public class ComponentClass : UnityModule.Class
    {
        public const string WrenName = "Component";
        public ComponentClass() : base(WrenName){}
    }

    public class UnityComponentClass : UnityModule.Class
    {
        public const string WrenName = "UnityComponent";

        public UnityComponentClass() : base(WrenName, ComponentClass.WrenName)
        {
            Add(new Method(Signature.Create(MethodType.FieldGetter, nameof(gameObject)), new ForeignMethod(gameObject)));
        }

        public static void gameObject(Vm vm)
        {
            vm.EnsureSlots(1);
            if (UnityModule.ExpectObject(vm.Slot0, out ForeignObject<Component> self) == false) return;
			
            if (UnityModule.TryGetId(vm, typeof(GameObject), out var typeId) == false) return;
            UnityModule.SetNewForeignObject(vm, vm.Slot0, typeId, self.Value.gameObject);
        }
    }

    public class WrenComponentClass : UnityModule.Class
    {
        public const string WrenName = "WrenComponent";

        public WrenComponentClass() : base(WrenName, ComponentClass.WrenName)
        {
            Add(new Method(Signature.Create(MethodType.FieldGetter, nameof(UnityComponentClass.gameObject)), new MethodBody
            {
                Token.DangerousInsert("return _gameObject"),
            }));

            Add(new Method(Signature.Create(MethodType.Method, "SetGameObject_", 1), new MethodBody
            {
                Token.DangerousInsert("_gameObject = arg0"),
            }));
        }
    }
}