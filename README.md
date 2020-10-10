# Localizer.Net

| Release Type |                                                                     Version |
|--------------|-----------------------------------------------------------------------------|
| Release      |![Nuget](https://img.shields.io/nuget/v/Localizer.Net)                       |
| Prerelease   |![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Localizer.Net) |

| Build Type     | Status                                                                                                                            |
|----------------|-----------------------------------------------------------------------------------------------------------------------------------|
| Prerelease     |![NuGet Deploy (Prerelease)](https://github.com/trinitrotoluene/Localizer.Net/workflows/NuGet%20Deploy%20(Prerelease)/badge.svg)   |
| Last PR/commit |![Build & Test (.NET Core)](https://github.com/trinitrotoluene/Localizer.Net/workflows/Build%20&%20Test%20(.NET%20Core)/badge.svg) |

This library provides abstractions to use when localising your .NET application. It includes APIs for injecting localisations into assemblies or type instances. Included is a simple provider that pulls and flattens JSON-formatted localisations, but providers for other formats can quite easily be added as builder extensions.

## Example

Given a folder structure

```
locales/
        en-US.json
        de-DE.json
        ...
```

containing localisations in the format

```json
{
    "home": {
        "welcome": "Welcome to Example",
        "login": "Log in",
        "register": "Register"
    }
}
```

we can build a localisation using the included JSON provider

```cs
ILocalization loc = new LocalizationBuilder()
    .UseJsonFiles("locales")
    .Build();
```

and given a class

```cs
class HomeStrings
{
    [Localize("home.welcome")]
    public static LocalizableString Welcome;

    [Localize("home.login")]
    public static LocalizableString Login;

    [Localize("home.register")]
    public static LocalizableString Register;
}
```

we can inject the localization into our assembly (or a specific type if you don't want to crawl the entire assembly!)

```cs
Localizer.Inject(loc, Assembly.GetAssembly(typeof(HomeStrings)));
```

and the localised strings can be retrieved using the name of the JSON file that was loaded (this is a feature of the provider, how locales are tagged will depend on the provider implementation)

```cs
string welcome = HomeStrings.Welcome.Localize("en-US");
string login = HomeStrings.Login.Localize("de-DE");
string register = HomeStrings.Register.Localize("en-US");
```

## Localization Context

Localizer.Net supports inline C# scripting with some restrictions. You do this by supplying an `ILocalizer`/`LocalizedString` with a localisation "context".
A context is simply a param array of `(string, object)` tuples which must have unique keys that will be made available to the scripting environment during execution as global variables.

For example:

```json
{
    "somekey": "Hello, {place}"
}
```

```cs
ILocalization myLocalization.Resolve("en-US", "somekey", ("place", "world!")); // "Hello, world!"
```

However since all code between braces executes in a full Roslyn-powered scripting environment, you can also add more complexity to your localisations.

> **In order to make writing scripts in JSON less painful, all single quotation marks in a script are replaced with double quotes after parsing.**

```json
{
    "somekey": "Hello, {isAlone ? 'friends!' : 'is anybody there?'}"
}
```

```cs
ILocalization myLocalization.Resolve("en-US", "somekey", ("isAlone", false)); // "Hello, is anybody there?"
```

## Performance Considerations

Compiled scripts are cached after the first call to resolve the localisation, however any replacement that involves executing a script will incur significant overhead compared to returning a raw string literal. For this reason, any scripts that are simply variable-name replacements are just evaluated as ToString calls and formatted into the localisation.

For some idea of how expensive complex replacements are, see this sample benchmark run as of commit 022d4d2

```ini
BenchmarkDotNet=v0.12.1, OS=pop 20.04
AMD Ryzen 7 3700X, 1 CPU, 16 logical and 8 physical cores
.NET Core SDK=3.1.402
  [Host]        : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT

Job=.NET Core 3.1  Runtime=.NET Core 3.1
```
|              Method |         Mean |        Error |        StdDev |       Median |
|-------------------- |-------------:|-------------:|--------------:|-------------:|
|           Plaintext |     63.36 ns |     0.543 ns |      0.481 ns |     63.46 ns |
|  Simple replacement |    468.27 ns |     2.330 ns |      2.180 ns |    467.96 ns |
| Complex replacement | 39,606.19 ns | 8,041.173 ns | 22,811.469 ns | 28,754.00 ns |

Whether this performance penalty is acceptable will depend on your use-case. For benchmarking performance of the library on your system you can use the `src/Localizer.Net.Benchmarks` project which contains benchmarks that evaluate various features of the library.
