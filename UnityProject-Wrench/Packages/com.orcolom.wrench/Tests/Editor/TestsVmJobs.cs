using NUnit.Framework;
using Tests.Helpers;
using Unity.Collections;
using Unity.Jobs;
using Wrench;

namespace Tests
{
	public class TestsVmJobs
	{
		private struct InvokeJob : IJobParallelFor
		{
			public Vm Vm;
			public Handle Handle;
			public NativeQueue<int>.ParallelWriter State;

			public void Execute(int index)
			{
				Vm.AtomicAccess(ref this, (ref InvokeJob job, in Vm vm) =>
				{
					vm.EnsureSlots(1);
					vm.Slot0.GetVariable("m", "fn");
					vm.Call(job.Handle);
					var value = vm.Slot0.GetInt();
					job.State.Enqueue(value);
				});
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
			var writer = list.AsParallelWriter();

			var job = new InvokeJob
			{
				Vm = vm,
				Handle = handle,
				State = writer,
			};
			var jobHandle = job.Schedule(64, 32);
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
