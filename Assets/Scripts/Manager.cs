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
	private string result;
	private List<UnityEngine.Object> blocks;
	private bool ENDOFGAME = false;
	private bool dataready = false;
	private bool closeconnect = false;
	private Thread receiveThread; //1
	private Thread sendThread; //1
	private UdpClient client;
	private UdpClient server;
    // Start is called before the first frame update
    void Start()
    {
		Application.targetFrameRate = 5;
        InitUDP()
        port += 4
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
			//Debug.Log("Distance"+ d.ToString());
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
    
    void getresults(){
		
		for (int i = 0; i < bot.Count; i++){
			if (i == 0){
				result += bot[i].getscore().ToString("0.##");
			}else{
				result += (";"+bot[i].getscore().ToString("0.##"));
			}
		}
	}
    
    void destroybots(){
		for (int i = 0; i < bot.Count; i++){
			while (bot[i].getnothread());
			GameObject.Destroy(bot[i].gameObject);
		}
	}
    
    void destroyblocks(){
		for (int i = 0; i< blocks.Count; i++){
			GameObject.Destroy(blocks[i]);
		}
	}
    
    
    
    void initbot()
    {
		bot = new List<Bot>();
		for (int i = 0; i < botcount; i++){
			Bot b  = Instantiate(prefab,new Vector3(0,0,-20),new Quaternion(0, 0, 0, 0)).GetComponent<Bot>();
			b.setportandaddress(port,"127.0.0.1");
			port += 4;
			bot.Add(b);
		}
		
	}
	
	private void InitUDP(){
		print ("UDP Initialized");
		receiveThread = new Thread (new ThreadStart(UDPstuff));
		sendThread = new Thread (new ThreadStart(UDPrecv));
		sendThread.IsBackground = true;
		receiveThread.IsBackground = true;
		receiveThread.Start();
		sendThread.Start();
	}
    
    
    private void UDPrecv(){
		server = new UdpClient();
		IPEndPoint localEp = new IPEndPoint(IPAddress.Parse(adress), port+2);
		server.Client.Bind(localEp);
		byte[] data;
		while (true){
			try{
				data = server.Receive(ref anyIP); 
				string text = Encoding.UTF8.GetString(data); 
				if (text == "CLOSECONNECT"){
					closeconnect = true;
					break;
				}
				if (text == "Shuffle"){
					ENDOFGAME = true;
					initblock();
				}
				if (text == "START"){
					destroybots();
					initbot();
					ENDOFGAME = false;
				}
				string[] text = text.Split(";");
				if (text[0]=="BOTCOUNT"){
					botcount = int.Parse(text[1]);
				}
				if (text[0] == "EOG"){
					getresults()
					ENDOFGAME = true;
					dataready = true;
					destroybots();
					
				}
				
				string[] text = text.Split(";");
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
		IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse(adress),port+1);
		byte[] startresult = System.Text.Encoding.UTF8.GetBytes("RESULTSTART")
		byte[] endresult = System.Text.Encoding.UTF8.GetBytes("RESULTEND");
		while (true){
			try{
				if (closeconnect){
					break;
				}
				if (dataready){
					client.Send(startresult,startresult.Length, anyIP);
					resultbyte = System.Text.Encoding.UTF8.GetBytes(result)
					while (true){
						if (resultbyte.Length < 8654){
							client.Send(resultbyte,picture.Length, anyIP);
							print("sending the rest!");
							
							break;
						}
						byte[] first = new byte[8654];
						Buffer.BlockCopy(resultbyte, 0, first, 0, first.Length);
						byte[] second = new byte[resultbyte.Length - first.Length];
						Buffer.BlockCopy(resultbyte, first.Length, second, 0, second.Length);
						client.Send(first,first.Length,anyIP);
						resultbyte = second;
					}
					client.Send(endresult,endresult.Length, anyIP);
				}
				
				
			}			
			catch(Exception e){
				print (e.ToString()); 
			}
		}
	}
	
}
