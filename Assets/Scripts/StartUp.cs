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

    public Boolean runAsyncTasks = true;

    // Use this for initialization
    void Start () {
        channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
        Client = new Everett.Ebstorf.WorldStreamer.WorldStreamerClient(channel);

        Debug.Log("Requesting world over GRPC");
        WorldRequest = new WorldRequest { WorldId = WorldSeed };
        var worldGenerationResult = Client.GenerateWorld(WorldRequest);
        WorldVisualizer.SetWorldScaleFactor((float)GetWorldResultQuantity(worldGenerationResult, "radius"));
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
        int nodeCount = 0;
        while (await nodeStream.ResponseStream.MoveNext() && runAsyncTasks)
        {
            CartesianStructure cartesianStructure = nodeStream.ResponseStream.Current;
            WorldVisualizer.AddNodeVertex(cartesianStructure.Id, (float)cartesianStructure.X, (float)cartesianStructure.Y, (float)cartesianStructure.Z);
            if (nodeCount++ % 100 == 0)
            {
                Debug.Log("Total vertices: " + nodeCount.ToString());
            }
        }
        var cellStream = Client.GetWorldCells(WorldRequest);
        while (await cellStream.ResponseStream.MoveNext() && runAsyncTasks)
        {
            Cell cell = cellStream.ResponseStream.Current;
            WorldVisualizer.AddCell(cell);
        }
        WorldVisualizer.MeshFinished();
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void OnDisable()
    {
        runAsyncTasks = false;
        channel.ShutdownAsync();
    }
    /*
    private async void RequestCellData()
    {
        var streamingstuff = Client.GetWorldCells(WorldRequest);
        while (await streamingstuff.ResponseStream.MoveNext())
        {
            Cell cell = streamingstuff.ResponseStream.Current;
            // add to list of all cells
            WorldVisualizer.AddCell(cell);
        }
        WorldVisualizer.RedrawMesh();
    }

    private void RequestNodeData()
    {

    }
    */

    private double GetWorldResultQuantity(WorldGenerationResult result, string quantityName)
    {
        foreach (var quantity in result.World.Quantities)
        {
            if (quantity.Key.Name.Equals(quantityName))
            {
                return quantity.Value;
            }
        }
        return 0.0;
    }
}
