using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    public GameObject mainMenuObject;
    public GameObject settingsObject;
    
    [Header("Main Menu UI Elements")]
    public TextMeshProUGUI seedField;

    [Header("Settings Menu UI Elements")]
    public Slider viewDstSlider;
    public TextMeshProUGUI viewDstText;
    public Slider mouseSlider;
    public TextMeshProUGUI mouseTxtSlider;
    public Toggle threadingToggle;
    
    Settings settings;
    public void Awake()
    {
        if (!File.Exists(Application.dataPath + "/settings.cfg"))
        {
            Debug.Log("No settings file found, creating one");
            settings = new Settings();
            string jsonExport = JsonUtility.ToJson(settings);
            File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);
        }else
        {
            Debug.Log("Loading settings file");
            string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
            settings = JsonUtility.FromJson<Settings>(jsonImport);
        }
    }
    
    public void StartGame()
    {
        VoxelData.seed = Mathf.Abs(seedField.text.GetHashCode()) / VoxelData.WorldSizeInChunks;
        SceneManager.LoadScene("Scene1", LoadSceneMode.Single);
    }
    
    public void EnterSettings()
    {
        viewDstSlider.value = settings.viewDistance;
        UpdateViewDstSlider();
        mouseSlider.value = settings.mouseSensitivity*10;
        UpdateMouseSlider();
        threadingToggle.isOn = settings.enableThreading;
        mainMenuObject.SetActive(false);
        settingsObject.SetActive(true);
    }

    public void LeaveSettings() {
        settings.viewDistance = (int)viewDstSlider.value;
        settings.mouseSensitivity = mouseSlider.value/10;
        settings.enableThreading = threadingToggle.isOn;

        string jsonExport = JsonUtility.ToJson(settings);
        File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);

        mainMenuObject.SetActive(true);
        settingsObject.SetActive(false);
    }

    public void QuitGame(){
        Application.Quit();
    }

    public void UpdateViewDstSlider()
    {
        viewDstText.text = "View Distance: " + viewDstSlider.value;
    }
    public void UpdateMouseSlider()
    {
        float adjustedValue = mouseSlider.value/10;
        mouseTxtSlider.text = "Mouse Sensitivity: " + adjustedValue.ToString("F1");
    }
}
