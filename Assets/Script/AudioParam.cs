using UnityEngine;
using UnityEngine.UI;

public class AudioParam : MonoBehaviour
{
    public Slider volumeSlider;

    void Start()
    {
        // initialiser le slider avec le volume actuel
        volumeSlider.value = AudioListener.volume;

        // quand le slider change, on appelle SetVolume
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;   // valeur entre 0 et 1
    }
}
