using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTheWorld : MonoBehaviour {

    public ON_MouseInteraction mouse;
    public GameObject trail;
	public AudioSource aud;
    GameObject Trail;
    public GameObject lazor;
    public GameObject[] sources;
    GameObject source;
    GameObject privateLazor;
    Vector3 init;
    public float laserWidth;
    public float particleAmount;
    public float trailWidth;
    public float trailTime;
    public float fadeSpeed;
    Vector3 prevPosition;
    float counter;
    float avgDistance = 1;
    float prevScale = 0;

    // Use this for initialization
    void Start () {
		privateLazor = lazor;// = Instantiate(lazor);
		Trail = trail;//Instantiate(trail);
		if(Trail.GetComponent<AudioSource>()!=null)
			aud = Trail.GetComponent<AudioSource> ();
        init = new Vector3(0, -1e6f, 0);
    }
	
	// Update is called once per frame
	void Update () {
        if (ON_MouseInteraction.beenHit) {
            FindClosestSource();
            //Debug.Log(mouse.hitObject);
            if (source != null && source.activeInHierarchy) {
                if (Vector3.Distance(source.transform.position, mouse.hitObject.transform.position) > 1 &&
                    mouse.hitObject.GetComponent<EW_DontLaserMe>() == null) {
                    counter = Mathf.Min(1, Mathf.Max(0, ((avgDistance - .1f))));
                    privateLazor.transform.position = Vector3.Lerp(mouse.hitPosition, source.transform.position, .5f);
                    privateLazor.transform.LookAt(source.transform.position);
                    float scale = Vector3.Distance(source.transform.position, mouse.hitPosition);
                    privateLazor.transform.localScale = new Vector3(counter * laserWidth, counter * laserWidth, prevScale);
                    Trail.transform.position = mouse.hitPosition;
                    Trail.GetComponent<TrailRenderer>().widthMultiplier = counter * trailWidth;
                    Trail.GetComponent<TrailRenderer>().time = counter * trailTime;
                    Trail.GetComponent<ParticleSystem>().emissionRate = counter * particleAmount;
                    prevScale = scale;
                    aud.volume = counter;
                }
            }
        }
        else if (counter > 0) {
            counter -= Time.deltaTime * fadeSpeed ;
            privateLazor.transform.localScale = new Vector3(counter * laserWidth, counter * laserWidth, prevScale);
            Trail.GetComponent<TrailRenderer>().widthMultiplier = counter * trailWidth;
            Trail.GetComponent<TrailRenderer>().time = counter * trailTime ;
            Trail.GetComponent<ParticleSystem>().emissionRate = counter * particleAmount;
			aud.volume = counter;
        }
        else {
            privateLazor.transform.position = init;
            prevScale = 0;
			aud.volume = 0;
        }

      
//        avgDistance *= 10;
        avgDistance = Vector3.Distance(mouse.hitPosition, prevPosition);
//        avgDistance /= 11;
        prevPosition = mouse.hitPosition;

    }
    void FindClosestSource() {

        float dist = 1e6f;
        int which = 0;
        for (int i = 0; i < sources.Length; i++) {
            if (dist > Vector3.Distance(sources[i].transform.position, mouse.hitPosition)) {
                dist = Vector3.Distance(sources[i].transform.position, mouse.hitPosition);
                which = i;
            }
        }
        if(which<sources.Length-1)
            source = sources[which];
    }

	public void AddToSource(GameObject g){
		GameObject[] newSource = new GameObject[sources.Length+1];
		for (int i = 0; i < sources.Length; i++) {
			newSource [i] = sources [i];
		}
		newSource [newSource.Length - 1] = g;
		sources = newSource;
	}
}
