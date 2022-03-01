using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Manager : MonoBehaviour
{
	public GameObject prefab;//holds bot prefab			y 238 -130 x -29 23
	public GameObject obsticalblue;
	public GameObject obsticalred;
	public int obsticalcount;
	public int botcount;
	public int port;
	public string adress;
	private int startport;
	public float maxx;
	public float minx;
	public float maxy;
	public float miny;
	private List<Bot> bot;
	private List<UnityEngine.Object> blocks;
	private Thread receiveThread; //1
	private UdpClient client;
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
    
    float[] createxy(){
		float[] k = {UnityEngine.Random.Range(minx, maxx),UnityEngine.Random.Range(miny, maxy)};
		Debug.Log("x:"+k[0].ToString()+ " y:"+ k[1].ToString());
		return k;
	}
    
    bool testxy(float[] temp, float[] check, int counter){
		for (int i = 0; i< counter; i=i+2){
			double d = System.Math.Sqrt(System.Math.Pow(temp[0]-check[i],2)+System.Math.Pow(temp[1]-check[i+1],2));
			Debug.Log("Distance"+ d.ToString());
			if (d<3){
				return false;
			}
		}
		return true;
	}
    
    float[] getrandom(int count){
		float[] random = new float[count*2];
		float[] temp = new float[2];
		for (int i = 0; i < count*2; i=i+2){
			temp = createxy();
			while(!testxy(temp,random, i)){
				
				temp = createxy();
			}
			random[i]= temp[0];
			random[i+1] = temp[1];
				
		} 
		return random;
		
	}
    
    void initblock(){
		blocks = new List<UnityEngine.Object>();
		float[] randoms = getrandom(obsticalcount);
		for (int i = 0; i < obsticalcount*2; i= i+4){
			blocks.Add(Instantiate(obsticalblue,new Vector3(randoms[i], 3f, randoms[i+1]),new Quaternion(0, 0, 1, 0)));
			blocks.Add(Instantiate(obsticalred,new Vector3(randoms[i+2], 3f, randoms[i+3]),new Quaternion(0, 0, 1, 0)));
		}
	}
    
    void destroybots(){
		for (int i = 0; i < bot.Count; i++){
			GameObject.Destroy(bot[i].gameObject);
		}
	}
    
    void destroyblocks(){
		for (int i = 0; i< blocks.Count; i++){
			GameObject.Destroy(blocks[i]);
		}
	}
    
    
    
    void initcar()
    {
		bot = new List<Bot>();
		for (int i = 0; i < botcount; i++){
			Bot b  = Instantiate(prefab,new Vector3(0,0,-20),new Quaternion(0, 0, 0, 0)).GetComponent<Bot>();
			bot.Add(b);
		}
		
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
					IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse(adress), port);	
					data = client.Receive(ref anyIP);
					string text = Encoding.UTF8.GetString(data); 
					if(text == "Start"){
						destroybots();
						initcar();
						data = Encoding.UTF8.GetBytes("Started");
						client.Send(data, data.Length);
					}
					if(text == "shuffle"){
						destroyblocks();
						initblock();
					    data = Encoding.UTF8.GetBytes("shuffled");
						client.Send(data, data.Length);
					}
				
				
			} 
			catch(Exception e){
				print (e.ToString()); 
			}
		}
		
	}
	
}
