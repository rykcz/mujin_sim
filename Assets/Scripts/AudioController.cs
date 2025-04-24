using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
    }

    public Sound[] sounds;
    private Dictionary<string, AudioClip> soundDict = new Dictionary<string, AudioClip>();

    private AudioSource seSource;    // SE再生用
    private AudioSource bgmSource;   // BGM再生用

    private Coroutine fadeOutCoroutine = null;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // SEとBGM用にAudioSourceを分ける
        seSource = gameObject.AddComponent<AudioSource>();
        seSource.playOnAwake = false;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true; // BGMはループする

        foreach (var sound in sounds)
        {
            soundDict[sound.name] = sound.clip;
        }
    }

    private void Start()
    {
        PlayBGM("BGM", 0.2f);
    }

    // --------------------------------
    // 🎵 SE再生（ボリューム指定あり、同じSEは同時再生させない）
    public void PlaySE(string name, float volume = 1.0f)
    {
        if (!soundDict.TryGetValue(name, out AudioClip clip))
        {
            Debug.LogWarning($"🔇 SE '{name}' が見つかりませんでした！");
            return;
        }

        if (seSource.isPlaying && seSource.clip == clip)
        {
            Debug.Log($"🔁 SE '{name}' はすでに再生中なのでスキップします");
            return;
        }

        seSource.clip = clip;
        seSource.volume = Mathf.Clamp01(volume);
        seSource.Play();
    }

    // --------------------------------
    // 🎶 BGM再生
    public void PlayBGM(string name, float volume = 1.0f)
    {
        if (!soundDict.TryGetValue(name, out AudioClip clip))
        {
            Debug.LogWarning($"🔇 BGM '{name}' が見つかりませんでした！");
            return;
        }

        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }

        bgmSource.clip = clip;
        bgmSource.volume = Mathf.Clamp01(volume);
        bgmSource.Play();
    }

    // --------------------------------
    // 🎶 BGM停止（即時）
    public void StopBGMImmediate()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    // --------------------------------
    // 🎶 BGM停止（フェードアウトあり）
    public void StopBGMFade(float fadeDuration)
    {
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
        }

        fadeOutCoroutine = StartCoroutine(FadeOutBGM(fadeDuration));
    }

    private IEnumerator FadeOutBGM(float duration)
    {
        float startVolume = bgmSource.volume;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = 1f; // 元に戻す
    }
}
