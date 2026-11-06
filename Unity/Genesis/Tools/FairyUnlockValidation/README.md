# Fairy unlock and localization validation

Run with the installed .NET 10 SDK from PowerShell:

```powershell
dotnet run --project 'E:\ZendaGame\Genesis\Tools\FairyUnlockValidation\FairyUnlockValidation.csproj' -- 'E:\ZendaGame\Genesis\Assets\Genesis\Resources\Genesis\Data\catalog.json' 'E:\ZendaGame\Genesis\Assets\Genesis\Resources\Genesis\Data\localization.json'
```

The runner compiles the actual pure-domain sources and uses Unity's existing
Newtonsoft.Json dependency. No test packages are downloaded. It checks sequential
fairy purchasing, legacy ownership, story gates, all translated table entries,
language selection, numeric captions, multiline captions, and English fallback.

In Unity, **Genesis > Validate > Idle persistence, VIP and completion** also checks
language save/load, older-save defaults and bundled Russian/Chinese font glyphs.

Language IDs: `en`, `es`, `ru`, `zh` (Simplified Chinese). The selector is on the
protagonist page and saves immediately, independently of character confirmation.
Resetting progression preserves the selected language. Proper names and stable
catalog IDs are not translated. Untranslated story dialogue, lore, and other
missing strings fall back to their original English; this is not a complete
translation of the narrative catalog.

The translation JSON maps an English source string to Spanish, Russian and
Simplified Chinese in that order. Every entry must contain all three translations.
Use `{#}` for each numeric value in the same order as the English caption.
Bundle fonts and their OFL licenses to keep Android/WebGL rendering independent
of operating-system font installations. Device layout checks remain necessary.