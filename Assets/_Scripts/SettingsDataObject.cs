/*
    This script defines a ScriptableObject called SettingsDataObject, 
    which is used to store audio settings for the game. It includes 
    fields for master volume, music volume, and SFX volume, as well 
    as a method to load these settings from PlayerPrefs.
*/
using UnityEngine;
[CreateAssetMenu(fileName = "SettingsData")]
public class SettingsDataObject : ScriptableObject
{
  public float MasterVolume = 1.0f;
  public float MusicVolume = 1.0f;
  public float SFXVolume = 1.0f;
  public void LoadSettings()
  {
    if (PlayerPrefs.HasKey("SettingsData") == true)
    {
      JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString("SettingsData", "{}"), this);
    }
  }
}