using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using FMODUnity;
using FMOD.Studio;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject commsPanel;
    public Image portraitImage;
    public TMP_Text speakerNameText;
    public TMP_Text subtitleText;

    private EventInstance voiceInstance;

    public IEnumerator PlayDialogueCoroutine(DialogueSO dialogue)
    {
        commsPanel.SetActive(true);

        foreach (DialogueLine line in dialogue.lines)
        {
            portraitImage.sprite = line.portrait;
            speakerNameText.text = line.speakerName;
            speakerNameText.color = line.speakerNameColor;

            if (!line.voiceClip.IsNull)
            {
                voiceInstance = RuntimeManager.CreateInstance(line.voiceClip);
                voiceInstance.start();
            }

            foreach (SubtitleSegment segment in line.subtitles)
            {
                subtitleText.text = segment.text;
                yield return new WaitForSeconds(segment.duration);
            }

            if (!line.voiceClip.IsNull)
            {
                yield return new WaitWhile(() =>
                {
                    voiceInstance.getPlaybackState(out PLAYBACK_STATE state);
                    return state != PLAYBACK_STATE.STOPPED;
                });
                voiceInstance.release();
            }

            subtitleText.text = "";
        }

        commsPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (voiceInstance.isValid())
        {
            voiceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            voiceInstance.release();
        }
    }
}