# Tanuki.Atlyss.FontAssetsManager
Plugin for Atlyss that customizes fonts and fixes missing Unicode characters. Use custom fonts, replace existing ones, add fallbacks, and prevent missing characters from being displayed.<br>
## Features
- Replace unknown characters with codes.
- Add fallback fonts.
  - Added fallback set of [Noto Sans](https://fonts.google.com/noto) fonts for Cyrillic, Arabic, Japanese, Traditional Chinese and Korean characters.
- Replace fonts.
- Configure replacement and fallback rules using regular expressions.
### Configuration
Plugin configuration<br>
> `BepInEx/config/9c00d52e-10b8-413f-9ee4-bfde81762442.cfg`

Plugin command configuration<br>
> `BepInEx/config/Tanuki.Atlyss.FontAssetsManager/{Language}.commands.json`

Plugin translation file<br>
> `BepInEx/config/Tanuki.Atlyss.FontAssetsManager/{Language}.translations.json`

### Font fallback rules
Path
> `BepInEx/config/Tanuki.Atlyss.FontAssetsManager/Fallbacks/*.json`

Rules from different files can be combined and override or supplement each other. They are read by the plugin in alphabetical order.

Example fallbacks file `100.json`.
```json
[
  {
    "Rule":
    {
      "Object": ".*",
      "Font": ".*"
    },
    "Fixed": false,
    "Assets":
    [
      {
        "Bundle": "tanuki.atlyss.fontassetsmanager.assets.noto_sans",
        "Object": "SDF"
      },
      {
        "Bundle": "tanuki.atlyss.fontassetsmanager.assets.noto_sans_arabic",
        "Object": "SDF"
      }
    ]
  },
  {
    "Rule":
    {
      "Object": ".*",
      "Font": ".*"
    },
    "Fixed": false,
    "Assets":
    [
      {
        "Bundle": "tanuki.atlyss.fontassetsmanager.assets.noto_sans_japanese",
        "Object": "SDF"
      }
    ]
  }
]
```
Its rules are read from top to bottom and will take precedence over the rules in the `99.json` file, but may be overridden or supplemented by the rules in the `101.json` file.<br>
The `Rule` property contains regular expressions for `Text` (Unity) or `TMP_Text` (TextMeshPro) object, `Font` is the name of its current font.<br>
The `Fixed` property specifies how the plugin will handle other rules for the same regular expression. If set to `true`, the list of fallback assets will be static, meaning it can't be supplemented with other rules, but it can be overriden.<br>
The `Assets` property contains a list of fallback assets for the specified regular expression. Each asset refers to a specific asset bundle and the name of the object within it.

Fallbacks are only supported by `TMP_Text` objects.
### Replacement rules
Path
> `BepInEx/config/Tanuki.Atlyss.FontAssetsManager/Replacements/*.json`

Font replacement rules work similarly to fallback rules.
Replacement rules can only contain one rule at a time with a unique regular expression for objects of type `Text` and `TMP_Text`.
This means that duplicate regular expressions will override previously read rules.

Example replacements file `100.json`.
```json
[
  {
    "Asset":
    {
      "Bundle": "tanuki.atlyss.fontassetsmanager.assets.noto_sans",
      "Object": "Font"
    },
    "Rule":
    {
      "Object": ".*",
      "Font": ".*"
    }
  },
  {
    "Asset":
    {
      "Bundle": "tanuki.atlyss.fontassetsmanager.assets.noto_sans",
      "Object": "SDF"
    },
    "Rule":
    {
      "Object": ".*",
      "Font": ".*"
    }
  }
]
```
This example shows two rules with identical regular expressions, as one of them is intended for `Text` and the other for `TMP_Text`.

### Asset bundles
Path
> `BepInEx/plugins/*/Tanuki.Atlyss.FontAssetsManager.AssetBundles/*/*.assetbundle`

Assets must be of type `Font` (for `Text` objects) or `TMP Font Asset` (for `TextMeshPro` objects) and have unique names in the asset bundle.

Instructions for creating an asset bundle:
1. Install [Unity](https://unity.com/releases/editor/archive) (the same version as the game is recommended).
2. Create an empty project (3D Built-In Render Pipeline).
3. Import the [example package](../../tree/main/Example).
4. Create a dirrectory for asset bundle files in your project and add a label to it.
5. Place the file with your font in the directory.
6. Use the `Unicode List Extractor` tool (`Tools > Fonts`) to create a file with the characters supported by the font.
7. Select a font and create a `TMP Font Asset` for it.
   - Set `Character Set` to `Characters from File` and select the previously created `.txt` file with characters.
   - Adjust the other settings to suit your font.
8. Once the asset has been created, delete file with characters.
9. Export all asset bundles using the `Build` tool (`Tools > AssetBundles`).
10. After the build, an `Output/AssetBundles` directory will be created with all asset bundles. Select the file corresponding to the label name you specified earlier, add the `.assetbundle` extension to it, then move it to the plugin's asset bundle directory.
11. Now you can use the label of your asset bundle and the names of its items in the rules.
## Getting started
### Thunderstore
This mod on [Thunderstore](https://thunderstore.io/c/atlyss/p/Tanuki/Tanuki_Atlyss_FontAssetsManager/).<br>
It's recommended to use it together with the [EasySettings](https://thunderstore.io/c/atlyss/p/Nessie/EasySettings/) mod for easier configuration directly in the game.
### Manual installation
1. Install [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html). It's recommended to use the [preconfigured package](https://thunderstore.io/c/atlyss/p/BepInEx/BepInExPack/).
2. Install the [Tanuki.Atlyss](https://github.com/TimofeyTanuki/Tanuki.Atlyss) framework.
3. Install the [Tanuki.Atlyss.FontAssetsManager](../../releases) files.
## Anything else?
[Contacts](https://tanu.su/)