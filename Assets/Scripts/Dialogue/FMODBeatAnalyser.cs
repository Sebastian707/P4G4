using UnityEngine;
using FMOD;
using FMODUnity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class FMODBeatAnalyzer : MonoBehaviour
{
    public static event Action OnBeat;

    private FMOD.DSP fftDSP;
    private FMOD.ChannelGroup targetGroup;

    public string fmodBusPath = "bus:/AdaptiveAudio"; // Set this in Inspector to match your FMOD bus path

    private int windowSize = 1024;
    private int bassBins = 10;

    private Queue<float> energyHistory = new Queue<float>();
    private int historySize = 43;
    private float beatThreshold = 1.5f;
    private float lastBeatTime = 0f;
    private float beatCooldown = 0.15f;

    void Start()
    {
        FMOD.System coreSystem = FMODUnity.RuntimeManager.CoreSystem;
        coreSystem.createDSPByType(FMOD.DSP_TYPE.FFT, out fftDSP);
        fftDSP.setParameterInt((int)FMOD.DSP_FFT.WINDOWSIZE, windowSize);
    }

    void Update()
    {
        if (!targetGroup.hasHandle())
        {
            FMOD.Studio.Bus bus;
            FMOD.RESULT busResult = FMODUnity.RuntimeManager.StudioSystem.getBus(fmodBusPath, out bus);
            if (busResult != FMOD.RESULT.OK || !bus.isValid())
                return;

            bus.lockChannelGroup();

            FMOD.RESULT cgResult = bus.getChannelGroup(out targetGroup);
            if (cgResult != FMOD.RESULT.OK || !targetGroup.hasHandle())
                return;

            targetGroup.addDSP(0, fftDSP);
        }

        if (fftDSP.handle == IntPtr.Zero) return;

        IntPtr dataPtr;
        uint length;
        fftDSP.getParameterData((int)FMOD.DSP_FFT.SPECTRUMDATA, out dataPtr, out length);

        if (dataPtr == IntPtr.Zero) return;

        var fftData = (FMOD.DSP_PARAMETER_FFT)Marshal.PtrToStructure(
            dataPtr, typeof(FMOD.DSP_PARAMETER_FFT)
        );

        if (fftData.numchannels == 0) return;

        float energy = 0f;
        for (int i = 0; i < bassBins; i++)
        {
            energy += fftData.spectrum[0][i];
        }

        energyHistory.Enqueue(energy);
        if (energyHistory.Count > historySize)
            energyHistory.Dequeue();

        float averageEnergy = 0f;
        foreach (float e in energyHistory)
            averageEnergy += e;
        averageEnergy /= energyHistory.Count;

        bool isBeat = false;
        if (energyHistory.Count >= historySize)
        {
            if (energy > averageEnergy * beatThreshold)
            {
                if (Time.time - lastBeatTime > beatCooldown)
                {
                    isBeat = true;
                    lastBeatTime = Time.time;
                }
            }
        }
        UnityEngine.Debug.Log($"Energy: {energy:F4} | Avg: {averageEnergy:F4}");
        if (isBeat)
        {
            UnityEngine.Debug.Log("🔥 BEAT DETECTED");
            OnBeat?.Invoke();
        }
    }

    void OnDestroy()
    {
        if (fftDSP.hasHandle())
        {
            if (targetGroup.hasHandle())
                targetGroup.removeDSP(fftDSP);

            fftDSP.release();
        }

        FMOD.Studio.Bus bus;
        if (FMODUnity.RuntimeManager.StudioSystem.getBus(fmodBusPath, out bus) == FMOD.RESULT.OK)
            bus.unlockChannelGroup();
    }
}