using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using System.IO;

public class DialogueVariables
{
    private Dictionary<string, Ink.Runtime.Object> variables;
    public DialogueManager dialogueManager;


    public DialogueVariables(string globalsFilePath)
    {
        // Compile the story
        string inkFilesContents = File.ReadAllText(globalsFilePath);
        Ink.Compiler compiler = new Ink.Compiler(inkFilesContents);
        Story globalVariablesStory = compiler.Compile();

        // Initialize the dictionary
        variables = new Dictionary<string, Ink.Runtime.Object>();
        foreach (string name in globalVariablesStory.variablesState)
        {
            Ink.Runtime.Object value = globalVariablesStory.variablesState.GetVariableWithName(name);
            variables.Add(name, value);
            Debug.Log("Initialized global dialogue variable: " + name + " = " + value);
        }
    }

    public void StartListening(Story story)
    {
        // It's important that VariablesToStory is before assigning the listener
        VariablesToStory(story);
        story.variablesState.variableChangedEvent += VariableChanged;
    }

    public void StopListening(Story story)
    {
        story.variablesState.variableChangedEvent -= VariableChanged;
    }

    private void VariableChanged(string name, Ink.Runtime.Object value)
    {
        // Only maintain variables that were intialized from the globals ink file
        if (variables.ContainsKey(name))
        {
            variables.Remove(name);
            variables.Add(name, value);
        }

        DialogueManager manager = UnityEngine.Object.FindObjectOfType<DialogueManager>();
        if (manager != null)
        {
            manager.ShowPopUp("Morality changed: " + name + " = " + value);
        }
        else
        {
            Debug.LogWarning("DialogueManager not found in scene!");
        }
        //FindObjectOfType<DialogueManager>().ShowPopUp("Morality changed: " + name + " = " + value);
        Debug.Log("Morality has changed: " + name + " = " + value); // change this into the pop up text
    }

    private void VariablesToStory(Story story)
    {
        foreach(KeyValuePair<string, Ink.Runtime.Object> variable in variables)
        {
            story.variablesState.SetGlobal(variable.Key, variable.Value);
        }
    }
}
