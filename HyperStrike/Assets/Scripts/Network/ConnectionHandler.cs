using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using Unity.Netcode;
using UnityEngine;

public class ConnectionHandler : NetworkBehaviour
{
    // used in ApprovalCheck. This is intended as a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
    const int k_MaxConnectPayload = 1024;

    /*
    public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var connectionData = request.Payload;
        var clientId = request.ClientNetworkId;
        if (connectionData.Length > k_MaxConnectPayload)
        {
            // If connectionData too high, deny immediately to avoid wasting time on the server. This is intended as
            // a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
            response.Approved = false;
            return;
        }

        var payload = System.Text.Encoding.UTF8.GetString(connectionData);
        var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload); // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html
        var gameReturnStatus = GetConnectStatus(connectionPayload);

        if (gameReturnStatus == ConnectStatus.Success)
        {
            SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId, connectionPayload.playerId,
                new SessionPlayerData(clientId, connectionPayload.playerName, new NetworkGuid(), 0, true));

            // connection approval will create a player object for you
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Position = Vector3.zero;
            response.Rotation = Quaternion.identity;
            return;
        }

        response.Approved = false;
        response.Reason = JsonUtility.ToJson(gameReturnStatus);
        if (m_LobbyServiceFacade.CurrentUnityLobby != null)
        {
            m_LobbyServiceFacade.RemovePlayerFromLobbyAsync(connectionPayload.playerId, m_LobbyServiceFacade.CurrentUnityLobby.Id);
        }
    }

    ConnectStatus GetConnectStatus(ConnectionPayload connectionPayload)
    {
        if (m_ConnectionManager.NetworkManager.ConnectedClientsIds.Count >= m_ConnectionManager.MaxConnectedPlayers)
        {
            return ConnectStatus.ServerFull;
        }

        if (connectionPayload.isDebug != Debug.isDebugBuild)
        {
            return ConnectStatus.IncompatibleBuildType;
        }

        return SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(connectionPayload.playerId) ?
            ConnectStatus.LoggedInAgain : ConnectStatus.Success;
    }
    */
}
