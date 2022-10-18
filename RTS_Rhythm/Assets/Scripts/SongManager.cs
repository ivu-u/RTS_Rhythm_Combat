using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.IO;
using UnityEngine.Networking;


public class SongManager : MonoBehaviour
{
    public static SongManager instance;
    public AudioSource audioSource;
    public float songDelayInSeconds;
    public double marginOfError;    // in seconds
    public int inputDelayInMilliseconds;

    public Lane[] lanes;
    public int laneYPos;
    public int laneXPos;
    public float laneDistanceFromLane;

    public string fileLocation; // where midi file is kept
    public float noteTime;  // how much time the note will be on screen
    public float noteSpawnY;
    public float noteTapY;
    public float noteDespawnY
    {
        get
        {
            return noteTapY - (noteSpawnY - noteTapY);
        }
    }

    public static MidiFile midiFile;    // midi file loads on ram (stay here once parsed)

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        // streaming websites can be a website or file
        // on windows, mac, or linux == file
        // webgl == website
        if (Application.streamingAssetsPath.StartsWith("http://")|| Application.streamingAssetsPath.StartsWith("https://")) // website
        {
            StartCoroutine(ReadFromWebsite());
        }
        else    //file
        {
            ReadFromFile();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator ReadFromWebsite()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + fileLocation))
        {
            yield return www.SendWebRequest();

            // check for network errors
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else // if everythig is okay then just read results
            {
                byte[] results = www.downloadHandler.data;
                // send results to memory stream
                using (var stream = new MemoryStream(results))
                {
                    // load this stream onto the midi file
                    midiFile = MidiFile.Read(stream);

                    SpawnLanes();
                    GetDataFromMidi();
                }
            }
        }
    }

    private void ReadFromFile()
    {
        // input file location rather than stream
        midiFile = MidiFile.Read(Application.streamingAssetsPath + "/" + fileLocation);

        SpawnLanes();
        GetDataFromMidi();
    }

    private void GetDataFromMidi()
    {
        var notes = midiFile.GetNotes();    // icollection
        var array = new Melanchall.DryWetMidi.Interaction.Note[notes.Count];  // we want icollection --> array
        notes.CopyTo(array, 0);

        // set time stamps for lanes (filter time stamps)
        foreach (var lane in lanes) lane.SetTimeStamps(array);

        Invoke(nameof(StartSong), songDelayInSeconds);  // start song after delay
    }

    // spawn lanes a certain distance away from each other
    private void SpawnLanes()
    {
        for (int i = 0; i < lanes.Length; i++)
        {
            Instantiate(lanes[i], new Vector3(laneXPos,laneYPos - (laneDistanceFromLane * i),0), Quaternion.identity);
        }
    }

    public void StartSong()
    {
        audioSource.Play();
    }

    public static double GetAudioSourceTime()
    {
        return (double)instance.audioSource.timeSamples / instance.audioSource.clip.frequency;
    }
}
