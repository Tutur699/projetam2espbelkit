using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class LanDiscovery : MonoBehaviour
{
    [Header("Config découverte LAN")]
    [SerializeField] private int discoveryPort = 47777;
    [SerializeField] private int gamePort = 7777;
    [SerializeField] private string gameId = "PRISONNIER_LYON1"; // identifiant du jeu

    public class DiscoveredServer
    {
        public string Address;
        public int Port;
        public float LastSeenTime;
    }

    private readonly List<DiscoveredServer> _servers = new List<DiscoveredServer>();

    private UdpClient _broadcaster;
    private UdpClient _listener;

    private IPEndPoint _listenEndPoint;
    private float _broadcastInterval = 1f;
    private float _cleanupDelay = 5f;

    private float _nextBroadcastTime = 0f;

    public IReadOnlyList<DiscoveredServer> Servers => _servers;

    // ----------- HOST : broadcast régulier -----------
    public void StartBroadcasting()
    {
        if (_broadcaster != null) return;

        try
        {
            _broadcaster = new UdpClient();
            _broadcaster.EnableBroadcast = true;
            Debug.Log("[LAN] Broadcast démarré.");
        }
        catch (Exception e)
        {
            Debug.LogError("[LAN] Erreur StartBroadcasting : " + e);
        }
    }

    public void StopBroadcasting()
    {
        if (_broadcaster != null)
        {
            _broadcaster.Close();
            _broadcaster = null;
            Debug.Log("[LAN] Broadcast arrêté.");
        }
    }

    // ----------- CLIENT : écoute des serveurs -----------
    public void StartListening()
    {
        if (_listener != null) return;

        try
        {
            _listenEndPoint = new IPEndPoint(IPAddress.Any, discoveryPort);
            _listener = new UdpClient(_listenEndPoint);
            Debug.Log("[LAN] Écoute LAN démarrée sur le port " + discoveryPort);
        }
        catch (Exception e)
        {
            Debug.LogError("[LAN] Erreur StartListening : " + e);
        }
    }

    public void StopListening()
    {
        if (_listener != null)
        {
            _listener.Close();
            _listener = null;
            _servers.Clear();
            Debug.Log("[LAN] Écoute LAN arrêtée.");
        }
    }

    private void Update()
    {
        BroadcastIfNeeded();
        ReceiveAnnouncements();
        CleanupOldServers();
    }

    private void BroadcastIfNeeded()
    {
        if (_broadcaster == null) return;

        if (Time.time >= _nextBroadcastTime)
        {
            _nextBroadcastTime = Time.time + _broadcastInterval;

            try
            {
                string msg = $"{gameId}|{gamePort}";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                var endpoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);
                _broadcaster.Send(data, data.Length, endpoint);
            }
            catch (Exception e)
            {
                Debug.LogError("[LAN] Erreur Broadcast : " + e);
            }
        }
    }

    private void ReceiveAnnouncements()
    {
        if (_listener == null) return;

        try
        {
            while (_listener.Available > 0)
            {
                IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _listener.Receive(ref remote);
                string msg = Encoding.UTF8.GetString(data);

                string[] parts = msg.Split('|');
                if (parts.Length != 2) continue;
                if (parts[0] != gameId) continue;

                if (!int.TryParse(parts[1], out int port)) continue;

                string ip = remote.Address.ToString();
                AddOrUpdateServer(ip, port);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[LAN] Erreur ReceiveAnnouncements : " + e);
        }
    }

    private void AddOrUpdateServer(string ip, int port)
    {
        var server = _servers.Find(s => s.Address == ip && s.Port == port);
        if (server == null)
        {
            server = new DiscoveredServer
            {
                Address = ip,
                Port = port,
                LastSeenTime = Time.time
            };
            _servers.Add(server);
            Debug.Log($"[LAN] Serveur trouvé : {ip}:{port}");
        }
        else
        {
            server.LastSeenTime = Time.time;
        }
    }

    private void CleanupOldServers()
    {
        if (_servers.Count == 0) return;

        float now = Time.time;
        _servers.RemoveAll(s => now - s.LastSeenTime > _cleanupDelay);
    }

    private void OnDisable()
    {
        StopBroadcasting();
        StopListening();
    }

    public int GetGamePort() => gamePort;
}
