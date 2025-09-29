using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
public class Menu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);
        
    }



}
