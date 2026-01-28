using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;

namespace coolbeats.scripts.managerScripts
{
    public partial class PlayerManagement : managerNode
    {
        int port = 8080;
        string IP = "127.0.0.1";
        ENetMultiplayerPeer server;
        public Dictionary<long, player> players;
        public override void setup()
        {
            players = new Dictionary<long, player>();
            peer_connected(1);
            server = new ENetMultiplayerPeer();
            server.CreateServer(port);
            Multiplayer.MultiplayerPeer = server;
            Multiplayer.PeerConnected += peer_connected;
            Multiplayer.PeerDisconnected += peer_disconnected;
        }
        public void peer_connected(long id)
        {
            players[id] = new player(id);
        }
        public void peer_disconnected(long id)
        {
            players.Remove(id);
        }
    }
    public class player
    {
        public long ID;
        public int team;
        public Camera2D camera;
        public player(long ID)
        {
            team = mAccess.teamManager.teams.Count();
            mAccess.teamManager.teams = mAccess.teamManager.teams.Append(new team()).ToArray();
            camera = mAccess.entityManager.spawnEntity("playerCamera") as Camera2D;
            camera.MakeCurrent();
        }
    }
}