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
	public GameObject prefab;
	public float speed;//Speed Multiplier
    public float rotation;//Rotation multiplier
	public float steerangle;
	public float acceleration;
	public int resWidth;
	public int resHeight;
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
	private Thread sendThread:
	private UdpClient client; //2
	private UdpClient server; 
	private bool gamegoson;
	private bool recvstop;
	private bool sendstop;
	public Camera camera;
    void FixedUpdate()//FixedUpdate is called at a constant interval
    {
		transform.Rotate(0, steerangle * rotation, 0, Space.World);//controls the cars movement
        transform.position += this.transform.right * acceleration * speed;//controls the cars turning
	}
    
    // Start is called before the first frame update
    void Start()
    {
		gamegoson = true;
		setupcam();
        starttime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        InitUDP();
    }

    // Update is called once per frame
    void Update()
    {
		picture = CamCapture();
		newpic = true;
        
    }
    public void setportandaddress(int aport, string aadress){
		port = aport;
		adress = aadress;
	}
	
	private void setupcam(){
		GameObject temp  = Instantiate(prefab,transform.position,new Quaternion(0, 0, 0, 0));
		CameraFollow script = temp.GetComponent<CameraFollow>();
		script.settransform(transform);
		camera = temp.GetComponent<Camera>();
	}
	
	public double getscore(){
		gamegoson = false;
		float dist = Vector3.Distance(new Vector3(0,0,-20), transform.position);
		score = dist/(starttime- new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds());
		return score;
	}
	
	public bool getnothread(){
		return (!recvstop && !sendstop)
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
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        return screenShot.EncodeToPNG();
		
    }
    
    private void InitUDP(){
		print ("UDP Initialized");
		receiveThread = new Thread (new ThreadStart(UPDrecv));
		receiveThread.IsBackground = true;
		receiveThread.Start();
		sendThread = new Thread (new ThreadStart(UDPstuff));
		sendThread.IsBackground = true;
		sThread.Start();
	}
	
	private void UPDrecv(){
		server = new UdpClient();
		IPEndPoint localEp = new IPEndPoint(IPAddress.Parse(adress), port+2);
		server.Client.Bind(localEp);
		byte[] data;
		while (true){
			try{
				if(!gamegoson){
					recvstop = false;
					break;
				}
				data = server.Receive(ref anyIP); 
				string text = Encoding.UTF8.GetString(data); 
				if (text == "ENDREG"){
					recvstop = false;
					break;
				}

				acc = Convert.ToDouble(text[0])
				steer = Convert.ToDouble(text[1])
			} 
			catch(Exception e){
				print (e.ToString());
			}
		}
		
	}
    
    
    
    
	
	private void UDPstuff(){
		client = new UdpClient(port);
		byte[] startcode = System.Text.Encoding.UTF8.GetBytes("LosGehtsKleinerHase");
		byte[] endcode = System.Text.Encoding.UTF8.GetBytes("ZuEndekleinerHase");
		print("Port:"+ port.ToString());
		int port2 = port+1;
		
		IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse(adress),port2);
		while (true){
			try{
				if (newpic){
					int len = picture.Length;
					print (len.ToString());
					client.Send(startcode,startcode.Length, anyIP);
					while (true){
						if (picture.Length < 8654){
							client.Send(picture,picture.Length, anyIP);
							print("sending the rest!");
							
							break;
						}
						byte[] first = new byte[8654];
						Buffer.BlockCopy(picture, 0, first, 0, first.Length);
						byte[] second = new byte[picture.Length - first.Length];
						Buffer.BlockCopy(picture, first.Length, second, 0, second.Length);
						client.Send(first,first.Length,anyIP);
						picture = second;
						print(picture.Length.ToString());
					}
					client.Send(endcode,endcode.Length, anyIP);
					newpic = false; 
				}
				if (!gamegoson){
					var bytes = System.Text.Encoding.UTF8.GetBytes("ENDOFGAME");
					client.Send(bytes, bytes.Length);
					sendstop = false;
					break;
				}
				
			} 
			catch(Exception e){
				print (e.ToString()); 
			}
		}
		
	}
				
	
	
	
}
