using System;
using System.Collections.Generic;
using UnityEngine;
using WobbrockLib;
using Recognizer.Dollar;
using System.IO;

//Adapted from : https://www.programmersought.com/article/92405795711/
public class UniStrokeRecognizer : AbstractRecognizer
{
    //Gesture recognition
    Recognizer.Dollar.Recognizer recognizer = new Recognizer.Dollar.Recognizer();
    private List<TimePointF> points = new List<TimePointF>(256);
    double Score { get; set; }
    string Name { get; set; }
    // Get all the file paths of the specified suffix under the path
    public string[] GetFiles(string path, string extension)
    {
        List<string> files = new List<string>();
        //Get all files in the specified folder
        string[] paths = Directory.GetFiles(path);
        foreach (var item in paths)
        {
            //Get the file extension
            string _extension = Path.GetExtension(item).ToLower();
            if (_extension == extension)
            {
                //Add to the list
                files.Add(item);
            }
        }
        return files.ToArray();
    }

    public override void ReadXMLFile()
    {
        FilePath = Directory.GetCurrentDirectory() + "\\Assets\\Gestures\\OneDollarXml";
        if (Directory.Exists(FilePath))
        {
            string[] files = GetFiles(FilePath, ".xml");
            for (int i = 0; i < files.Length; i++)
            {
                string name = files[i];
                recognizer.LoadGesture(name);
            }

            Debug.Log("File read" + files.Length);
        }
    }

    public void RecognizeShape(List<TimePointF> points)
    {
        this.points = points;
        RecognizeAndGetResults();
    }

    private double score = 0;
    private string name = "";

    protected override void RecognizeAndGetResults()
    {
        //NBestList result = recognizer.Recognize(points, false);
        NBestList result = recognizer.Recognize(points, true);
        resultText = string.Format("{0}: {1} ({2}px,{3}{4}{5})",
            result.Name.Split('/')[result.Name.Split('/').Length - 1],
            Math.Round(result.Score, 2),
            Math.Round(result.Distance, 2),
            Math.Round(result.Angle, 2), (char)176, points.Count);

        Score = Math.Round(result.Score, 2);
        Name = result.Name.Split('/')[result.Name.Split('/').Length - 1];
        this.points = new List<TimePointF>(256);
    }

    public double getScore()
    {
        return Score;
    }

    public string getActionName()
    {
        return Name;
    }
}


