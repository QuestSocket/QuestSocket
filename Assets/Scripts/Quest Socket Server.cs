using UnityEngine;
using UnityEngine.InputSystem.XR;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections;

public class QuestSocketServer : MonoBehaviour
{
    private WebSocketServer wssv;
    public static QuestSocketServer Instance { get; private set; }
    
    public TrackedPoseDriver rightHandTrackedPoseDriver; 
    public TrackedPoseDriver leftHandTrackedPoseDriver;  
    public TrackedPoseDriver headTrackedPoseDriver;

    [Range(10f, 120f)]
    public float updateRate = 60f; // Updates per second

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        wssv = new WebSocketServer(8080);
        wssv.AddWebSocketService<QuestSocketBehavior>("/");
        wssv.Start();
        Debug.Log("WebSocket server started on ws://localhost:8080/");
        
        StartCoroutine(BroadcastTrackingData());
    }

    void OnDestroy()
    {
        Instance = null;
        if (wssv != null)
        {
            wssv.Stop();
            Debug.Log("WebSocket server stopped.");
        }
    }

    private IEnumerator BroadcastTrackingData()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / updateRate);
            
            if (wssv != null && wssv.IsListening)
            {
                string trackingData = GetTrackingDataJson();
                wssv.WebSocketServices["/"].Sessions.Broadcast(trackingData);
            }
        }
    }

    private string GetTrackingDataJson()
    {
        TrackingData data = new TrackingData();
        
        if (rightHandTrackedPoseDriver != null)
        {
            Transform t = rightHandTrackedPoseDriver.transform;
            data.rightHand = new PoseData(t.position, t.rotation);
        }
        
        if (leftHandTrackedPoseDriver != null)
        {
            Transform t = leftHandTrackedPoseDriver.transform;
            data.leftHand = new PoseData(t.position, t.rotation);
        }
        
        if (headTrackedPoseDriver != null)
        {
            Transform t = headTrackedPoseDriver.transform;
            data.head = new PoseData(t.position, t.rotation);
        }
        
        return JsonUtility.ToJson(data);
    }
    
    [System.Serializable]
    public class TrackingData
    {
        public PoseData rightHand;
        public PoseData leftHand;
        public PoseData head;
    }
    
    [System.Serializable]
    public class PoseData
    {
        public Vector3Data position;
        public QuaternionData rotation;
        
        public PoseData(Vector3 pos, Quaternion rot)
        {
            position = new Vector3Data(pos);
            rotation = new QuaternionData(rot);
        }
    }
    
    [System.Serializable]
    public class Vector3Data
    {
        public float x, y, z;
        
        public Vector3Data(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }
    }
    
    [System.Serializable]
    public class QuaternionData
    {
        public float x, y, z, w;
        
        public QuaternionData(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }
    }
    
    private class QuestSocketBehavior : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            Send("Connected to Quest Socket Server");
        }
        
        protected override void OnMessage(MessageEventArgs e)
        {
        }
    }
}
