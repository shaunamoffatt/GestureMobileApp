/*/
 * Adapted From
 * https://www.devination.com/2013/06/unity-touch-input-tutorials.html
/*/
using UnityEngine;

public class TouchLogic : MonoBehaviour
{
    public static int currTouch = 0;
    private Ray ray;
    private RaycastHit rayHitInfo = new RaycastHit();

    void Update()
    {
        //is there a touch on screen?
        if (Input.touches.Length <= 0)
        {
            //if no touches then execute this code
        }
        else //if there is a touch
        {
            //loop through all the the touches on screen
            for (int i = 0; i < Input.touchCount; i++)
            {
                currTouch = i;
                Debug.Log(currTouch);
                //executes this code for current touch (i) on screen
               // if (this.guiTexture != null && (this.guiTexture.HitTest(Input.GetTouch(i).position)))
              //  {
                    //if current touch hits our guitexture, run this code
                    if (Input.GetTouch(i).phase == TouchPhase.Began)
                    {
                        this.SendMessage("OnTouchBegan");
                    }
                    if (Input.GetTouch(i).phase == TouchPhase.Ended)
                    { 
                        this.SendMessage("OnTouchEnded");
                    }
                    if (Input.GetTouch(i).phase == TouchPhase.Moved)
                    {
                        this.SendMessage("OnTouchMoved");
                    }
                    if (Input.GetTouch(i).phase == TouchPhase.Stationary)
                    {
                        this.SendMessage("OnTouchStayed");
                    }
               // }

                //outside so it doesn't require the touch to be over the guitexture
                ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);//creates ray from screen point position
                switch (Input.GetTouch(i).phase)
                {
                    case TouchPhase.Began:
                        this.SendMessage("OnTouchBeganAnyWhere");
                        if (Physics.Raycast(ray, out rayHitInfo))
                            rayHitInfo.transform.gameObject.SendMessage("OnTouchBegan3D");
                        break;
                    case TouchPhase.Ended:
                        this.SendMessage("OnTouchEndedAnywhere");
                        if (Physics.Raycast(ray, out rayHitInfo))
                            rayHitInfo.transform.gameObject.SendMessage("OnTouchEnded3D");
                        break;
                    case TouchPhase.Moved:
                        this.SendMessage("OnTouchMovedAnywhere");
                        if (Physics.Raycast(ray, out rayHitInfo))
                            rayHitInfo.transform.gameObject.SendMessage("OnTouchMoved3D");
                        break;
                    case TouchPhase.Stationary:
                        this.SendMessage("OnTouchStayedAnywhere");
                        if (Physics.Raycast(ray, out rayHitInfo))
                            rayHitInfo.transform.gameObject.SendMessage("OnTouchStayed3D");
                        break;
                }
            }
        }
    }
}