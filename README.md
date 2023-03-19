# Tomia

A [Wren](https://wren.io/) binding made from the ground up for Unity.

> **Tomium**: A tooth-like structures that line the inside of a birds brill. That helps them handles their typical food sources better.

## Features

- [x] Full Wren support.
- [x] Syntax tree based module builder.
- [x] C# syntax style,
- [ ] Well documented where needed.
- [ ] Tests where needed
- [ ] Code coverage, because why not
- [ ] [All scripting backends including JobSystem](#scripting-backend)
- [x] [Close-to-0 garbage collection and allocation](#memory-allocation-and-garbage-collection)
- [x] Optimized profiler markers.
- [x] Safe handeling of native allocations.
- [x] Pre-emptive error handling where Wren would cause a crash.
- [WIP] blueprints for full Unity bindings

## Example VM

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

## Memory Allocation and Garbage Collection

Because of the nature of this project and the need for call to native code, 0 allocation became impossible. We pre-allocate as much as possible and use classes only for objects that should exist for the whole application lifetime.

Every time we have possible allocation that is unavoidable we've wrapped it in its own re-used Profiler Marker  

```cs
using (ProfilerUtils.AllocScope.Auto())
{
  // code that has possible allocation
}
```

## Defines

We have a few define symbols to change the level of debugging

- `TOMIUM_DEBUG`: Logs creation and destruction of native objects.
- `TOMIUM_LOG_ABORTEXCEPTION`: Log exceptions with callstack when we have to abort from C# 


## Support

### Scripting Backend and Platforms

|Platform    | |mono |il2cpp | |jobs*|burst |
|------------|-|-----|-------|-|-----|------|
|Windows x64 | |x    |x      | |?    |?     |
|Linux       | |?    |?      | |?    |?     |
|Mac         | |?    |?      | |?    |?     |
|Android     | |     |x      | |?    |?     |
|iOS         | |     |?      | |?    |?     |
|WebGL       | |     |x      | |?    |?     |

*x = supported, ? = support unknown*

*Tomia was structurally conceptually to support this. But needs more implementation and testing.

### Version Compatibility

The compatibilities between Wren, Tomia and Unity

|Tomia   |Unity      |Wren    |
|--------|-----------|--------|
|0.4.0   |2021.3+    |0.4.0   |
