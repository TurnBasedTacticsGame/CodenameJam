using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PickupLogUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TextMeshProUGUI textUi;
    [SerializeField] private Animator animator;
    [SerializeField] private TextWriter textWriter;

    private TextData currentLogData;
    private static readonly int Opened = Animator.StringToHash("Opened");


    private void OnEnable()
    {
        textWriter.OnWriteCharacter += WriteToUi;
        textUi.text = "";
    }
    
    private void OnDisable()
    {
        textWriter.OnWriteCharacter -= WriteToUi;
    }
    
    public void StartOpening(TextData newLogData)
    {
        gameObject.SetActive(true);
        animator.SetBool(Opened, true);
        currentLogData = newLogData;
    }
    
    // Used by button
    public void StartClosing()
    {
        animator.SetBool(Opened, false);
    }
    
    // Used by animator events
    public void Open()
    {
        gameObject.SetActive(true);
        textUi.gameObject.SetActive(true);
        textWriter.StartWriting(currentLogData);
    }

    // Used by animator events
    public void Close()
    {
        textWriter.StopWriting();
        gameObject.SetActive(false);
        textUi.gameObject.SetActive(false);
    }

    private void WriteToUi(string characters)
    {
        textUi.text += characters;
    }
}
