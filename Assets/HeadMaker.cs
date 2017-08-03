using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Creates a 3d head from a series of spheres, using HDF5 brain scan data extracted into CSV files
/// </summary>
public class HeadMaker : MonoBehaviour {

    //UI element for the current time display
    public Text currentTimeText;

    //Whether the play button has been pressed or not
    private bool playing;

    //A timer that increments alongside deltaTime if the play button has been pressed
    private float changeTimer;

    //The head object which contains all nodes
    private GameObject head;

    //A list of all active nodes in the simulation
    private List<GameObject> nodes = new List<GameObject>();

    //A list of lists of data values, each list belonging to one node
    private List<double[]> data = new List<double[]>();

    //The current time in the simulation
    int currentTime;

    //Create min and max doubles, to determine the minimum and maximum data values
    double min = double.MaxValue;
    double max = double.MinValue;

    void Start () {
        //Read the head shape file
        string[] fileContents = File.ReadAllLines("head_shape.csv");

        //Create a list to store vertices in for the head
        List<Vector3> verts = new List<Vector3>();

        //Parse the head shape file to obtain the vertices
        for(int i = 0; i < fileContents.Length; i++)
        {
            string[] lineContents = fileContents[i].Split(',');
            verts.Add(new Vector3(float.Parse(lineContents[0]), float.Parse(lineContents[1]), float.Parse(lineContents[2])));
        }

        //Create the head object
        head = new GameObject();
        head.name = "Head";

        //Create each 'vertice' of the head as a sphere
        foreach(Vector3 vert in verts)
        {
            GameObject headPoint = Instantiate(Resources.Load("Ball") as GameObject, vert * 5, Quaternion.identity);
            headPoint.transform.parent = head.transform;
            nodes.Add(headPoint);
        }

        //Rotate the head to face the camera
        head.transform.rotation = Quaternion.Euler(-90, 90, 0);

        //Read the data file
        StreamReader dataStream = new StreamReader("data.csv");
        for (int i = 0; i < verts.Count; i++)
        {
            string[] dataText = dataStream.ReadLine().Split(',');
            double[] dataNumbers = new double[dataText.Length];

            for(int j = 0; j < dataText.Length - 1; j++)
            {
                dataNumbers[j] = double.Parse(dataText[j]);
            }
            data.Add(dataNumbers);
        }

        //Get the correct values for the min and max doubles
        foreach (double[] d in data)
        {
            for(int i = 0; i < d.Length; i++)
            {
                if (d[i] > max)
                    max = d[i];

                if (d[i] < min)
                    min = d[i];
            }
        }

        //Set the initial time for the simulation to be 0
        ChangeTime(0);
    }

    void Update () {
        //Slowly rotate the head over time
        head.transform.Rotate(0, 0, 0.5f);

        //If the play button has been pressed, then increment the time value automatically
        if(playing)
        {
            changeTimer += Time.deltaTime;
            if (changeTimer > 0.25f)
            {
                incrementTime();
                changeTimer = 0;
            }
        }
	}

    public void PlayButtonPressed()
    {
        playing = true;
        changeTimer = 0;
    }

    public void PauseButtonPressed()
    {
        playing = false;
    }

    public void incrementTime()
    {
        if (currentTime < data[0].Length - 1)
            ChangeTime(currentTime + 1);
        else
            ChangeTime(0);
    }

    public void decrementTime()
    {
        if (currentTime > 0)
            ChangeTime(currentTime - 1);
        else
            ChangeTime(data[0].Length - 1);
    }

    void ChangeTime(int newTime)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            //Update the current time display in the UI
            currentTime = newTime;
            currentTimeText.text = "Current Time: " + currentTime;

            //Calculate the percentage of this nodes data value between the min and max values and color it accordingly
            double percentage = (data[i][newTime] - min) / (max - min);
            nodes[i].GetComponent<Renderer>().material.color = new Color32((byte)(255 - (percentage * 255)), (byte)(percentage * 255), 0, 255);

            //Scale each node according to its percentage between the min and max values
            float scaleFactor = Mathf.Clamp((float)(.1f - (0.5f - percentage)), 0.08f, 0.12f);
            nodes[i].transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        }
    }
}
