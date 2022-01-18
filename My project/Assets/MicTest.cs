using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class MicTest : MonoBehaviour
{
    enum RecordState
    {
        Wait, //대기
        Input, //입력
        Recording,//녹음중
        Saving,//저장중
        Done //완료
    }
    RecordState recordState;
    AudioSource _audio;
    bool recording;
    bool switching;
    float sensitivity = 100;
    float loudness = 0;
    float recTime = 5;

    void Start()
    {
        switching = true;
        _audio = GetComponent<AudioSource>();
        _audio.clip = Microphone.Start(Microphone.devices[0], true, 10, 44100);
        _audio.loop = true;
        while (!(Microphone.GetPosition(Microphone.devices[0]) > 0)) { }
        _audio.Play();
    }
    void Update()
    {
        loudness = GetAveragedVolume() * sensitivity;
        Debug.Log("LoudnessUpdate : " + loudness);
        if (switching)
        {
            SwitchRecording();
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
    void StartMicrophone()
    {
        _audio.clip = Microphone.Start(Microphone.devices[0], true, 100, 44100);
    }
    void StopMicrophone()
    {
        int lastTime = Microphone.GetPosition(Microphone.devices[0]);

        Debug.Log("LastTime" + lastTime);
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
            //SavWav.Save("Voice", _audio.clip);
        }
    }
    void SwitchRecording()
    {

        switch (recordState)
        {
            case RecordState.Wait:
                Wait();
                break;
            case RecordState.Input:
                InputMic();
                break;
            case RecordState.Recording:
                Recording();
                break;
            case RecordState.Saving:
                Saving();
                break;
            case RecordState.Done:
                Done();
                break;
        }
    }
    void Wait()
    {//대기중 소리가 1이상이면 Input
        if (loudness > 1)
        {
            Microphone.End(Microphone.devices[0]);
            recordState = RecordState.Input;
        }
    }
    void InputMic()
    {//마이크 입력 시작
        Microphone.Start(Microphone.devices[0], true, 10, 44100);
        if(loudness > 1)
        {
            Debug.Log("dd" + loudness);
        }
    }
    void Recording()
    {
        Debug.Log("LoudnessRecording" + GetAveragedVolume() * sensitivity);
    }
    void Saving()
    {
        StopMicrophone();
        SavWav.Save("Voice", _audio.clip);
        recordState = RecordState.Done;
    }
    void Done()
    {
        switching = false;
    }
}
