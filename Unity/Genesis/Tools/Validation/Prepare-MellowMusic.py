"""Prepare Indieteur's CC0 Mystical Piano using the author's 1:35 loop point."""
import pathlib
import wave

ROOT = pathlib.Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'Tools/Validation/MysticalPiano-source.wav'
DESTINATION = ROOT / 'Assets/Genesis/Resources/Genesis/Audio/MysticalPiano.wav'

with wave.open(str(SOURCE), 'rb') as source:
    assert source.getsampwidth() == 2 and source.getnchannels() == 2
    frames = source.getframerate() * 95
    assert source.getnframes() >= frames
    data = source.readframes(frames)
    with wave.open(str(DESTINATION), 'wb') as output:
        output.setparams(source.getparams())
        output.writeframes(data)
with wave.open(str(DESTINATION), 'rb') as result:
    assert result.getnframes() / result.getframerate() == 95
    print(f'PASS: {DESTINATION} - 95 seconds, stereo PCM; author-recommended loop cut')