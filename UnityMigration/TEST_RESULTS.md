# Migration validation results

Executed against this source snapshot with the provided Godot console executable:

| Check | Result |
| --- | --- |
| Godot headless editor import | Exit 0 |
| CreationJourneyTest --genesis-test | 376 checks, 0 failures |
| DemoTest --demo | 737 checks, 0 failures |
| FairyAvatarTest --demo | 143 checks, 0 failures |
| ResponsiveTest --demo --mobile-preview --capture-mobile | 219 checks, 0 failures |
| Total gameplay checks | 1,475 passed |
| Save converter unit tests | 6 tests passed |
| Converter command-line smoke test | Clean opening fixture converted; original unchanged |

Additional package checks are recorded by the packaging script in VALIDATION.json:
catalog/source equality, formula parity, stable unlock references, demo state count,
60 pose samples, GLB structure, and 126 runtime asset SHA-256 hashes.
ZIP CRC and per-file package hash checks are run after assembly.

Not performed: Unity editor import/build, C# compilation of a recreated game,
Unity PlayMode tests, Android hardware profiling, or exported browser validation.
The package contains no Unity gameplay implementation claiming to pass these tests.

Source Godot tests are copied for behavior-porting reference; their 1,475 checks
are NOT represented as passing Unity tests. No live player save was read or copied.