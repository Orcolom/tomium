using System;
using System.Collections;
using System.Collections.Generic;
using AOT;
using UnityEngine;

namespace Tomia.Samples
{
	public class Foreign : MonoBehaviour
	{
		private void Start()
		{
			TimeDateModule timeDateModule = new TimeDateModule();
			
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

			vm.SetLoadModuleListener((_, module) =>
			{
				Debug.Log($"[load] module:{module}");
				if (module == "Time") return timeDateModule.Source;
				return null;
			});

			vm.SetBindForeignClassListener((_, module, className) =>
			{
				Debug.Log($"[bind class] module:{module} class:{className}");
				if (module != "Time") return default;
				if (className != "DateTime") return default;
				return timeDateModule.ForeignClass;
			});
			
			vm.SetBindForeignMethodListener((_, module, className, isStatic, signature) =>
			{
				Debug.Log($"[bind method] module:{module} class:{className} static:{isStatic} signature:{signature}");
				if (module != "Time") return default;
				if (className != "DateTime") return default;
				if (isStatic) return default;
				if (signature == "init Now()") return timeDateModule.Now;
				if (signature == "init Today()") return timeDateModule.Today;
				if (signature == "toString") return timeDateModule.ToString;
				return default;
			});

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

	public class TimeDateModule
	{
		public readonly ForeignClass ForeignClass;
		
		public readonly ForeignMethod Now;
		public readonly ForeignMethod Today;
		public new readonly ForeignMethod ToString;
		
		public readonly string Source;
		
		public TimeDateModule()
		{
			ForeignClass = new ForeignClass(Alloc);
			
			Now = new ForeignMethod(NowInstanced);
			Today = new ForeignMethod(TodayStatic);
			ToString = new ForeignMethod(vm =>
			{
				var fo = vm.Slot0.GetForeign<DateTime>();
				vm.Slot0.SetString(fo.Value.ToString());
			});

			Source = @"
foreign class DateTime {
	foreign construct Now()
	foreign construct Today()
	foreign toString 
}
";
		}

		// IL2cpp only allows static methods. this #if is purely as an example. best to only use static methods
#if ENABLE_IL2CPP
		private static void NowInstanced(Vm vm)
#else
		private void NowInstanced(Vm vm)
#endif		
		{
			var fo = vm.Slot0.GetForeign<DateTime>();
			fo.Value = DateTime.Now;
		}
		
		private static void TodayStatic(Vm vm)
		{
			var fo = vm.Slot0.GetForeign<DateTime>();
			fo.Value = DateTime.Today;
		}
#if ENABLE_IL2CPP
		[MonoPInvokeCallback(typeof(ForeignAction))]
#endif
		private static void Alloc(Vm vm)
		{
			vm.Slot0.SetNewForeign(vm.Slot0, new DateTime());
		}
	}
}
