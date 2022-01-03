using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Bot : MonoBehaviour
{
	[SerializeField] public int count;
	public float speed;//Speed Multiplier
    public float rotation;//Rotation multiplier
	public float steerangle;
	public float acceleration;
	
	private long starttime;
	private long duration;
    void FixedUpdate()//FixedUpdate is called at a constant interval
    {
		transform.Rotate(0, steerangle * rotation, 0, Space.World);//controls the cars movement
        transform.position += this.transform.right * acceleration * speed;//controls the cars turning
	}
    
    // Start is called before the first frame update
    void Start()
    {
        var starttime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public long Getduration(){
		return duration;
	}

    private void OnTriggerEnter(Collider other)
    {
	    if (other.tag == "red" && !(isleft(other.transform.position))){
			
		   count++;
		}else if(other.tag == "blue" && isleft(other.transform.position)){
			count++;
		}else if (other.tag == "finish"){
			duration =  (new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds())-starttime;
		}else{
			Debug.Log("Tag Problem:"+other.tag);
		}
		//text.text = b.ToString();
	    
    }
    
    private bool isleft(Vector3 position){
		Vector3 Dir = position - transform.position;
		Dir = Quaternion.Inverse(transform.rotation) * Dir;
		return (Dir.x>0);
	}
	
	
}
