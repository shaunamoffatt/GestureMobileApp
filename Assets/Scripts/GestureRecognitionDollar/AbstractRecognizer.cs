using System;
using System.Collections.Generic;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;


public abstract class AbstractRecognizer
{
    protected string FilePath;
    protected string resultText = "";

    // Start is called before the first frame update
    public AbstractRecognizer()
    {
        ReadXMLFile();
    }

    public abstract void ReadXMLFile();

    // Get the comparison result and display
    protected abstract void RecognizeAndGetResults();

    public string GetResultText()
    {
        return resultText;
    }
}
