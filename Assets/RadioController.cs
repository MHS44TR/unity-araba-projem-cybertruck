using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class RadioController : MonoBehaviour
{
    [Header("Radio Stations")]
    public RadioStation[] radioStations;
    
    [Header("Audio Settings")]
    public AudioMixerGroup radioMixerGroup;
    public float maxVolume = 1.0f;
    public float fadeTime = 1.0f;
    
    [Header("Static Effect")]
    public AudioSource staticSound;
    public float staticDuration = 0.5f;
    public float staticVolume = 0.3f;

    [Header("Radio State")]
    public bool radioEnabled = true;
    public bool shuffleMode = false;
    public bool repeatMode = false;

    private int currentStationIndex = -1;
    private bool isSwitchingStations = false;
    private bool isFading = false;
    private float originalStaticVolume;
    private Coroutine currentFadeCoroutine;

    [System.Serializable]
    public class RadioStation
    {
        public string stationName;
        public AudioSource audioSource;
        public Sprite stationIcon;
        [TextArea] public string stationDescription;
        public float volume = 1.0f;
        public bool unlocked = true;
    }

    public event Action<int> OnStationChanged;
    public event Action<bool> OnRadioToggled;

    void Start()
    {
        InitializeRadio();
        
        // Set initial static volume
        if (staticSound != null)
        {
            originalStaticVolume = staticSound.volume;
            staticSound.volume = staticVolume;
        }
    }

    void InitializeRadio()
    {
        foreach (RadioStation station in radioStations)
        {
            if (station.audioSource != null)
            {
                station.audioSource.Stop();
                station.audioSource.loop = true;
                station.audioSource.playOnAwake = false;
                
                // Apply mixer group if assigned
                if (radioMixerGroup != null)
                {
                    station.audioSource.outputAudioMixerGroup = radioMixerGroup;
                }
            }
        }

        if (staticSound != null)
        {
            staticSound.loop = false;
            staticSound.playOnAwake = false;
            if (radioMixerGroup != null)
            {
                staticSound.outputAudioMixerGroup = radioMixerGroup;
            }
        }
    }

    public void ToggleRadio()
    {
        radioEnabled = !radioEnabled;
        
        if (radioEnabled)
        {
            // If radio was turned on and we have a current station, play it
            if (currentStationIndex != -1)
            {
                PlayStation(currentStationIndex);
            }
        }
        else
        {
            // Turn off all stations
            StopAllStations();
        }
        
        OnRadioToggled?.Invoke(radioEnabled);
    }

    public void SwitchStation(int stationIndex)
    {
        if (!radioEnabled || isSwitchingStations || stationIndex < 0 || stationIndex >= radioStations.Length)
            return;

        if (!radioStations[stationIndex].unlocked)
        {
            Debug.Log($"Station {radioStations[stationIndex].stationName} is locked!");
            return;
        }

        if (stationIndex == currentStationIndex && radioStations[stationIndex].audioSource.isPlaying)
        {
            // Same station, toggle pause
            TogglePauseCurrentStation();
            return;
        }

        StartCoroutine(SwitchStationWithEffects(stationIndex));
    }

    public void NextStation()
    {
        if (radioStations.Length == 0) return;

        int nextIndex;
        if (shuffleMode)
        {
            nextIndex = GetRandomStationIndex();
        }
        else
        {
            nextIndex = (currentStationIndex + 1) % radioStations.Length;
        }

        SwitchStation(nextIndex);
    }

    public void PreviousStation()
    {
        if (radioStations.Length == 0) return;

        int previousIndex;
        if (shuffleMode)
        {
            previousIndex = GetRandomStationIndex();
        }
        else
        {
            previousIndex = (currentStationIndex - 1 + radioStations.Length) % radioStations.Length;
        }

        SwitchStation(previousIndex);
    }

    private IEnumerator SwitchStationWithEffects(int newStationIndex)
    {
        isSwitchingStations = true;

        // Fade out current station if playing
        if (currentStationIndex != -1 && radioStations[currentStationIndex].audioSource.isPlaying)
        {
            yield return FadeStation(currentStationIndex, 0f, fadeTime * 0.5f);
            radioStations[currentStationIndex].audioSource.Stop();
        }

        // Play static effect
        if (staticSound != null && staticDuration > 0f)
        {
            staticSound.Play();
            yield return new WaitForSeconds(staticDuration);
        }

        // Play new station with fade in
        PlayStation(newStationIndex);
        yield return FadeStation(newStationIndex, GetStationVolume(newStationIndex), fadeTime * 0.5f);

        currentStationIndex = newStationIndex;
        isSwitchingStations = false;

        OnStationChanged?.Invoke(currentStationIndex);
    }

    private void PlayStation(int stationIndex)
    {
        if (stationIndex < 0 || stationIndex >= radioStations.Length) return;

        var station = radioStations[stationIndex];
        if (station.audioSource != null)
        {
            station.audioSource.volume = 0f; // Start at 0 for fade in
            station.audioSource.Play();
        }
    }

    private IEnumerator FadeStation(int stationIndex, float targetVolume, float duration)
    {
        if (stationIndex < 0 || stationIndex >= radioStations.Length || 
            radioStations[stationIndex].audioSource == null) yield break;

        isFading = true;
        
        AudioSource audioSource = radioStations[stationIndex].audioSource;
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
        isFading = false;
    }

    public void SetVolume(float volume)
    {
        maxVolume = Mathf.Clamp01(volume);
        
        // Update current station volume if playing
        if (currentStationIndex != -1 && radioStations[currentStationIndex].audioSource.isPlaying)
        {
            radioStations[currentStationIndex].audioSource.volume = GetStationVolume(currentStationIndex);
        }
    }

    public void SetStationVolume(int stationIndex, float volume)
    {
        if (stationIndex >= 0 && stationIndex < radioStations.Length)
        {
            radioStations[stationIndex].volume = Mathf.Clamp01(volume);
            
            // If this is the current playing station, update immediately
            if (stationIndex == currentStationIndex && radioStations[stationIndex].audioSource.isPlaying)
            {
                radioStations[stationIndex].audioSource.volume = GetStationVolume(stationIndex);
            }
        }
    }

    private float GetStationVolume(int stationIndex)
    {
        return radioStations[stationIndex].volume * maxVolume;
    }

    public void TogglePauseCurrentStation()
    {
        if (currentStationIndex == -1) return;

        var station = radioStations[currentStationIndex];
        if (station.audioSource.isPlaying)
        {
            station.audioSource.Pause();
        }
        else
        {
            station.audioSource.UnPause();
        }
    }

    public void StopAllStations()
    {
        foreach (var station in radioStations)
        {
            if (station.audioSource != null)
            {
                station.audioSource.Stop();
            }
        }
        currentStationIndex = -1;
    }

    public void UnlockStation(int stationIndex)
    {
        if (stationIndex >= 0 && stationIndex < radioStations.Length)
        {
            radioStations[stationIndex].unlocked = true;
        }
    }

    public void LockStation(int stationIndex)
    {
        if (stationIndex >= 0 && stationIndex < radioStations.Length)
        {
            radioStations[stationIndex].unlocked = false;
            
            // If locked station is currently playing, stop it
            if (stationIndex == currentStationIndex)
            {
                StopAllStations();
            }
        }
    }

    private int GetRandomStationIndex()
    {
        if (radioStations.Length == 0) return -1;
        
        // Get only unlocked stations for random selection
        var unlockedStations = new System.Collections.Generic.List<int>();
        for (int i = 0; i < radioStations.Length; i++)
        {
            if (radioStations[i].unlocked)
            {
                unlockedStations.Add(i);
            }
        }
        
        if (unlockedStations.Count == 0) return -1;
        
        int randomIndex = UnityEngine.Random.Range(0, unlockedStations.Count);
        return unlockedStations[randomIndex];
    }

    public RadioStation GetCurrentStation()
    {
        if (currentStationIndex >= 0 && currentStationIndex < radioStations.Length)
        {
            return radioStations[currentStationIndex];
        }
        return null;
    }

    public string GetCurrentStationName()
    {
        var station = GetCurrentStation();
        return station != null ? station.stationName : "Radio Off";
    }

    public bool IsPlaying()
    {
        return currentStationIndex != -1 && 
               radioStations[currentStationIndex].audioSource != null && 
               radioStations[currentStationIndex].audioSource.isPlaying;
    }

    // For UI integration
    public void SetStaticVolume(float volume)
    {
        staticVolume = Mathf.Clamp01(volume);
        if (staticSound != null)
        {
            staticSound.volume = staticVolume;
        }
    }

    public void ToggleShuffleMode()
    {
        shuffleMode = !shuffleMode;
    }

    public void ToggleRepeatMode()
    {
        repeatMode = !repeatMode;
        foreach (var station in radioStations)
        {
            if (station.audioSource != null)
            {
                station.audioSource.loop = repeatMode;
            }
        }
    }

    void OnDestroy()
    {
        // Clean up coroutines
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
    }
}