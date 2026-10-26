# One hundred and six fairy discoveries

This is original anime-fantasy cosmology. It combines imagined creation, symbolic
duality, five elemental domains, and speculative cosmic scales. It is not presented
as established physics or an authoritative account of a spiritual tradition.

The chosen five-element set is **Wood, Fire, Earth, Metal, Water**. Other fantasy
systems use different sets; this game intentionally uses this one consistently.

| Arc | Discoveries, in order |
| --- | --- |
| Hope and Duality | Possibility Fluctuation; First Intention; Primordial Breath; Yin Sanctuary; Yang Radiance; Duality Dimension |
| Five Elements | Wood Dimension; Fire Dimension; Earth Dimension; Metal Dimension; Water Dimension; Interwoven Elemental Dimension |
| Time | First Moment; Time Dimension |
| Space | First Somewhere; Space Dimension |
| Causal Weave | Spacetime Tapestry; Causal Dimension |
| Quantum Foundations | Dreaming Quantum Sea; Quark Filament; Electron Song; Quantum Dimension |
| Matter and Stars | Molecular Promise; Molecular Nebula Cloud; Gravity Lullaby; Stellar Ignition; Stellar Dimension |
| Worlds and Spirit | Worldseed Crust; Ocean Dream; Sky Veil; Living Biosphere; Moon and Tides; Wheel of Seasons; Soul Dimension |
| Living Universe | Constellation Covenant; Galactic Heart; Galactic Web; Living Universe Dimension |
| Universe and Beyond | Daughter Universe; Branching Timeline Tree; Multiverse Loom; Possible Laws Dimension |
| Omnielle's Universe Confluence | A Chorus Without Last World; Superposed Universes; A Thousand Incompatible Nows; Where Distance Has No Common Meaning; Laws That Need Not Agree; Resonance Without Travel; The Uncollapsed Garden; Concord Without Erasure; Omniverse Confluence Dimension |
| Three Thousand Dao | First Dao Seed; Garden of Innumerable Ways; Concord of Different Ways; Three Thousand Dao Dimensions |
| Reality | Reality Dimension |
| Life Remembers | Memory in Every Seed; One Breath, Many Lives; The Inner Witness; Perception Dimension |
| Divine Recognition | All-Seeing Awareness; Presence Without Distance; Everywhere Is Within; Omnipresence Recognized; Will That Nurtures; The Unbounded Possible; Omnipotence Recognized; Divine Awareness Dimension |
| Living Divinity | The Whole That Cherishes Each; Life and Ultimate Divinity; Omniall: Life Recognizes Itself; Living Divinity Dimension |
| Dimensional Life: Angels | Wings of the First Welcome; The Sanctuary Choir; Angel Realm |
| Dimensional Life: Demons | Embers That Choose; The Freeflame Accord; Demon Realm |
| Dimensional Life: Elves and Fairies | Roots and Gossamer; The Bloomroot Covenant; Elves and Fairies Realm |
| Dimensional Life: Gods | Keepers of Many Callings; The Listening Pantheon; Gods Realm |
| Dimensional Life: Shadows | Lives Beyond the Lantern; The Quiet Lantern Pact; Shadow Realm |
| Concord of Living Realms | Five Realms, Many Voices; Thresholds of Welcome; Concord of Living Realms Dimension |
| Universal Emotions | Joy; Sorrow; Wonder; Tenderness; Courage; Anger; Compassion; Serenity; Celebration; Universal Heart Dimension |
| Human Life and Soul Resonance | Human Life on Earth; One Life, More Than One Presence; Earthly Life, Angelic Home; Souls Across Living Realms; Rooted Here, Connected Beyond; Embodied Soul Dimension |

The late-game philosophy belongs to the fictional setting: life and ultimate divinity
share a nature already latent in the first heartbeat. Discovery is recognition, not
a declaration that the player has literal supernatural powers. Omniall is this game's
name for that revelation, not a claim to represent an established doctrine.
Unity preserves individual lives; the theme is nurturing rather than domination.

## Actual gameplay

- A new universe starts with zero Hope, no production and no fairy portrait.
- Elpis costs 5 Hope. Discovering a layer reveals only the next layer.
- There are 107 claimable chapters: the opening plus one per discovery.
- Each chapter grants 25% of its discovery's base cost; the opening grants 15 Hope.
- Balanced Yin/Yang pairs give +1% global production per pair (up to 100).
- Complete sets containing all five elements give +2% production per set (up to 100).
- These two bonuses multiply; over-investing in only one domain does not raise balance.
- Living Unity: each complete set of Living Biosphere, All-Seeing Awareness,
  Omnipresence, and Omnipotence adds 2% global production, capped at 50 sets (x2).
  Removing living-world investment removes the bonus. No power produces infinite numbers.
- Miracles unlock at atoms; nebula backgrounds at nebulae; stars at Stellar Ignition;
  optional Big Bloom at Universe Horizon, not a hard-coded numeric stage.
- Visual companions show only the newest eight discovered domains to limit clutter.
- Demo previews: Only Hope, Five Elements, Living Worlds, Reality, All-Seeing,
  Living Divinity, Dao Dimensions, Omnielle. These are temporary,
  and their earlier chapters are marked remembered to match the preview objective.

## Data and balance

`E:\Godot\GenesisIdleRPG\data\creation_stages.json` is the shared source for stage
order, fairy names, domains, story descriptions, colors, and staff motifs.
`StageCatalog.gd` builds generator tables; `CreationJourney.gd` builds chapters;
`tools\generate_assets.py` generates matching original fairy SVGs.
Both export presets explicitly include the JSON catalog.

Base cost is 5 * 3.2^index. Base output is max(0.5, cost * 0.025).
This is a first balancing pass, not a claim that a full 72-stage playthrough has
been duration-tested. Automated tests fund each discovery to check reachability;
they do not measure real player waiting time. Late-game pacing needs playtesting.

## Save compatibility

The 13 original generator IDs are retained. Inserted domains default to zero in
older saves. The highest existing owned domain preserves access to earlier layers,
so existing worlds are not forced back to the opening. Historical chapter IDs are
retained where their discoveries still exist. Old one-off currency milestones no
longer appear; the save's file format/path is unchanged. Costs and outputs have
been rebalanced, so old saves can progress faster or slower than before.
All previous 60 discovery IDs retain their relative order. Four Dao discoveries
are inserted before Reality; advanced saves can access inserted earlier foundations.
Previously claimed chapter rewards retain their IDs.
## Dimension construction

Existence in this fictional cosmology is built on multiple interwoven dimensions.
Each major arc ends in construction, with 29 explicit dimension milestones including
five elemental dimensions, time, space, quantum, the living universe, and reality.
Dimensions are domains of existence, not a count of geometric axes or power ranks.
Any domain can precede or follow another in the lore. The implemented game uses one
fixed sequential path; free-order selection and randomized paths are not implemented.
Later dimensions can become foundations of already-built worlds.

Three Thousand Dao Dimensions is one collective construction milestone preceded by
three discoveries, not 3,000 individually simulated or separately purchased domains.
It evokes abundant ways in an original fantasy rather than a canonical doctrine.

Dimension cards say Build before first purchase and Dimension built afterward.
Their chapters record construction and show a DIMENSION BUILT reward cue.

## Phone preview

`E:\Godot\GenesisIdleRPG\Play Demo.bat` opens a 390x844 portrait desktop
window with the responsive phone layout and no save access. Scroll vertically to
reach creation cards. `E:\Godot\GenesisIdleRPG\Play Desktop Demo.bat` opens
the 1280x720 desktop version. This is a viewport preview, not physical-device validation.

## Omnielle's Universe Confluence

A dedicated nine-discovery arc develops Omnielle's role: she shelters relationships
between infinitely many universes without reducing them to a single world.
Superposition here is a fantasy metaphor, not established physics. Universes retain
their local clocks, geometry, and laws; there is no shared external timeline,
measurable separation, or universal causal rulebook for their confluence.
Resonance is fictional, not a physical faster-than-light communication mechanism.
Recognition neither selects one true universe nor erases the others.

Eight discoveries are inserted before the existing `omniverse` milestone. Its ID
and `first_omniverse` chapter ID remain intact; the relative order of previous
stages is unchanged. Advanced saves can access inserted earlier layers. Possible
Laws now concludes its previous arc with a dimension; Omnielle concludes her own.
All 106 costs and production values remain finite. Infinite universes are represented
in lore, not by spawning or simulating infinite objects. Existing Living Unity and
harmony rules are unchanged. Waiting-time balance still requires playtesting.

The Omnielle demo preset prepares Concord Without Erasure, reveals her concluding
Build button, and provides enough currency to construct the confluence. The preset
is appended so earlier preview indices remain unchanged. Phone capture:
`E:\Godot\GenesisIdleRPG\omnielle-mobile-preview.png`.

## Dimensional Life Realms saga

Eighteen discoveries follow Living Divinity: five three-stage realm arcs, followed
by a three-stage concord. Each realm awakens its inhabitants, establishes a social
covenant, and culminates in a purchasable dimension. The realms are Angel, Demon,
Elves and Fairies (one shared home), Gods, and Shadow. The finale builds a dimension
of relationship; it does not consume or replace the five sovereign homes.

These are original fantasy societies, not religious claims or a moral ranking of
species. Angels are not infallible, demons are not inherently evil, shadows are a
habitat, and gods are accountable stewards rather than beings of greater worth.
The order is the game's sequential path, not a universal hierarchy of life.

The original 72 stages and chapter IDs are unchanged; appending the saga preserves
their costs and output. Old completed saves can discover the first angel stage;
new counts default to zero. No save reset is required. Existing Living Unity
and elemental/duality rules are not extended by this saga; it adds standard
generators, chapter rewards, lore, icons, and dimension builds, not combat or travel.

Six appended demo presets prepare each realm and the final concord with enough
currency for its Build action. Earlier eight preview indices are preserved. The
Living Divinity preview now opens the saga instead of ending the whole journey.
Phone capture: `E:\Godot\GenesisIdleRPG\life-realms-mobile-preview.png`.
Real waiting-time balance is not validated by funded reachability tests.

## Universal Emotions saga and animation unlocks

Ten appended discoveries explore emotions as relationships throughout creation,
not literal physical forces or a claim that all beings must feel the same way.
The Universal Heart Dimension concludes this arc. Sorrow, fear, and anger are not
evil or failures; the story emphasizes room for feelings and care in choosing acts.

| Chapter | Animation unlocked on claim |
| --- | --- |
| Joy: Creation in Bloom | Happy |
| Sorrow: What Love Remembers | Sorrow |
| Wonder: The Unopened Sky | Surprised |
| Tenderness: The Trust to Be Seen | Shy |
| Courage: A Light Beside Fear | Courage |
| Anger: The Boundary Flame | Anger |
| Compassion: No Heart Outside | Compassion |
| Serenity: A Place for Every Feeling | Serenity |
| Celebration: The Many-Hearted Dance | Dance |

Idle is always available. Other animations unlock only when their chapter reward
is claimed, not merely when its generator is purchased. The unlock uses existing
saved completed chapter IDs and survives Big Bloom; no new save field is needed.
Older saves retain their progress, but previously unrestricted prototype animations
now require their corresponding new chapters. Menu entries show locked states and
their chapter requirements. Tapping reacts with Happy only after Joy is remembered;
ordinary chapter rewards trigger Dance only after Celebration is remembered.
New emotion claims immediately play their newly unlocked pose and show a banner.

Universal Emotions preview (index 14) funds the first discovery; Universal Heart
(index 15) remembers chapters through Universal Heart and exposes every expression. Switching previews
clears unavailable reactions. There are eighteen demo presets in total. Preview:
`E:\Godot\GenesisIdleRPG\emotions-mobile-preview.png`.

The current character is still procedural placeholder art. These are joint/eye
animation states, not polished animation clips or an upgrade to the reference's
illustration quality. Unlock logic is separated from character geometry so it can
be reused when the visual model is replaced.

## Human life and simultaneous soul-presence

Six discoveries follow Universal Heart, imagining a human embodied on planet Earth
while their soul may also reside in the Angel Realm or other living realms at the
same time. This is fictional cosmology, not a claim about real people or evidence
of supernatural abilities. It is coexistence, not death, reincarnation, or leaving
the body. Different beliefs are welcome; no realm defines a person's worth.

Each discovery has its own fairy, production source, chapter, and reward. The arc
ends by constructing the Embodied Soul Dimension. Existing discovery/chapter IDs
and emotion requirements are unchanged. Human & Soul (index 16) funds the opening;
Embodied Soul (index 17) funds the final construction without pre-building it.
Earthly care and relationships remain important throughout the story.
Phone capture: `E:\Godot\GenesisIdleRPG\soul-realms-mobile-preview.png`.
