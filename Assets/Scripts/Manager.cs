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
	private List<UnityEngine.Object> blocks = new List<UnityEngine.Object>();
	private string result;
	private bool ENDOFGAME = false;
	private bool dataready = false;
	private bool closeconnect = false;
	private bool shuffle = false;
	private bool start = false;
	private Thread receiveThread; //1
	private Thread sendThread; //1
	private UdpClient client;
	private UdpClient server;
	private int botport;
    // Start is called before the first frame update
    void Start()
    {
		Application.targetFrameRate = 5;
        InitUDP();

    }

	void FixedUpdate()//FixedUpdate is called at a constant interval
    {
		
	}
    // Update is called once per frame
    void Update()
    {
        if (start){
			botport = port+4;
			
			/*try{
			   destroybots();
			}catch(Exception e){
				print (e.ToString()); 
			}*/
			initbot();
			ENDOFGAME = false;
			start = false;
		}
		if (shuffle){
			if (blocks.Count >0 ){
				destroyblocks();
			}
			initblock();
			shuffle = false;
		}
		if (ENDOFGAME){
			print("ENDOFGAME");
			getresults();
			dataready = true;
			destroybots();
			ENDOFGAME = false;
		}
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
		
		float[] randoms = getrandom(obsticalcount);
		for (int i = 0; i < obsticalcount*2; i= i+4){
			blocks.Add(Instantiate(obsticalblue,new Vector3(randoms[i], 3f, randoms[i+1]),new Quaternion(0, 0, 1, 0)));
			blocks.Add(Instantiate(obsticalred,new Vector3(randoms[i+2], 3f, randoms[i+3]),new Quaternion(0, 0, 1, 0)));
		}
	}
    
    void getresults(){
		for (int i = 0; i < bot.Count; i++){
			if (i == 0){
				result += bot[i].getscore().ToString("0.#######");
			}else{
				result += (";"+bot[i].getscore().ToString("0.#######"));
				
			}
		}
	}
    
    void destroybots(){
		for (int i = 0; i < bot.Count; i++){
			bool temp  = bot[i].getnothread();
			while (!temp){
				
				temp  = bot[i].getnothread();
			}
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
			b.setportandaddress(botport,"127.0.0.1");
			botport += 4;
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
		server = new UdpClient(port+2);
		IPEndPoint localEp = new IPEndPoint(IPAddress.Parse(adress), 0);
		//server.Client.Bind(localEp);
		byte[] data;
		while (true){
			try{
				data = server.Receive(ref localEp); 
				string text = Encoding.UTF8.GetString(data); 
				print(text);
				if (text == "CLOSECONNECT"){
					closeconnect = true;
					break;
				}
				if (text == "SHUFFLE"){
					shuffle = true;
					ENDOFGAME = true;
					continue;
					
				}
				if (text == "START"){
					start = true;
					continue;
				}
				if (text == "GETRESULT"){
					print("sendingagain");
					dataready = true;
					continue;
				}
				string[] texts = text.Split(";");
				if (texts[0]=="BOTCOUNT"){
					botcount = int.Parse(texts[1]);
					continue;
				}
				if (texts[0] == "EOG"){
					ENDOFGAME = true;
					continue;
					
				}
				
			
			} 
			catch(Exception e){
				print (e.ToString());
			}
		}
		
	}
	
	private void UDPstuff(){
		client = new UdpClient(port);
		IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse(adress),port+3);
		byte[] startresult = System.Text.Encoding.UTF8.GetBytes("RESULTSTART");
		byte[] endresult = System.Text.Encoding.UTF8.GetBytes("RESULTEND");
		while (true){
			try{
				if (closeconnect){
					break;
				}
				if (dataready){
					print(result);
					
					byte[] resultbyte = System.Text.Encoding.UTF8.GetBytes(result);
					client.Send(resultbyte,resultbyte.Length, anyIP);
					dataready = false;
					/*while (true){
						if (resultbyte.Length < 8654){
							client.Send(resultbyte,resultbyte.Length, anyIP);
							print("sending the rest!");
							dataready = false;
							break;
						}
						byte[] first = new byte[8654];
						Buffer.BlockCopy(resultbyte, 0, first, 0, first.Length);
						byte[] second = new byte[resultbyte.Length - first.Length];
						Buffer.BlockCopy(resultbyte, first.Length, second, 0, second.Length);
						client.Send(first,first.Length,anyIP);
						resultbyte = second;
					}
					client.Send(endresult,endresult.Length, anyIP);*/
				}
				
				
			}			
			catch(Exception e){
				print (e.ToString()); 
			}
		}
	}
	
}
