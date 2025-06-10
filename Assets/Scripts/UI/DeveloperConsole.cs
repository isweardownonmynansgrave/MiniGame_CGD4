using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

public class DeveloperConsole : MonoBehaviour
{
    private TextField inputField;
    private ScrollView outputArea;

    private Dictionary<string, Action<string[]>> commands = new();

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        inputField = root.Q<TextField>("inputField");
        outputArea = root.Q<ScrollView>("outputArea");

        inputField.RegisterCallback<KeyDownEvent>(OnKeyDown);

        // Commands
        commands["echo"] = Echo;
        commands["help"] = Help;
    }

    void OnKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
        {
            var text = inputField.value;
            inputField.value = "";

            AppendOutput($"> {text}");
            ParseCommand(text);
        }
    }

    void ParseCommand(string input)
    {
        string[] parts = input.Trim().Split(' ');
        if (parts.Length == 0) return;

        string cmd = parts[0].ToLower();
        string[] args = parts.Skip(1).ToArray();

        if (commands.ContainsKey(cmd))
        {
            try { commands[cmd].Invoke(args); }
            catch (Exception e) { AppendOutput($"[Error] {e.Message}"); }
        }
        else
        {
            AppendOutput($"Unknown command: {cmd}");
        }
    }

    void AppendOutput(string message)
    {
        Label newLine = new Label(message);
        outputArea.Add(newLine);
        outputArea.ScrollTo(newLine);
    }

    // === Example Commands ===

    void Echo(string[] args)
    {
        AppendOutput(string.Join(" ", args));
    }

    void Help(string[] args)
    {
        AppendOutput("Commands: " + string.Join(", ", commands.Keys));
    }
}
