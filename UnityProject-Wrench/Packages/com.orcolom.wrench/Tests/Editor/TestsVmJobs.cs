using NUnit.Framework;
using Tests.Helpers;
using Unity.Collections;
using Unity.Jobs;
using Wrench;

namespace Tests
{
	public class TestsVmJobs
	{
		private struct InvokeJob : IJobFor
		{
			public UnmanagedVm Vm;
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
		public void Interpret_CompileError()
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
				Vm = new UnmanagedVm(vm),
				Handle = handle,
				State = list,
			};
			var jobHandle = job.Schedule(64, new JobHandle());
			// job = new InvokeJob {Vm = vm, State = writer};
			// handle = job.Schedule(handle);
			// job = new InvokeJob {Vm = vm, State = writer};
			// handle = job.Schedule(handle);
			// job = new InvokeJob {Vm = vm, State = writer};
			// handle = job.Schedule(handle);
			//
			// job = new InvokeJob {Vm = vm, State = writer};
			// var handle2 = job.Schedule();
			// job = new InvokeJob {Vm = vm, State = writer};
			// handle2 = job.Schedule(handle2);
			// job = new InvokeJob {Vm = vm, State = writer};
			// handle2 = job.Schedule(handle2);
			// job = new InvokeJob {Vm = vm, State = writer};
			// handle2 = job.Schedule(handle2);
				
			// JobHandle.CompleteAll(ref handle);//, ref handle2);
			jobHandle.Complete();
			Assert.AreEqual(64, list.Count);
			list.Dispose();
		}
	}
}
