using System;
using System.Collections.Generic;
using TMPro;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;
using WobbrockLib;
using WobbrockLib.Extensions;

public class GestureRecognitionSwipe : MonoBehaviour
{
    [SerializeField] public ScreenTransformGesture transformGesture;
    [SerializeField] HandController hand;// takes up less memory to pass in through editor

    private bool isDown = false;
    private float MinNoPoints = 5;

    UniStrokeRecognizer uniStroke;
    // used for the unistroke
    private List<TimePointF> points = new List<TimePointF>(256);
    // The current list of allowed shapes that make up the spells
    // star- fire
    // circle- heal
    // pigtail- birth
    // v - electric (Could make my own shapes with eventually..)
    Dictionary<string, string> spells;
    //Result text was for debug purposes
    public TMP_Text ResultText;

    private void Awake()
    {
        uniStroke = new UniStrokeRecognizer();
        spells = new Dictionary<string, string>();
        spells.Add("fire", "star");
        spells.Add("heal", "circle");
        spells.Add("birth", "pigtail");
        spells.Add("electric", "v");
    }

    private void OnEnable()
    {
        Debug.Log("Enabled TransformGesture");
        transformGesture.TransformStarted += TransformGesture_TransformStarted;
        transformGesture.Transformed += TransformGesture_Transformed;
        transformGesture.TransformCompleted += TransformGesture_TransformCompleted;
    }

    private void OnDisable()
    {
        transformGesture.TransformStarted -= TransformGesture_TransformStarted;
        transformGesture.Transformed -= TransformGesture_Transformed;
        transformGesture.TransformCompleted -= TransformGesture_TransformCompleted;
    }


    // Finger starts to slide
    protected void TransformGesture_TransformStarted(object sender, EventArgs e)
    {
        Debug.Log("TransformGesture Started......");
        float x = transformGesture.ScreenPosition.x;
        float y = transformGesture.ScreenPosition.y;
        isDown = true;
        points.Clear();
        points.Add(new TimePointF(y, x, TimeEx.NowMs));
        hand.StartPointing(transformGesture.ScreenPosition);

    }

    /// When the finger sliding
    protected void TransformGesture_Transformed(object sender, EventArgs e)
    {
        if (isDown)
        {
            float x = transformGesture.ScreenPosition.x;
            float y = transformGesture.ScreenPosition.y;

            points.Add(new TimePointF(y, x, TimeEx.NowMs));

            hand.PointMove(transformGesture.ScreenPosition);
        }
    }

    /// When the finger sliding ends (lifted up)
    protected void TransformGesture_TransformCompleted(object sender, EventArgs e)
    {
        Debug.Log("TransformGesture Complete");
        if (isDown)
        {
            isDown = false;
            if (points.Count >= MinNoPoints)
            {
                uniStroke.RecognizeShape(points);
                ResultText.text = uniStroke.GetResultText();//for debugging
            }

            // If the score is greater than 5 we have got a proper result
            if (uniStroke.getScore() >= 2.5)
            {
                string name = uniStroke.getActionName();
                LoadSpell(name);
                //Turn of Magic Screen
                GetComponent<GestureActivationController>().ToggleActiveGestures();
            }
            else
            {
                //Play error sound
            }

            hand.StopPointing();
        }
    }

    private void LoadSpell(string resultName)
    {
        if (TryLoadSpell("fire", resultName))
        {
            hand.LoadFireSpell();
            return;
        }
        else if (TryLoadSpell("heal", resultName))
        {
            hand.LoadHealSpell();
            return;
        }
        else if (TryLoadSpell("electric", resultName))
        {
            hand.LoadElectricSpell();
            return;
        }
        else if (TryLoadSpell("birth", resultName))
        {
            hand.LoadBirthSpell();
            return;
        }
    }

    private bool TryLoadSpell(string spellName, string resultName)
    {
        string actionShape;
        if (spells.TryGetValue(spellName, out actionShape))
        {
            // TODO might not work so well with differently named spells..
            // if adding more-  need to consider re-factoring
            if (resultName.Contains(actionShape))
            {
                return true;
            }
        }
        return false;
    }
}
