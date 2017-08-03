using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Creates a 3d head from a series of spheres, using HDF5 brain scan data extracted into CSV files
/// </summary>
public class HeadMaker : MonoBehaviour {

    /// <summary>
    /// UI element for the current time display
    /// </summary>
    public Text currentTimeText;

    /// <summary>
    /// Whether the play button has been pressed or not
    /// </summary>
    private bool playing;

    /// <summary>
    /// A timer that increments alongside deltaTime if the play button has been pressed
    /// </summary>
    private float changeTimer;

    /// <summary>
    /// The head object which contains all nodes
    /// </summary>
    private GameObject head;

    /// <summary>
    /// A list of all active nodes in the simulation
    /// </summary>
    private List<GameObject> nodes = new List<GameObject>();

    /// <summary>
    /// A list of lists of data values, each list belonging to one node
    /// </summary>
    private List<double[]> data = new List<double[]>();

    /// <summary>
    /// The current time in the simulation
    /// </summary>
    int currentTime;

    /// <summary>
    /// The minimum data value
    /// </summary>
    double min = double.MaxValue;

    /// <summary>
    /// The maximum data value
    /// </summary>
    double max = double.MinValue;

    /// <summary>
    /// Called once at the start of the application
    /// Extracts head shape and brain data from the csv files and populates the <see cref="nodes"/> and <see cref="data"/> lists with them
    /// </summary>
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

    /// <summary>
    /// Called every frame
    /// Rotates the head representation and controls the current timestamp being displayed
    /// </summary>
    void Update () {
        //Slowly rotate the head over time
        head.transform.Rotate(0, 0, 0.5f);

        //If the play button has been pressed, then increment the time value automatically
        if(playing)
        {
            changeTimer += Time.deltaTime;
            if (changeTimer > 0.25f)
            {
                IncrementTime();
                changeTimer = 0;
            }
        }
	}

    /// <summary>
    /// Called whent the play button is pressed
    /// Starts the brain animation
    /// </summary>
    public void PlayButtonPressed()
    {
        playing = true;
        changeTimer = 0;
    }

    /// <summary>
    /// Called when the pause button is pressed
    /// Pauses the brain animation
    /// </summary>
    public void PauseButtonPressed()
    {
        playing = false;
    }

    /// <summary>
    /// Increments the current time value of the simulation
    /// </summary>
    public void IncrementTime()
    {
        if (currentTime < data[0].Length - 1)
            ChangeTime(currentTime + 1);
        else
            ChangeTime(0);
    }

    /// <summary>
    /// Decrements the current time value of the simulation
    /// </summary>
    public void DecrementTime()
    {
        if (currentTime > 0)
            ChangeTime(currentTime - 1);
        else
            ChangeTime(data[0].Length - 1);
    }

    /// <summary>
    /// Changes the time in the simulation to the passed in time value
    /// </summary>
    /// <param name="newTime">The time to set the simulation to</param>
    private void ChangeTime(int newTime)
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
