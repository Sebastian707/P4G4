using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Runtime.InteropServices;

public class FMODTempoBeat : MonoBehaviour
{
    public static event Action OnBeat;


    public EventReference eventPath;

    private EventInstance musicInstance;

    private FMOD.Studio.EVENT_CALLBACK beatCallback;

    void Start()
    {
        musicInstance = RuntimeManager.CreateInstance(eventPath);

        beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatCallback);

        musicInstance.setCallback(
            beatCallback,
            EVENT_CALLBACK_TYPE.TIMELINE_BEAT
        );

        musicInstance.start();
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    private static FMOD.RESULT BeatCallback(
        EVENT_CALLBACK_TYPE type,
        IntPtr instancePtr,
        IntPtr parameterPtr)
    {
        if (type != EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
            return FMOD.RESULT.OK;

        var beat = (TIMELINE_BEAT_PROPERTIES)
            Marshal.PtrToStructure(
                parameterPtr,
                typeof(TIMELINE_BEAT_PROPERTIES)
            );

        OnBeat?.Invoke();

        Debug.Log($"Beat | BPM: {beat.tempo} Beat: {beat.beat} Bar: {beat.bar}");

        return FMOD.RESULT.OK;
    }

    void OnDestroy()
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicInstance.release();
    }
}