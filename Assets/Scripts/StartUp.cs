using Grpc.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Everett.Ebstorf;
using System;

public class StartUp : MonoBehaviour {

    public ulong WorldSeed;
    private Everett.Ebstorf.WorldStreamer.WorldStreamerClient Client;
    private Channel channel;
    private WorldRequest WorldRequest;
    public VisualizeWorld WorldVisualizer;

    public float WorldScaleFactor = 10f;
    public Boolean runAsyncTasks = true;

    // Use this for initialization
    void Start () {
        channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
        Client = new Everett.Ebstorf.WorldStreamer.WorldStreamerClient(channel);

        Debug.Log("Requesting world over GRPC");
        WorldRequest = new WorldRequest { WorldId = WorldSeed };
        var worldGenerationResult = Client.GenerateWorld(WorldRequest);
        Debug.Log(worldGenerationResult.Status);

        if (worldGenerationResult.Status == WorldGenerationStatus.Failed)
        {
            // Uh oh
            return;
        }
        RequestAllNodeCartesianLocations();

        /*
        RequestCellData();

        for(int i = 0; i < 100; i++) {
            var point = GenerateRandomSpherePoint();
            WorldVisualizer.AddNodeVertex(i.ToString(), point);
            //Debug.Log(point);
        }*/
    }

    private async void RequestAllNodeCartesianLocations()
    {
        runAsyncTasks = true;
        var nodeStream = Client.GetCartesianWorld(WorldRequest);
        while (await nodeStream.ResponseStream.MoveNext() && runAsyncTasks)
        {
            CartesianStructure cartesianStructure = nodeStream.ResponseStream.Current;
            WorldVisualizer.AddNodeVertex(cartesianStructure.Id, new Vector3((float)cartesianStructure.X* WorldScaleFactor, (float)cartesianStructure.Y* WorldScaleFactor, (float)cartesianStructure.Z* WorldScaleFactor));
        }
        var cellStream = Client.GetWorldCells(WorldRequest);
        while (await cellStream.ResponseStream.MoveNext() && runAsyncTasks)
        {
            Cell cell = cellStream.ResponseStream.Current;
            WorldVisualizer.AddCell(cell);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void OnDisable()
    {
        runAsyncTasks = false;
        channel.ShutdownAsync();
    }

    private async void RequestCellData()
    {
        var streamingstuff = Client.GetWorldCells(WorldRequest);
        while (await streamingstuff.ResponseStream.MoveNext())
        {
            Cell cell = streamingstuff.ResponseStream.Current;
            // add to list of all cells
            WorldVisualizer.AddCell(cell);
        }
    }

    private void RequestNodeData()
    {

    }

    private Vector3 GenerateRandomSpherePoint()
    {
        var theta = UnityEngine.Random.Range(0f, 1.0f) * 2 * Mathf.PI;
        var phi = Mathf.Asin(UnityEngine.Random.Range(0f, 1.0f) * 2 - 1);
        return new Vector3(Mathf.Cos(theta) * Mathf.Cos(phi) * WorldScaleFactor, Mathf.Sin(theta) * Mathf.Cos(phi) * WorldScaleFactor, Mathf.Sin(phi) * WorldScaleFactor);
    }
}
