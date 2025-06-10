using UnityEngine;
using UnityEngine.UIElements;

public class DevConsoleController : MonoBehaviour
{
    public GameObject devConsoleObject;
    private bool isVisible = false;
    private const KeyCode TOGGLE_KEY = KeyCode.BackQuote; // Taste für Aufruf

    private void Awake()
    {
        if (devConsoleObject == null)
            devConsoleObject = transform.GetChild(0).gameObject;
    }
    private void Start()
    {
        if (devConsoleObject != null)
            devConsoleObject.SetActive(isVisible);
    }

    void Update()
    {
        if (Input.GetKeyDown(TOGGLE_KEY)) // ^ / `
        {
            isVisible = !isVisible;
            devConsoleObject.SetActive(isVisible);

            // Optional: Fokus auf das Eingabefeld setzen
            if (isVisible)
            {
                var uiDoc = devConsoleObject.GetComponent<UIDocument>();
                var input = uiDoc?.rootVisualElement.Q<TextField>("inputField");
                input?.Focus();
            }
        }
    }
}
