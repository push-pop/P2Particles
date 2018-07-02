using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeScreenshot : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    int fileNum = 0;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.S))
        {
            ScreenCapture.CaptureScreenshot(string.Format("Screenshot_{0}.png", fileNum), 4);
            fileNum++;
        }
    }
}
