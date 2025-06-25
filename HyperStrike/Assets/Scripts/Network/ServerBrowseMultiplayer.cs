using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ServerBrowseMultiplayer : MonoBehaviour
{
    private enum ServerStatus
    {
        AVAILABLE,
        ONLINE,
        ALLOCATED
    }

    [Serializable]
    public class ListServers
    {
        public Server[] serverList;
    }

    [Serializable]
    public class MachineSpecs
    {
        public string contractEndDate;
        public string contractStartDate;
        public int cpuCores;
        public string cpuDetail;
        public string cpuName;
        public string cpuShortname;
        public int cpuSpeed;
        public int memory;
    }

    [Serializable]
    public class Server
    {
        

        public int buildConfigurationID;
        public string buildConfigurationName;
        public int buildID;
        public string buildName;
        public int cpuLimit;
        public bool deleted;
        public string fleetID;
        public string fleetName;
        public string hardwareType;
        public int holdExpiresAt;
        public int id;
        public string ip;
        public int locationID;
        public string locationName;
        public int machineID;
        public string machineName;
        public MachineSpecs machineSpec;
        public int port;
        public string regionID;
        public string regionName;
        public string status;
    }

    [SerializeField] private MainMenuUI mainMenuUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
#if ONLINE_SERVER              
        string keyId = "ee30d031-19be-4790-86b2-f18788d1c25f";
        string keySecret = "peWwpcPyFGVrxsXoXRXuEoHZ-q2laTtV";
        byte[] keyByteArray = Encoding.UTF8.GetBytes(keyId + ":" + keySecret);
        string keyBasee64 = Convert.ToBase64String(keyByteArray);

        string projectId = "d8382658-c3db-42e2-9192-72dd7334ff45";
        string environmentId = "cf7b0fc6-c314-4154-90f4-1f1ccbcfb827";

        string url = $"https://services.api.unity.com/multiplay/servers/v1/projects/{projectId}/environments/{environmentId}/servers";

        WebRequests.Get(url,
            (UnityWebRequest unityWebRequest) =>
            {
                unityWebRequest.SetRequestHeader("Authorization", "Basic " + keyBasee64);
            },

            (string error) =>
            {
                Debug.Log("Error: " + error);
            },

            (string json) =>
            {
                Debug.Log("Success: " + json);
                ListServers listServers = JsonUtility.FromJson<ListServers>("{\"serverList\":" + json + "}");
                foreach (var server in listServers.serverList)
                {
                    if (server.status == ServerStatus.ONLINE.ToString() || server.status == ServerStatus.ALLOCATED.ToString())
                    {
                        mainMenuUI.SetOnlineServerParams(server.ip, (ushort)server.port);
                        Debug.Log("IP: " + server.ip);
                        Debug.Log("PORT: " + server.port);
                        break;
                    }
                }
            }
        );
#endif
    }



}
