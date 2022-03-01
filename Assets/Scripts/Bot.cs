using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
public class Bot : MonoBehaviour
{
	[SerializeField] public int count;
	public float speed;//Speed Multiplier
    public float rotation;//Rotation multiplier
	public float steerangle;
	public float acceleration;
	//public Camera camera;
	private long starttime;
	private double acc;
	private double steer;
	private string adress;
	private double score;
	private int port;
	private bool newpic;
	private byte[] picture;
	private long duration;
	private Thread receiveThread; //1
	private UdpClient client; //2
	private bool gamegoson;
	private Camera camera;
    void FixedUpdate()//FixedUpdate is called at a constant interval
    {
		transform.Rotate(0, steerangle * rotation, 0, Space.World);//controls the cars movement
        transform.position += this.transform.right * acceleration * speed;//controls the cars turning
	}
    
    // Start is called before the first frame update
    void Start()
    {
		gamegoson = true;
		camera = GetComponent<Camera>();
        starttime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        InitUDP();
    }

    // Update is called once per frame
    void Update()
    {
		if (!gamegoson){
			float dist = Vector3.Distance(new Vector3(0,0,-20), transform.position);
			score = dist/duration;
		}
		picture = CamCapture();
		newpic = true;
        
    }
    public Bot(int aport, string aadress){
		port = aport;
		adress = aadress;
		
	}
	
	public double getscore(){
		return score;
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
			gamegoson = false;
		}else{
			Debug.Log("Tag Problem:"+other.tag);
		}	    
    }
    
    private bool isleft(Vector3 position){
		Vector3 Dir = position - transform.position;
		Dir = Quaternion.Inverse(transform.rotation) * Dir;
		return (Dir.x>0);
	}
	
	private byte[] CamCapture()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;
        camera.Render();
        Texture2D Image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;
        var Bytes = Image.EncodeToPNG();
        Destroy(Image);
        return Bytes;
		
    }
    
    private void InitUDP(){
		print ("UDP Initialized");
		receiveThread = new Thread (new ThreadStart(UDPstuff));
		receiveThread.IsBackground = true;
		receiveThread.Start();
	}
    
	
	private void UDPstuff(){
		client = new UdpClient (port);
		byte[] data;
		while (true){
			try{
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse(adress), port);
				data = client.Receive(ref anyIP); 
				string text = Encoding.UTF8.GetString(data); 
				client.Send(data, data.Length);
				break;
			} 
			catch(Exception e){
				print (e.ToString());
			}
		}
		while (true){
			try{
				
				if (newpic){
					IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse(adress), port);
					client.Send(picture, picture.Length);
					data = client.Receive(ref anyIP);
					acc = Convert.ToDouble(Encoding.UTF8.GetString(data));
					data = client.Receive(ref anyIP);
					steer = Convert.ToDouble(Encoding.UTF8.GetString(data));
					newpic = false; 
				}
				if (gamegoson){
					IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse(adress), port);
					var bytes = System.Text.Encoding.UTF8.GetBytes("ENDOFGAME");
					client.Send(bytes, bytes.Length);
					break;
				}
				
			} 
			catch(Exception e){
				print (e.ToString()); 
			}
		}
		
	}
				
	
	
	
}
