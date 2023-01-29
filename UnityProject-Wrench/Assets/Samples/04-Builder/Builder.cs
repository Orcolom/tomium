using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wrench.Builder;

namespace Wrench.Samples
{
	public class Builder : MonoBehaviour
	{
		private void Start()
		{
			ModuleCollection collection = new ModuleCollection();
			collection.Add(new TimeDateBuilderModule());

			var vm = Vm.New();
			vm.SetWriteListener((_, text) => Debug.Log(text));
			vm.SetErrorListener((_, type, module, line, message) =>
			{
				string str = type switch
				{
					ErrorType.CompileError => $"[{module} line {line}] {message}",
					ErrorType.RuntimeError => message,
					ErrorType.StackTrace => $"[{module} line {line}] in {message}",
					_ => string.Empty,
				};
				Debug.LogError(str);
			});


			vm.SetLoadModuleListener(collection.LoadModuleHandler);
			vm.SetBindForeignClassListener(collection.BindForeignClassHandler);
			vm.SetBindForeignMethodListener(collection.BindForeignMethodHandler);

			vm.Interpret("<main>", @"
import ""Time"" for DateTime 

var time = DateTime.Now()
System.print(time)

System.print(DateTime.Today())
");


			vm.Dispose();

			SampleRunner.NextSample();
		}
	}

	public class TimeDateBuilderModule : Module
	{
		public readonly string Source;

		public TimeDateBuilderModule() : base("Time")
		{
			Add(new Class("DateTime", null, ForeignClass.DefaultAlloc())
			{
				new Method(Signature.Create(MethodType.Construct, "Now"), new ForeignMethod(NowInstanced)),
				new Method(Signature.Create(MethodType.Construct, "Today"), new ForeignMethod(TodayStatic)),
				new Method(Signature.Create(MethodType.ToString), new ForeignMethod(vm =>
				{
					var fo = vm.Slot0.GetForeign<DateTime>();
					vm.Slot0.SetString(fo.Value.ToString());
				})),
			});
		}

		private void NowInstanced(Vm vm)
		{
			var fo = vm.Slot0.GetForeign<DateTime>();
			fo.Value = DateTime.Now;
		}

		private static void TodayStatic(Vm vm)
		{
			var fo = vm.Slot0.GetForeign<DateTime>();
			fo.Value = DateTime.Today;
		}

		private void Alloc(Vm vm)
		{
			vm.Slot0.SetNewForeign(vm.Slot0, new DateTime());
		}
	}
}
