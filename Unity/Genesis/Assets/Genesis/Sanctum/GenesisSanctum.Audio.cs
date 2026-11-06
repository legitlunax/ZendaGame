using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        public bool AudioEnabled { get; private set; } = true;
        public bool AssetCreditsOpen { get; private set; }
        private AudioSource musicSource, buttonSource;
        private AudioListener ownedListener;
        private AudioClip buttonChime;
        private Texture2D musicOnIcon, musicOffIcon;
        private GUIStyle rpgInputStyle;
#if UNITY_WEBGL && !UNITY_EDITOR
        // Browsers only unlock their audio context after a user gesture, so this flag
        // is WebGL-only. Declaring it on every platform would leave it written but
        // never read (CS0414) inside the Windows and Android players.
        private bool audioGestureReceived;
#endif
        private int lastButtonSoundFrame = -1;
        private Vector2 assetLicenseScroll;
        private string assetLicenseText;

        private void UpdateGameAudio()
        {
            if (!Application.isPlaying || !isActiveAndEnabled) return;
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
                musicSource.spatialBlend = 0;
                musicSource.volume = 0.14f;
                musicSource.clip = Resources.Load<AudioClip>("Genesis/Audio/MysticalPiano");
                buttonSource = gameObject.AddComponent<AudioSource>();
                buttonSource.playOnAwake = false;
                buttonSource.spatialBlend = 0;
                buttonSource.volume = 0.18f;
                buttonChime = CreateButtonChime();
                // The release scene does not contain a listener. Do not add a second one
                // when running alongside another camera or a user's preview scene.
                bool listenerPresent = false;
                foreach (AudioListener listener in FindObjectsByType<AudioListener>())
                    if (listener.isActiveAndEnabled) { listenerPresent = true; break; }
                if (!listenerPresent) ownedListener = gameObject.AddComponent<AudioListener>();
            }
            RefreshAudioPlayback();
        }

        private void RefreshAudioPlayback()
        {
            if (musicSource == null) return;
            bool audible = AudioEnabled && !lostFocus && !appPaused && isActiveAndEnabled;
            musicSource.mute = buttonSource.mute = !audible;
            if (!audible) { PauseGameAudio(); return; }
#if UNITY_WEBGL && !UNITY_EDITOR
            // Browsers require a player gesture before their audio context can start.
            if (!audioGestureReceived) return;
#endif
            if (musicSource.clip != null && !musicSource.isPlaying)
            {
                if (musicSource.timeSamples > 0) musicSource.UnPause();
                else musicSource.Play();
            }
        }

        private void PauseGameAudio()
        {
            if (musicSource != null) musicSource.Pause();
            if (buttonSource != null) buttonSource.Stop();
        }

        /// <summary>
        /// Records that the player interacted with the UI. Browsers require a user
        /// gesture before their audio context may start, so the flag is only kept
        /// for WebGL players; everywhere else this is a no-op.
        /// </summary>
        private void MarkAudioGesture()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            audioGestureReceived = true;
#endif
        }

        public void SetAudioEnabled(bool enabled)
        {
            AudioEnabled = enabled;
            RefreshAudioPlayback();
            SavePlayerProgress();
        }

        private void PlayButtonSound()
        {
            MarkAudioGesture();
            if (!AudioEnabled || lostFocus || appPaused || !Application.isPlaying) return;
            if (lastButtonSoundFrame == Time.frameCount) return;
            lastButtonSoundFrame = Time.frameCount;
            UpdateGameAudio();
            if (buttonSource != null && buttonChime != null) buttonSource.PlayOneShot(buttonChime);
        }

        private bool SoundButton(Rect rect, string caption, GUIStyle style)
        { return SoundButton(rect, new GUIContent(caption), style); }

        private bool SoundButton(Rect rect, GUIContent caption, GUIStyle style)
        {
            bool enchantedButton = style == action || style == mobileButton;
            if (enchantedButton) DrawEnchantedButton(rect, SelectedTab == 1 && !GoddessPopupOpen);
            var content = new GUIContent(L(caption.text), caption.image, L(caption.tooltip));
            GUIStyle renderedStyle = FittedLocalizedStyle(rect, content, style);
            if (gameTheme != null && gameTheme.button != null)
            {
                renderedStyle = new GUIStyle(renderedStyle);
                ClearButtonBackgrounds(renderedStyle);
            }
            bool clicked = GUI.Button(rect, content, renderedStyle);
            if (clicked) PlayButtonSound();
            return clicked;
        }

        private static AudioClip CreateButtonChime()
        {
            const int sampleRate = 44100;
            const float duration = 0.38f;
            var samples = new float[(int)(sampleRate * duration)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)sampleRate;
                // Rounded attack/release and mostly fundamental energy: a felt-like
                // wooden note, not the previous bright, high-frequency fairy bell.
                float attack = Mathf.SmoothStep(0, 1, t / 0.025f);
                float release = Mathf.SmoothStep(0, 1, (duration - t) / 0.12f);
                float envelope = attack * Mathf.Exp(-t * 9) * release;
                samples[i] = envelope * (0.34f * Mathf.Sin(2 * Mathf.PI * 330 * t)
                    + 0.065f * Mathf.Sin(2 * Mathf.PI * 495 * t) * Mathf.Exp(-t * 8)
                    + 0.018f * Mathf.Sin(2 * Mathf.PI * 660 * t) * Mathf.Exp(-t * 16));
            }
            AudioClip clip = AudioClip.Create("Zenda mellow felt button note", samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void DrawAudioToggle(Rect rect)
        {
            EnsureIconStyles();
            if (musicOnIcon == null) musicOnIcon = CreateMusicIcon(false);
            if (musicOffIcon == null) musicOffIcon = CreateMusicIcon(true);
            DrawEnchantedSocket(rect, AudioEnabled);
            Color previous = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(rect.x + 9, rect.y + 9, rect.width - 18, rect.height - 18), AudioEnabled ? musicOnIcon : musicOffIcon);
            GUI.color = previous;
            if (GUI.Button(rect, new GUIContent("", L(AudioEnabled ? "Mute music and sounds" : "Enable music and sounds")), invisibleButton))
            {
                MarkAudioGesture();
                SetAudioEnabled(!AudioEnabled);
                if (AudioEnabled) PlayButtonSound();
            }
        }

        private static Texture2D CreateMusicIcon(bool muted)
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { name = muted ? "Music muted" : "Music enabled", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f - 48) / 48, v = (y + 0.5f - 48) / 48;
                    float heads = Mathf.Min(Ellipse(u, v, -0.40f, -0.43f, 0.24f, 0.17f), Ellipse(u, v, 0.29f, -0.30f, 0.24f, 0.17f));
                    float stems = Mathf.Min(MusicLineDistance(new Vector2(u, v), new Vector2(-0.20f, -0.43f), new Vector2(-0.20f, 0.48f)),
                        MusicLineDistance(new Vector2(u, v), new Vector2(0.49f, -0.30f), new Vector2(0.49f, 0.61f)));
                    float beam = MusicLineDistance(new Vector2(u, v), new Vector2(-0.20f, 0.48f), new Vector2(0.49f, 0.61f));
                    float alpha = Mathf.Max(Mathf.Clamp01((1 - heads) * 12), Mathf.Clamp01((0.075f - Mathf.Min(stems, beam)) * 48));
                    Color color = new Color(1f, 0.87f, 0.62f, alpha);
                    if (muted)
                    {
                        float slash = Mathf.Clamp01((0.085f - MusicLineDistance(new Vector2(u, v), new Vector2(-0.72f, 0.74f), new Vector2(0.72f, -0.74f))) * 48);
                        color = Color.Lerp(color, new Color(1f, 0.42f, 0.48f, 1), slash);
                    }
                    texture.SetPixel(x, y, color);
                }
            texture.Apply(false, true);
            return texture;
        }

        private static float Ellipse(float x, float y, float cx, float cy, float rx, float ry)
        { return (x - cx) * (x - cx) / (rx * rx) + (y - cy) * (y - cy) / (ry * ry); }

        private static float MusicLineDistance(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 edge = end - start;
            return (point - start - edge * Mathf.Clamp01(Vector2.Dot(point - start, edge) / edge.sqrMagnitude)).magnitude;
        }

        private void DrawAssetCredits(float height)
        {
            if (!AssetCreditsOpen) return;
            DrawReadingSurface(new Rect(12, 80, 366, height - 164));
            LocalizedLabel(new Rect(26, 96, 338, 38), "Music & enchanted lettering", mobileTitle);
            if (assetLicenseText == null)
            {
                assetLicenseText = "";
                foreach (string name in new[] { "Asset-Credits", "Almendra-OFL", "CrimsonText-OFL" })
                {
                    TextAsset text = Resources.Load<TextAsset>("Genesis/Licenses/" + name);
                    if (text != null) assetLicenseText += text.text + "\n\n";
                }
            }
            float textHeight = LocalizedStyle(mobileText).CalcHeight(new GUIContent(L(assetLicenseText)), 302);
            assetLicenseScroll = GUI.BeginScrollView(new Rect(26, 142, 338, Mathf.Max(40, height - 300)), assetLicenseScroll,
                new Rect(0, 0, 304, textHeight + 12));
            LocalizedLabel(new Rect(0, 0, 302, textHeight), assetLicenseText, mobileText);
            GUI.EndScrollView();
            if (MobileButton(new Rect(26, height - 142, 338, 44), "Close / Return to Zenda")) AssetCreditsOpen = false;
        }

        private void ReleaseGameAudio()
        {
            PauseGameAudio();
            if (musicSource != null) Destroy(musicSource);
            if (buttonSource != null) Destroy(buttonSource);
            if (ownedListener != null) Destroy(ownedListener);
            if (buttonChime != null) Destroy(buttonChime);
            if (musicOnIcon != null) Destroy(musicOnIcon);
            if (musicOffIcon != null) Destroy(musicOffIcon);
        }
    }
}
