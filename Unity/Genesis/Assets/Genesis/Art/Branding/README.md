# ZendaGenesis application icon

`ZendaGenesis-Fairy.png` is a 1024 x 1024 render of the project's existing
`Migration/GodotOriginal/ReusableAssets/assets/generated/icons/elf_fairy_realm.svg`.
It uses the original game fairy artwork, not an externally downloaded icon.

Active artwork comes from `E:\ZendaGame\ZendaGenesis_Icon_Kit`, copied into
`E:\ZendaGame\Genesis\Assets\Genesis\Art\Branding\IconKit`.
The default icon uses the opaque app icon, Windows uses size-specific transparent
PNGs, and Android uses Legacy plus Adaptive background/foreground layers.
WebGL postprocessing installs the kit favicon, touch icon and manifest icons,
and uses the transparent logo for the loading/footer branding. The same transparent
logo is available in Resources and displayed in the in-game Help/Creator panel.
The previous fairy and single-logo assets are retained but no longer assigned.
The stable Android
application identifier and internal resource paths are intentionally unchanged.

New release outputs:
- `E:\ZendaGame\Genesis\Builds\Windows\ZendaGenesis.exe` (distribute its full player folder)
- `E:\ZendaGame\Genesis\Builds\Android\ZendaGenesis.apk`
- `E:\ZendaGame\Genesis\Builds\WebGL` (product title ZendaGenesis)

Existing built executables are not patched or merely renamed: rebuild each target to
embed the icon, product metadata, matching data-folder name and current gameplay.
Windows runtime migration copies former Genesis saves without deleting originals.
Android keeps its application ID; WebGL deployments should retain their existing URL.