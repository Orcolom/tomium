# Tomia

A [Wren](https://wren.io/) binding made from the ground up for Unity.

> **Tomia**: A tooth-like structures that line the inside of a birds brill. That helps them handles their typical food sources better.

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
    
    var result = vm.Interpret("<main>", "var CallMe = Fn.new{|arg|\nSystem.print(\"Hello World %(arg)\")\n}"
    );
    
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

Every time we have possible allocation it is wrapped its own re-used Profiler Marker  

```cs
using (ProfilerUtils.AllocScope.Auto())
{
  // code that has possible allocation
}
```

## Support

### Scripting Backend

Implement it whatever your scripting backend is.

|Version    |Support level |*      |
|-----------|--------------|-------|
|Mono       |full support  |       |
|IL2CPP     |full support* | Il2cpp is stricter with what is allowed and has not been tested on all platforms yet  |
|Job System |conceptual*   | **Parts of this support is currently disabled** <br> Was structurally build to support it. But not willing to promote this until more tests prove good support.  |

### Platforms

|Platform    |            |
|------------|------------|
|Windows x64 |Planned     |
|Windows x32 |Not Planned |
|Windows UWP |Not Planned |
|Linux       |Planned     |
|Mac         |Planned     |
|Android     |Planned     |
|iOS         |Planned     |

### Version Compatibility

The compatibilities between Wren, Tomia and Unity

|Tomia   |Unity      |Wren    |
|--------|-----------|--------|
|0.4.0   |2021.3+    |0.4.0   |
