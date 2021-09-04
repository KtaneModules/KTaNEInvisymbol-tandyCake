using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class InvisymbolScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;

    public SpriteMask[] masks;
    public SpriteRenderer innerSymbol;
    public KMSelectable submit, next;
    public Sprite[] maskSymbols, displaySymbols;

    private DisplayInfo[] displays = new DisplayInfo[5];
    private int displayPointer;
    private List<int> adjPositions = new List<int>();
    private int decoy;
    private int correctPointer;


    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake () {
        moduleId = moduleIdCounter++;
        next.OnInteract += delegate () { NextPress(); return false; };
        submit.OnInteract += delegate () { Submit(); return false; };
    }

    void Start ()
    {
        GetPositions();
        GenerateSymbols();
        ShowSymbol();
    }

    void GetPositions()
    {
        adjPositions.Add(Rnd.Range(0, 25));
        for (int i = 0; i < 3; i++)
            adjPositions.Add(adjPositions.SelectMany(x => GetAdjacents(x)).Distinct().PickRandom(x => !adjPositions.Contains(x))); //Adds an adjacent square which has not already been taken.
        decoy = Enumerable.Range(0, 25).PickRandom(x => !adjPositions.SelectMany(pos => GetAdjacents(pos)).Contains(x)); //Adds a random square which is not adjacent to any already present one.
        Log("The four adjacent symbols are at positions {0} in reading order.", adjPositions.Select(x => x + 1).Join(", "));
        Log("The symbol which is not adjacent is at position {0} in reading order.", decoy + 1);
    }
    void GenerateSymbols()
    {
        for (int i = 0; i < 4; i++)
            displays[i] = new DisplayInfo(displaySymbols[adjPositions[i]], maskSymbols.PickRandom(), new Vector3(Rnd.Range(0f, 360), Rnd.Range(0f, 360), Rnd.Range(0f, 360)), adjPositions[i]);
        displays[4] = new DisplayInfo(displaySymbols[decoy], maskSymbols.PickRandom(), new Vector3(Rnd.Range(0f, 360), Rnd.Range(0f, 360), Rnd.Range(0f, 360)), decoy);
        displays.Shuffle();
    }
    void ShowSymbol()
    {
        Sprite outerSymbol = displays[displayPointer].masks;
        foreach (SpriteMask mask in masks)
            mask.sprite = outerSymbol;
        innerSymbol.sprite = displays[displayPointer].inner;
        innerSymbol.transform.localEulerAngles = displays[displayPointer].rotation;
    }
    void NextPress()
    {
        next.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, next.transform);
        Audio.PlaySoundAtTransform("shutter", transform);
        displayPointer++;
        displayPointer %= 5;
        ShowSymbol();
    }
    void Submit()
    {
        submit.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submit.transform);
        if (moduleSolved)
            return;
        int submittedIx = displays[displayPointer].val;
        if (submittedIx == decoy)
        {
            moduleSolved = true;
            Log("Submitted the symbol in position {0}, module solved!", submittedIx + 1);
            Module.HandlePass();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        }
        else
        {
            Log("Submitted the symbol in position {0}, strike!", submittedIx + 1);
            Module.HandleStrike();
        }
    }

    IEnumerable<int> GetAdjacents(int pos)
    {
        if (pos >= 5) yield return pos - 5;
        if (pos < 20) yield return pos + 5;
        if (pos % 5 != 0) yield return pos - 1;
        if (pos % 5 != 4) yield return pos + 1;
    }

    void Log(string message, params object[] args)
    {
        Debug.LogFormat("[Invisymbol #{0}] {1}", moduleId, string.Format(message, args));
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use <!{0} next> to go the next symbol. Use <!{0} submit> to submit the currently displayed symbol.";
    #pragma warning restore 414

    IEnumerator Press(KMSelectable btn, float delay)
    {
        btn.OnInteract();
        yield return new WaitForSeconds(delay);
    }
    IEnumerator ProcessTwitchCommand (string command)
    {
        command = command.Trim().ToUpperInvariant();
        if (command == "NEXT")
        {
            yield return null;
            yield return Press(next, 0.1f);
        }
        else if (command == "SUBMIT")
        {
            yield return null;
            yield return Press(submit, 0.1f);
        }
    }

    IEnumerator TwitchHandleForcedSolve ()
    {
        while (displays[displayPointer].val != decoy)
            yield return Press(next, 0.2f);
        yield return Press(submit, 0.1f);
    }
}
