using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class screenCTRL : MonoBehaviour {

    public GameObject ctrl;
    public runShader2 shad;
    public float speed = .00001f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButton(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //if (Physics.Raycast(ray, out hit)) {
                ctrl.transform.position = ray.GetPoint(5);
            //}
            shad.LeftSpeed = speed;
        }
        else if (Input.GetMouseButton(1)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //if (Physics.Raycast(ray, out hit)) {
            ctrl.transform.position = ray.GetPoint(5);
            //}
            shad.LeftSpeed = -speed;
        }
        else
            shad.LeftSpeed = 0;
    }
}
