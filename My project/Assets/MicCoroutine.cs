using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class MicCoroutine : MonoBehaviour
{
    AudioSource _audio;
    float sensitivtity = 100;
    float loudness = 0;
    bool recording;
    bool saving;
    void Start()
    {
        _audio = GetComponent<AudioSource>();
        _audio.clip = Microphone.Start(Microphone.devices[0], true, 1, 44100);
        _audio.loop = true;
        while (!(Microphone.GetPosition(Microphone.devices[0]) > 0))
        { }
        _audio.Play();
        recording = false;
        saving = false;
    }

    void FixedUpdate()
    {
        loudness = GetAveragedVolume() * sensitivtity;
        Debug.Log("Loudness" + loudness);
        if (loudness > 1 && !recording)
        {
            recording = true;
            StartCoroutine(InputVoice(loudness));
            return;
        }
    }

    float GetAveragedVolume()
    {
        float[] data = new float[256];
        float a = 0;
        _audio.GetOutputData(data, 0);
        foreach (float s in data)
        {
            a += Mathf.Abs(s);
        }
        return a / 256;
    }

    IEnumerator InputVoice(float LoudNess)
    {
        if (recording)
        {
            Microphone.End(Microphone.devices[0]);
            _audio.clip = Microphone.Start(Microphone.devices[0], true, 10, 44100);

            recording = false;
            saving = true;
            yield return new WaitForSeconds(2);
        }
        else if (saving)
        {
            StopMicrophone();
            SavWav.Save("voice", _audio.clip);
            saving = false;
        }
        yield return null;
    }
    void StopMicrophone()
    {//클립 자르기
        int lastTime = Microphone.GetPosition(Microphone.devices[0]);

        if (lastTime == 0)
        {
            return; 
        }
        else
        {
            Microphone.End(Microphone.devices[0]);
            float[] samples = new float[_audio.clip.samples];
            _audio.clip.GetData(samples, 0);
            float[] cutSamples = new float[lastTime];
            Array.Copy(samples, cutSamples, cutSamples.Length - 1);
            _audio.clip = AudioClip.Create("Voice", cutSamples.Length, 1, 44100, false);
            _audio.clip.SetData(cutSamples, 0);
        }
    }
}
