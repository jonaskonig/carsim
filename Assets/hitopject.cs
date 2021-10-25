using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class hitopject : MonoBehaviour
{
	[SerializeField] private UnityEngine.UI.Text text;
	[SerializeField] private Transform car;
    
    private void OnTriggerEnter(Collider other)
    {
		int b = int.Parse(text.text);
	    text.text = other.tag ;
	    Debug.Log(other.tag);
	    if (other.tag == "red" && !(isleft(other.transform.position))){
			
		   b++;
		}else if(other.tag == "blue" && isleft(other.transform.position)){
			b++;
		}else{
			Debug.Log("Tag Problem:"+other.tag);
		}
		text.text = b.ToString();
	    
    }
    
    // returns true if the car is left of object, false if right
    private bool isleft(Vector3 position){
		float carrotation = car.rotation.y;
		if (carrotation< 0){
			carrotation = carrotation+360;
		}
		Vector3 diff = position-car.position;
		Debug.Log("Rotation: "+carrotation.ToString());
		Debug.Log("diff Vektor: x"+ diff.x.ToString()+" z: " +diff.y.ToString());
		if((carrotation > 315) || (carrotation > 0 && carrotation < 45) ){
			return (diff.x > 0);
		}else if (carrotation > 45 && carrotation < 135 ){
			return (diff.z > 0);
		}else if (carrotation > 135 && carrotation < 225 ){
			return (diff.x < 0);
		}else if (carrotation > 225 && carrotation < 315 ){
			return (diff.z < 0);
		}
		return false;
		
	}

}
