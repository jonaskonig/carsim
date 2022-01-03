using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
	public GameObject prefab;//holds bot prefab			y 238 -130 x -29 23
	public GameObject obsticalblue;
	public GameObject obsticalred;
	public int obsticalcount;
	private Bot bot;

	
    // Start is called before the first frame update
    void Start()
    {
        initblock();
        initcar();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void initblock(){
		for (int i = 0; i < obsticalcount/2; i++){
			Instantiate(obsticalblue,new Vector3(Random.Range(-29.0f, 23.0f), 3f, Random.Range(-11f, 76f)),new Quaternion(0, 0, 1, 0));
			Instantiate(obsticalred,new Vector3(Random.Range(-29.0f, 23.0f), 3f, Random.Range(-11f, 76f)),new Quaternion(0, 0, 1, 0));
		}
	}
    
    void initcar()
    {
		bot = Instantiate(prefab,new Vector3(0,0,-20),new Quaternion(0, 0, 0, 0)).GetComponent<Bot>();
		
	}
}
