using PDollarGestureRecognizer;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class PointCloudRecognizer : AbstractRecognizer
{
    private List<Gesture> trainingSet = new List<Gesture>();
    private List<Point> points = new List<Point>();
    public override void ReadXMLFile()
    {
        FilePath = Application.streamingAssetsPath + "/Gestures/PointCloudXml";
        if (Directory.Exists(FilePath))
        {
            //Load gestures
            TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
            foreach (TextAsset gestureXml in gesturesXml)
                trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));
        }
    }

    protected override void RecognizeAndGetResults()
    {
        throw new NotImplementedException();
    }
}
