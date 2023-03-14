using System;
using System.Text;
using Tomium.Builder;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Tomium.Samples
{
	public class Jobs : MonoBehaviour
	{
		private readonly StringBuilder _writeBuffer = new StringBuilder();
		private readonly StringBuilder _errorBuffer = new StringBuilder();

		private ModuleCollection _collection;

		private void Start()
		{
			_collection = new ModuleCollection();
			_collection.ModuleSourceGeneratedEvent += (path, _, str) => Debug.LogWarning($"Load `{path}`\n{str}");
			_collection.Add(new Vector3Module());

			// single
			var data = CreateVm();
			var number = InvokeJobParallelFor.Call(data.Item1, data.Item2, 10);
			Debug.Log(number);
			data.Item2.Dispose();
			data.Item1.Dispose();
			
			const int count = 512;
			
			NativeArray<(Vm, Handle)> vms = new NativeArray<(Vm, Handle)>(count, Allocator.Persistent);
			NativeArray<int> values = new NativeArray<int>(count, Allocator.Persistent);
			
			
			for (int i = 0; i < count; i++)
			{
				vms[i] = CreateVm();
			}

			var job = new InvokeJobParallelFor()
			{
				Vms = vms,
				Values = values,
			};
			
			Debug.Log("------ start job");
			var jobHandle = job.Schedule(count, count / 4, new JobHandle());
			jobHandle.Complete();
			
			for (int i = 0; i < count; i++)
			{
				vms[i].Item2.Dispose();
				vms[i].Item1.Dispose();
			}
			vms.Dispose();
			values.Dispose();
			
			SampleRunner.NextSample();
		}

		private (Vm, Handle) CreateVm()
		{
			var logger = new Logger();
			var vm = Vm.New();
			vm.SetWriteListener(logger.OnWrite);
			vm.SetErrorListener(logger.OnError);
			vm.SetUserData(logger);
			vm.SetLoadModuleListener(_collection.LoadModuleHandler);
			vm.SetBindForeignClassListener(_collection.BindForeignClassHandler);
			vm.SetBindForeignMethodListener(_collection.BindForeignMethodHandler);

			vm.Interpret("<main>", @"
import ""Vector3"" for Vector3

var fn = Fn.new {|value|
	// var vector = Vector3.new(value, value, value)
	// return vector.x * 100
	return 10
}
");

			vm.EnsureSlots(1);
			vm.Slot0.GetVariable("<main>", "fn");
			return (vm, vm.Slot0.GetHandle());
		}
	}

	public class Logger
	{
		// string builder isn't thread safe
		private readonly StringBuilder _writeBuffer = new StringBuilder();
		private readonly StringBuilder _errorBuffer = new StringBuilder();

		public void OnWrite(Vm _, string text)
		{
			if (text == "\n")
			{
				Debug.Log(_writeBuffer);
				_writeBuffer.Clear();
			}
			else
				_writeBuffer.Append(text);
		}

		public void OnError(Vm _, ErrorType type, string module, int line, string message)
		{
			string str = type switch
			{
				ErrorType.CompileError => $"[{module} line {line}] {message}",
				ErrorType.RuntimeError => message,
				ErrorType.StackTrace => $"[{module} line {line}] in {message}",
				_ => string.Empty,
			};

			if (type == ErrorType.CompileError)
				Debug.LogWarning(str);
			else if (type == ErrorType.StackTrace)
			{
				_errorBuffer.AppendLine(str);
				Debug.LogWarning(_errorBuffer);
				if (message == "(script)") _errorBuffer.Clear();
			}
			else
				_errorBuffer.AppendLine(str);
		}
	}

	struct InvokeJobParallelFor : IJobParallelFor
	{
		public NativeArray<(Vm, Handle)> Vms;
		public NativeArray<int> Values;

		public void Execute(int index)
		{
			var data = Vms[index];
			var result = Call(data.Item1, data.Item2, index);
			Values[index] = result;
		}

		public static int Call(Vm vm, Handle handle, int index)
		{
			using var call = vm.MakeCallHandle("call(_)");
			vm.EnsureSlots(2);
			vm.Slot0.SetHandle(handle);
			vm.Slot1.SetInt(index);
			vm.Call(call);
			
			vm.EnsureSlots(1);
			call.Dispose();
			return vm.Slot0.GetInt();
		}
	}

	public class Vector3Module : Module
	{
		public Vector3Module() : base("Vector3")
		{
			Add(new Class("Vector3", null, ForeignClass.DefaultStructAlloc<Vector3>())
			{
				new Method(Signature.Create(MethodType.Construct, "new", 3), new ForeignMethod((vm) =>
				{
					var fo = vm.Slot0.GetForeignStruct<Vector3>();
					var value = fo.Value;
					value.x = vm.Slot1.GetFloat();
					value.y = vm.Slot2.GetFloat();
					value.z = vm.Slot3.GetFloat();
					fo.Value = value;
				})),

				new Method(Signature.Create(MethodType.FieldGetter, "x"), new ForeignMethod((vm) => Get(vm, 0))),
				new Method(Signature.Create(MethodType.FieldSetter, "x", 1), new ForeignMethod((vm) => Set(vm, 0))),

				new Method(Signature.Create(MethodType.FieldGetter, "y"), new ForeignMethod((vm) => Get(vm, 1))),
				new Method(Signature.Create(MethodType.FieldSetter, "y", 1), new ForeignMethod((vm) => Set(vm, 1))),

				new Method(Signature.Create(MethodType.FieldGetter, "z"), new ForeignMethod((vm) => Get(vm, 2))),
				new Method(Signature.Create(MethodType.FieldSetter, "z", 1), new ForeignMethod((vm) => Set(vm, 2))),

				new Method(Signature.Create(MethodType.ToString), new ForeignMethod(vm =>
				{
					var fo = vm.Slot0.GetForeignStruct<Vector3>();
					vm.Slot0.SetString(fo.Value.ToString());
				})),
			});
		}

		private static void Get(Vm vm, int index)
		{
			var fo = vm.Slot0.GetForeignStruct<Vector3>();
			vm.Slot0.SetFloat(fo.Value[index]);
		}

		private static void Set(Vm vm, int index)
		{
			var fo = vm.Slot0.GetForeignStruct<Vector3>();
			var value = vm.Slot1.GetFloat();

			var vector = fo.Value;
			vector[index] = value;
			fo.Value = vector;
		}
	}
}
