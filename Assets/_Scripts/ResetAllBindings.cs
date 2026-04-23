/*
    This script is responsible for helping with keybind 
    customization as per the requirments.
*/
using UnityEngine;
using UnityEngine.InputSystem;

// This script is part of the key bind system as stated in the requirments. 
public class ResetAllBindings : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    public void resetBindings()
    {
        foreach(InputActionMap map in inputActions.actionMaps)
        {
            map.RemoveAllBindingOverrides();
        }
        PlayerPrefs.DeleteKey("rebinds");
    }
}
