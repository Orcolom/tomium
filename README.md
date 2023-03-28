# Tomium

A [Wren](https://wren.io/) binding made from the ground up for Unity.

## Features

- [x] Full Wren support.
- [x] Syntax tree-based module builder.
- [x] C# syntax style,
- [x] [All scripting backends including JobSystem](#scripting-backends-and-platforms)
- [x] [Optimized garbage collection and allocation](#memory-allocation-and-garbage-collection)
- [x] Optimized profiler markers.
- [x] Safe handling of native allocations.
- [x] Pre-emptive exception where Wren would cause a native crash.
- [x] blueprints for full Unity bindings

### Example VM

```cs
  private void Start()
  {
    // Create a new vm
    var vm = Vm.New();

    // Add listeners for logs and errors
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
    
    // Interpret some code
    var result = vm.Interpret("<main>", "var CallMe = Fn.new{|arg|\nSystem.print(\"Hello World %(arg)\")\n}"
    );
    
    // Ensure the amount of slots needed
    // get the `CallMe` variable and store it in slot 0
    // set a string value to slot 1
    // make a call handle and run it on `CallMe`
    vm.EnsureSlots(2);
    vm.Slot0.GetVariable("<main>", "CallMe");
    vm.Slot1.SetString("\n-From Tamia");
    using (var handle = vm.MakeCallHandle("call(_)"))
    {
      vm.Call(handle);
    }

    vm.Dispose();
  }
}
```

The project has multiple VM's in the samples `UnityProject-Tomium/Assets/Samples/Tomium/Latest/GettingStarted`
Or by installing the samples using the package manager. Each implementation is different and builds on the previous ones.

### Memory Allocation and Garbage Collection

Because of the nature of this project and the need for call-to-native code and object tracking, 0 allocations became impossible. We pre-allocate as much as possible and use classes only for objects that should exist for the whole application lifetime.

Every time we have a possible allocation that is unavoidable we've wrapped it in its own re-used Profiler Marker  

```cs
using (ProfilerUtils.AllocScope.Auto())
{
  // code that has possible allocation
}
```

### Defines

We have a few defined symbols to change the level of debugging

- `TOMIUM_DEBUG`: Logs creation and destruction of native objects.
- `TOMIUM_LOG_ABORTEXCEPTION`: Log exceptions with call stack when we have to abort from C#.

## Modules

### con.orcolom.tomium

This is the core that talks to native and handles the heavy lifting.

### con.orcolom.tomium.builder

This package helps with module management and creation.

## Install

Unity can accept git URLs that follow this structure.

```xml
{
  "<package_name>": "<git_url>?path=<path_to_folder>#<tag>",

  
  "com.orcolom.tomium": "https://github.com/Orcolom/tomium.git?path=UnityProject-Tomium/Packages/com.orcolom.tomium#1.0.0", 
  "com.orcolom.tomium.builder": "https://github.com/Orcolom/tomium.git?path=UnityProject-Tomium/Packages/com.orcolom.tomium.builder#1.0.0", 
}
```

You can also download the source and add the packages locally.

## Support

### Scripting Backends and Platforms

|Platform    | |mono |il2cpp | |jobs*|burst |
|------------|-|-----|-------|-|-----|------|
|Windows x64 | |✔️   |✔️    | |✔️   |🚧   |
|Linux       | |❔   |✔️    | |✔️   |🚧   |
|Mac         | |❔   |❔    | |❔   |🚧   |
|Android     | |❌   |✔️    | |✔️   |🚧   |
|iOS         | |❌   |❔    | |❔   |🚧   |
|WebGL       | |❌   |✔️    | |✔️   |🚧   |

✔️ = Supported
❔ = Not verified. (you can only own so many devices)
🚧 = To be implemented
❌ = Not supported

*Tomium was structurally and conceptually designed to support jobs. But safety is harder to ensure and handle.

### Version Compatibility

The compatibilities between Wren, Tomium and Unity

|Tomium  |Unity      |Wren    |
|--------|-----------|--------|
|0.4.0   |2021.3+    |0.4.0   |
