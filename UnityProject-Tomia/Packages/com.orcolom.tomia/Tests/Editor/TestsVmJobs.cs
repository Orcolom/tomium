using NUnit.Framework;
using Tomia.Tests.Helpers;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tomia.Tests
{
	public class TestsVmJobs
	{
		private struct InvokeJobFor : IJobFor
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
		
		private struct InvokeJobParallelFor : IJobParallelFor
		{
			public Vm Vm;
			public Handle Handle;
			public NativeQueue<int>.ParallelWriter State;

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
		public void Interpret_InvokeJobFor()
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

			var job = new InvokeJobFor
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
		
		[Test]
		public void Interpret_InvokeJobParallelFor()
		{
			// NOTE: have not found a good way to make this task mark itself as success. but AccessViolationException is actually expected behaviour
			
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

			var job = new InvokeJobParallelFor()
			{
				Vm = vm,
				Handle = handle,
				State = list.AsParallelWriter(),
			};
		
			LogAssert.Expect(LogType.Exception, "AccessViolationException: Tried to use the same Vm on multiple threads at the same time.");
			{
				var jobHandle = job.Schedule(64, 2, new JobHandle());
				jobHandle.Complete();
			}
			
			list.Dispose();
		}
	}
}
