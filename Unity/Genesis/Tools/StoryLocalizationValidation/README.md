# Story localization status

The offline `Assets/Genesis/Resources/Genesis/Data/story-localization.json` table
currently contains 271 English-keyed entries, each with Spanish, Russian and
Simplified Chinese translations, in that order. The first thirteen chapters
(`first_light`, `first_possibility`, `discover_first_intention`,
`discover_primordial_breath`, `discover_yin`, `discover_yang`,
`discover_yin_yang`, `discover_element_wood`, `discover_element_fire`,
`discover_element_earth`, `discover_element_metal`, `discover_element_water`,
and `discover_elemental_harmony`)
are fully translated, including
all conversation passages, chapter titles, goals and lore. This includes the
resident introductions embedded in Aeria's, Lianghua's, Sylvara's, Ignis's,
Terrielle's, Argentia's, Mizuelle's, and Wuxielle's chapters.
Other chapters retain English wherever translations are not yet supplied.
This is NOT a complete translation of the story library.

Story passages are translated before pagination. Speaker names, portrait IDs,
moods and progression IDs are unchanged. Changing language while reading
rebuilds the chapter and returns to its first page, without marking it finished.

Run from PowerShell:

```powershell
dotnet run --project 'E:\ZendaGame\Genesis\Tools\StoryLocalizationValidation\StoryLocalizationValidation.csproj' --verbosity quiet -tl:off
```

The validator checks every chapter in every supported language for nonempty,
bounded pages, speaker preservation and unchanged English output. It checks
complete translation coverage for the first thirteen chapters, and proper-name,
Unicode and paragraph-break preservation across all translated entries. It also exports
the full source inventory and missing translations to `E:\ZendaGame\Genesis\Temp`.

Current inventory: 2,174 unique source strings, including chapter titles/goals,
story passages, resident descriptions, and gallery histories. 1,903 remain
untranslated. These coverage numbers must not be confused with the number
of pagination checks passing.

## UI localization audit — October 27, 2026

The separate UI table at
`E:\ZendaGame\Genesis\Assets\Genesis\Resources\Genesis\Data\localization.json`
now contains 261 entries (including three inspiration credits), each translated into Spanish,
Russian and Simplified Chinese. These are UI entries, not additional story coverage.

Added coverage includes halo names/equipment states, help and reaction tooltips,
reset confirmations, speed-code results, completion credits, Journey reward/bond
summaries, gallery empty states, save status, and story-reader captions/gift buttons.
Dynamic sentences now use translated format templates where concatenation previously
prevented lookup. Names remain substitution values, not translation keys.

The validator additionally checks UI table shape, numeric and indexed placeholder
parity, template formatting, name preservation, and representative live captions.
No native-speaker review or in-editor visual layout validation was performed in
this audit. Artwork and hexagonal portraits were not changed.

Remaining audit targets include catalog-backed chapter titles/goals, creation and
resident descriptions, gallery histories, older dimension/prototype screens, some
assembled notification messages and error details. License documents remain intact.
The heuristic scan in `E:\ZendaGame\Genesis\Temp\ui-translation-audit.json` includes
internal asset names and IDs as well as UI strings; it is not a translation count.

## Chapters 6–7 translation batch — October 27, 2026

Added 38 exact English-keyed entries (114 translated values) covering Riyara's
awakening and the construction of the Duality Dimension: titles, objectives,
lore, recollections, conversations, and introductions for Lady Sable, Lady Sunniva,
Lady Maelle, Lady Ruoxi, and Lady Tamsin. Character names remain unchanged.
The same resident introduction keys are also used by their catalog descriptions.
Standalone gallery-history coverage is not implied by chapter coverage.

The validator exports the next two untranslated chapters to
`E:\ZendaGame\Genesis\Temp\next-complete-chapters.json`. At the end of this batch,
the next chapters were the Wood and Fire Dimensions. No native-speaker review or
new visual runtime validation was performed for this batch.

## Chapters 8–9 translation batch — October 27, 2026

Added 56 exact English-keyed entries (168 translated values) covering the Wood
and Fire Dimensions: titles, objectives, lore, recollections, conversations,
Lady Ignis's Hearthkindler title passage, and all 14 resident introductions.
Place names match the existing UI terminology; character names remain unchanged.
The repeated boundaries recollection is shared by both chapters through one key.
These resident introductions also translate the matching catalog descriptions;
standalone gallery histories are not claimed as complete.

Complete-chapter coverage assertions now include the first nine chapters.
The next exported batch covers the Earth and Metal Dimensions. Native-speaker
review and in-editor visual validation remain outstanding.

Validation: all 428 chapter/language pagination and metadata checks passed, as did
exact-key, name, paragraph-break, Unicode and duplicate-key checks. The Sanctum
build completed with zero errors and zero warnings. There are 171 translated
story entries and 2,003 inventoried source strings still untranslated.

## Chapters 10–11 translation batch — October 27, 2026

Added 50 exact English-keyed entries (150 translated values) for the Earth and
Metal Dimensions: titles, objectives, lore, recollections, conversations, and all
14 resident introductions. Identical boundary/cooperation passages reuse existing
translations. Domain names match the UI table, and character names are unchanged.
The batch application verified exact chapter source sets and preserved every
pre-existing translation. Matching resident catalog descriptions are translated;
standalone gallery histories are not claimed as complete.

Coverage assertions now enforce complete translation through chapter 11. All 428
chapter/language pagination and metadata checks passed, including name, Unicode,
paragraph-break and exact-key validation. The Sanctum build completed with zero
errors and zero warnings. The story table now has 221 entries, with 1,953
inventoried source strings still untranslated.

The next exported batch covers the Water and Interwoven Elemental Dimensions.
Native-speaker review and in-editor visual validation remain outstanding.

## Chapters 12–13 translation batch — October 27, 2026

Added 50 exact English-keyed entries (150 translated values) for the Water and
Interwoven Elemental Dimensions: titles, objectives, lore, recollections,
conversations, and all 14 resident introductions. Identical shared passages reuse
existing translations. Domain names match the UI table and character names are
unchanged. The batch verifies exact source sets and preserves all earlier entries.
Matching resident catalog descriptions are translated; standalone gallery histories
are not claimed as complete.

Complete-chapter coverage assertions now include the first thirteen chapters.
All 428 chapter/language pagination and metadata checks passed, along with exact-key,
name, Unicode, paragraph-break and duplicate-key checks. The Sanctum build completed
with zero errors and zero warnings. There are 271 translated story entries and
1,903 inventoried source strings still untranslated.

The next exported batch covers Tickelle's awakening and the Time Dimension.
Native-speaker review and in-editor visual validation remain outstanding.