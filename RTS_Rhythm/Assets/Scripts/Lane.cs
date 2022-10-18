using Melanchall.DryWetMidi.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Lane : MonoBehaviour
{
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestriction;  // restricts note to a specific keynote
    public KeyCode input;
    public GameObject notePrefab;
    List<Note> notes = new List<Note>();
    public List<double> timeStamps = new List<double>();

    int spawnIndex = 0; // what time stamp needs to be spawned
    int inputIndex = 0; // what time stamp needs to be detected

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // SPAWN NOTE ---
        if (spawnIndex < timeStamps.Count)
        {
            // basically seeing if it's time for the note to spawn
            // spawn note X time before player actually taps it
            if (SongManager.GetAudioSourceTime() >= timeStamps[spawnIndex] - SongManager.instance.noteTime)
            {
                var note = Instantiate(notePrefab, transform);
                notes.Add(note.GetComponent<Note>());
                note.GetComponent<Note>().assignedTime = (float)timeStamps[spawnIndex];
                spawnIndex++;
            }
        }

        // SIMPLIFY CODE ---
        if (inputIndex < timeStamps.Count)
        {
            double timeStamp = timeStamps[inputIndex];
            double marginOfError = SongManager.instance.marginOfError;
            double audioTime = SongManager.GetAudioSourceTime() - (SongManager.instance.inputDelayInMilliseconds / 1000.0);

            // PLAYER INPUT ---
            if (Input.GetKeyDown(input))
            {
                double noteHitDelay = Math.Abs(audioTime - timeStamp);

                // player hits note
                if (noteHitDelay < marginOfError)
                {
                    HitNote();
                    print($"Hit on {inputIndex} note");
                    Destroy(notes[inputIndex].gameObject);
                    inputIndex++;
                }
                else
                {
                    print($"Hit inaccurate on {inputIndex} note with {noteHitDelay} delay");
                }

                // player misses note
                if (timeStamp + marginOfError >= audioTime)
                {
                    MissNote();
                    print($"Missed {inputIndex} note");
                    inputIndex++;
                }
            }
        }
    }
    
    private void HitNote()
    {
        ScoreManager.Hit();
    }

    private void MissNote()
    {
        ScoreManager.Miss();
    }

    // filter out what notes we don't need
    public void SetTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        foreach (var note in array) // if note name == note restriction) get note time
        {
            if (note.NoteName == noteRestriction)
            {
                // convert to metric timespan
                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, SongManager.midiFile.GetTempoMap());

                // convert to seconds
                timeStamps.Add((double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f);
            }
        }
    }
}
