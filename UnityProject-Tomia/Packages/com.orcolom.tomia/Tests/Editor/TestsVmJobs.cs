using NUnit.Framework;
using Tests.Helpers;
using Unity.Collections;
using Unity.Jobs;
using Tomia;

namespace Tests
{
	public class TestsVmJobs
	{
		private struct InvokeJob : IJobFor
		{
			public Vm Vm;
			public Handle Handle;
			public NativeQueue<int> State;

			public void Execute(int index)
			{
				Vm.EnsureSlots(1);
				Vm.Slot0.GetVariable("m", "fn");
				Vm.Call(Handle);
				var value = Vm.Slot0.GetInt();
				State.Enqueue(value);
			}
		}

		[Test]
		public void Interpret_InvokeJob()
		{
			using var vm = Vm.New();
			using var handle = vm.MakeCallHandle("call()");

			using var test = new InterpretTest();
			test.ExpectResult(InterpretResult.Success);
			test.Interpret(vm, "m", @"
var value = -1
var fn = Fn.new { 
	value = value + 1
	return value
}");

			NativeQueue<int> list = new NativeQueue<int>(AllocatorManager.Persistent);

			var job = new InvokeJob
			{
				Vm = vm,
				Handle = handle,
				State = list,
			};
			var jobHandle = job.Schedule(64, new JobHandle());
			jobHandle.Complete();
			Assert.AreEqual(64, list.Count);
			list.Dispose();
		}
	}
}
