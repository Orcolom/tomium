using NUnit.Framework;
using Tomia.Tests.Helpers;

namespace Tomia.Tests
{
	public class TestsVm
	{
		#region Wren

		[Test]
		public void Wren_IsSupported()
		{
			Assert.IsTrue(Tomia.IsSupported);
		}

		#endregion

		#region Vm

		[Test]
		public void Vm_New_Dispose()
		{
			using var vm = Vm.New();
			Assert.IsTrue(vm.IsValid());
			vm.Dispose();
			Assert.IsFalse(vm.IsValid());
		}

		#endregion

		#region Interpret

		[Test]
		public void Interpret_CompileError()
		{
			using var vm = Vm.New();

			using var test = new InterpretTest();
			test.ExpectResult(InterpretResult.CompileError);
			test.Interpret(vm, "m", "Sys.print(\"e\")");
		}

		[Test]
		public void Interpret_RuntimeError()
		{
			using var vm = Vm.New();

			using var test = new InterpretTest();
			test.ExpectResult(InterpretResult.RuntimeError);
			test.Interpret(vm, "m", "System.pri(10)");
		}

		[Test]
		public void Interpret_Success()
		{
			using var vm = Vm.New();

			using var test = new InterpretTest();
			test.ExpectResult(InterpretResult.Success);
			test.ExpectMessage("hello world", "\n");
			test.Interpret(vm, "m", "System.print(\"hello world\")");
		}

		[Test]
		public void Interpret_SameModuleName()
		{
			using var vm = Vm.New();

			using (var test = new InterpretTest())
			{
				test.ExpectResult(InterpretResult.Success);
				test.Interpret(vm, "m", "System.write(\"hello world\")");
			}

			using (var test = new InterpretTest())
			{
				test.ExpectResult(InterpretResult.Success);
				test.Interpret(vm, "m", "System.write(\"hello world\")");
			}
		}

		#endregion

		#region Cals

		[Test]
		public void Call()
		{
			using var vm = Vm.New();

			using (var test = new InterpretTest())
			{
				test.ExpectResult(InterpretResult.Success);
				test.Interpret(vm, "m", @"var fn = Fn.new { |arg| System.print(arg) }");
			}

			using (var test = new InterpretTest())
			{
				test.ExpectResult(InterpretResult.Success);

				using var handle = vm.MakeCallHandle("call(_)");
				vm.EnsureSlots(2);
				vm.Slot0.GetVariable("m", "fn");
				vm.Slot1.SetString("test");
				test.Call(vm, handle);
			}
		}

		[Test]
		public void CallFail()
		{
			using var vm = Vm.New();

			vm.Interpret("m", @"
var fn6 = Fn.new { System.p() }
var fn5 = Fn.new { fn6.call() }
var fn4 = Fn.new { fn5.call() }
var fn3 = Fn.new { fn4.call() }
var fn2 = Fn.new { fn3.call() }
var fn = Fn.new { fn2.call() }
");

			using var test = new InterpretTest();
			test.ExpectResult(InterpretResult.RuntimeError);

			using var handle = vm.MakeCallHandle("call(_,_,_)");
			vm.EnsureSlots(15);
			vm.Slot0.GetVariable("m", "fn");
			vm.Slot1.SetString("hello world");
			vm.Slot2.SetDouble(12.4);
			vm.Slot3.SetNull();
			vm.Slot4.SetNewList();
			vm.Slot4.AddToList(vm.Slot1);
			vm.Slot4.AddToList(vm.Slot2);
			vm.Slot5.SetNewMap();
			vm.Slot5.SetMapValue(vm.Slot3, vm.Slot4);
			test.Call(vm, handle);
		}

						
		[Test]
		public void CallHandle_Sharing()
		{
			using var from = Vm.New();

			using var to = Vm.New();
			using var handle = from.MakeCallHandle("call()");

			to.Interpret("<x>", "var fn = Fn.new {}");
			to.EnsureSlots(1);
			to.Slot0.GetVariable("<x>", "fn");
			var result = to.Call(handle);
			Assert.AreEqual(result, InterpretResult.Success);
		}
		
		#endregion

		#region Module

		[Test]
		public void Module_Resolve_Load_Success()
		{
			using Vm vm = Vm.New();

			// markers
			var resolveMarker = new Marker("resolveMarker");
			var loadMarker = new Marker("loadMarker");

			// setup listeners
			vm.SetResolveModuleListener((_, _, name) =>
			{
				resolveMarker.Trigger();
				if (name == "Foo") return "Foo/Bar";
				return null;
			});

			vm.SetLoadModuleListener((_, name) =>
			{
				loadMarker.Trigger();
				return name switch
				{
					"Foo/Bar" => "var Val1 = 10",
					"Bar" => "var Val2 = 20",
					_ => null,
				};
			});

			using (var test = new InterpretTest())
			{
				test.ExpectResult(InterpretResult.Success);
				test.ExpectMessage("20");
				test.ExpectMarkers(resolveMarker, loadMarker);
				test.Interpret(vm, "x", @"
import ""Foo"" for Val1
import ""Bar"" for Val2

System.write(Val2)
");
			}

			using (var test = new InterpretTest())
			{
				test.ExpectResult(InterpretResult.Success);
				test.ExpectMessage("10");
				test.ExpectMarkers(resolveMarker);
				test.Interpret(vm, "x", @"
import ""Foo/Bar"" for Val1

System.write(Val1)
");
			}
		}

		[Test]
		public void Module_Load_Error()
		{
			using Vm vm = Vm.New();

			using var test = new InterpretTest();
			test.ExpectResult(InterpretResult.RuntimeError);
			test.Interpret(vm, "m", @"
import ""Fo/Bar"" for Val1

System.write(Val1)
");
		}

		#endregion

		#region Foreign Methods

		[Test]
		public void ForeignMethod_Bind_Call()
		{
			using Vm vm = Vm.New();
			var loadModuleMarker = new Marker("loadModuleMarker");
			var foreinMethodMarker = new Marker("foreign method");
			var bindMethodMarker = new Marker("bind method");

			ForeignMethod method = new ForeignMethod(_ => foreinMethodMarker.Trigger());
			vm.SetBindForeignMethodListener((_, _, _, _, _) =>
			{
				bindMethodMarker.Trigger();
				return method;
			});

			vm.SetLoadModuleListener((_, _) =>
			{
				loadModuleMarker.Trigger();

				return @"
class Bar {
	foreign static Test()
}
";
			});

			using var test = new InterpretTest();
			test.ExpectResult(InterpretResult.Success);
			test.ExpectMarkers(foreinMethodMarker, bindMethodMarker, loadModuleMarker);
			test.Interpret(vm, "m", @"
import ""Foo"" for Bar

Bar.Test()
");
		}

		#endregion
	}
}
