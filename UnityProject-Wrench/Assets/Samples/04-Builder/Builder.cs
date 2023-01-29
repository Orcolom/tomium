using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Wrench.Builder;

namespace Wrench.Samples
{
	public class Builder : MonoBehaviour
	{
		private readonly StringBuilder _writeBuffer = new StringBuilder();
		private void Start()
		{
			ModuleCollection collection = new ModuleCollection();
			collection.Add(new TimeDateBuilderModule());

			var vm = Vm.New();
			vm.SetWriteListener((_, text) =>
			{
				if (text == "\n")
				{
					Debug.Log(_writeBuffer);
					_writeBuffer.Clear();
				} else _writeBuffer.Append(text);
			});
			
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


			vm.SetLoadModuleListener((vm, path) =>
			{
				var str = collection.LoadModuleHandler(vm, path);
				Debug.LogWarning($"Load `{path}`\n{str}");
				return str;
			});
			
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
