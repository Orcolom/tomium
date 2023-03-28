# Intro

For the last 3 years, Tomium and its previous iterations have been an excuse for experimentation. In Wren's spirit and moto, being a gateway to learn c and interpreters, I wanted this plugin to be my tool to learn things I otherwise never had a reason for like: writing plugins or packages, c interop, "low-level" c#, reflections, weaving, code design intended for public use, and many other things I can't think of right now.

It has learned me many of those things, and in the progress, it has had me run into many walls that would take days of debugging, weeks of scouring documentation and months of burnout. But now that I'm through the list I wanted to learn, try and experiment it has become time to wrap it up and move on to some extent. But not before I document some of the things I learned.

# Iterations

## 1. Wrenit (2021)

This was my first attempt at making a plugin. So things were probably never going to last. But all things have to start somewhere.

Features consisted of:

- Native wren interoperability. [Read more][interop]
- Data binding and source builder using reflections.
- Build to be for all .Net projects, not just Unity.

What it didn't support was [il2cpp][il2cpp]. at this point, I was unaware of the roundabout ways you needed to fix some of these issues. Code was also heavily based and a big mishmash of existing .net implementations of Wren.

## 2. Wrenu (2022 to Apr 2022)

Redesigned to work with il2cpp and expanded upon many features.

Features consisted of:

- il2cpp support [Read more][il2cpp]
- Native wren interoperability. [Read more][interop]
- Safety checks to avoid unity crashing. [Read more][thread_safe]
- Documentation
- Syntax Tree-based source builder. The idea here was to eventually be able to parse wren scripts to c#.
- Tests
- A single sample.

## 3. Wrench (Apr 2022 to Dec 2022)

This is when I started to think about Unity job system support. I also took another shot at data binding but this time using weaving. 

Features consisted of:

- Fully struct based
- Many attempts to have weaving work.
- il2cpp support [Read more][il2cpp]
- Native wren interoperability. [Read more][interop]
- Safety checks to avoid unity crashing. [Read more][thread_safe]
- Syntax Tree-based source builder. The idea here was to eventually be able to parse wren scripts to c#.

## 4. Tomia (2023+)

This is where I decided to move towards a public 1.0.0 version of the project. and have a working sample project on every possible build target.

- Fully struct-based and job-compatible
- il2cpp support [Read more][il2cpp]
- Native wren interoperability. [Read more][interop]
- (Thread) Safety checks to avoid unity crashing. [Read more][thread_safe]
- Syntax Tree-based source builder. The idea here was to eventually be able to parse wren scripts to c#.
- Many tests and samples.
- A Sample project that can be built to all platforms as POC.

[interop]: ./interop.md
[il2cpp]: ./pitfalls_of_supporting_il2cpp.md
[thread_safe]: ./thread_safety.md