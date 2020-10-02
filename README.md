# Localizer.Net

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
string login = HomeStrings.Welcome.Localize("de-DE");
string register = HomeStrings.Register.Localize("en-US");
```

## Localization Context

**todo**
