using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public enum State
    {
        PressToStart,
        Intro,
        TutorialSlow,
        TutorialFast,
        MainLevel,
        Ending,
        End
    }

    private State state;
    public Text debugText;
    public Text statusText;
    public Text scoreText;

    public AudioSource musicSource;  // voice lines and pattern music
    public AudioSource soundSource;  // buttons

    public AudioClip pressAnyButtonToStart;
    public AudioClip introVoiceLine1;
    public AudioClip introVoiceLine2;
    public AudioClip introVoiceLine3;
    public AudioClip tutorialSlowMusic;
    public AudioClip tutorialFastMusic;
    public AudioClip mainLevelMusic;
    public AudioClip buttonSoundError;
    public AudioClip buttonSoundSuccess;
    public AudioClip buttonSoundPerfect;
    public AudioClip neutralEndingVoiceLine1;
    public AudioClip neutralEndingVoiceLine2;
    public AudioClip neutralEndingVoiceLine3;
    public AudioClip neutralEndingVoiceLine4;
    public AudioClip neutralEndingVoiceLine5;
    public AudioClip goodEndingVoiceLine1;
    public AudioClip goodEndingVoiceLine2;
    public AudioClip goodEndingVoiceLine3;
    public AudioClip goodEndingVoiceLine4;
    public AudioClip goodEndingVoiceLine5;
    public AudioClip goodEndingVoiceLine6;
    public AudioClip goodEndingVoiceLine7;

    // smaller score is better
    public float scoreTutorialSlowThreshold = 2f;
    public float scoreTutorialFastThreshold = 2f;
    public float scoreNeutralEndingThreshold = 2f;
    public float scoreGoodEndingThreshold = 5f;
    private float score = 0f;

    private float startTime = 0f;
    public List<float> tutorialSlowBeats = new List<float>();
    public List<float> tutorialFastBeats = new List<float>();
    public List<float> mainLevelBeatsPattern1 = new List<float>();
    public List<float> mainLevelBeatsPattern2 = new List<float>();
    public List<float> mainLevelBeatsPattern3 = new List<float>();
    public List<float> mainLevelBeats = new List<float>();

    public float successDistanceThreshold = 1f;
    public float perfectDistanceThreshold = .1f;

    // Start is called before the first frame update
    void Start()
    {
        float patternLength = 4.3636f;
        float patternStartTime = 0f;
        for( int i=0; i < 8; ++i )
        {
            foreach( float f1 in mainLevelBeatsPattern1 )
            {
                mainLevelBeats.Add(patternStartTime+f1);
            }
            patternStartTime += patternLength;
        }
        for( int i = 0; i < 8; ++i )
        {
            foreach( float f2 in mainLevelBeatsPattern2 )
            {
                mainLevelBeats.Add(patternStartTime+f2);
            }
            patternStartTime += patternLength;
        }
        for( int i = 0; i < 8; ++i )
        {
            foreach( float f3 in mainLevelBeatsPattern3 )
            {
                mainLevelBeats.Add(patternStartTime+f3);
            }
            patternStartTime += patternLength;
        }

        state = State.PressToStart;
        debugText.text = "Press Any Button to Start!";
        musicSource.clip = pressAnyButtonToStart;
        musicSource.loop = true;
        musicSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        switch(state)
        {
            case State.PressToStart:
                if( Input.anyKeyDown )
                {
                    state = State.Intro;
                    debugText.text = "Introduction";
                    statusText.text = "(Currently playing intro)";
                    StartCoroutine(IntroSequence());
                }
                break;
            case State.Intro:
                // just waiting while intro plays
                break;
            case State.TutorialSlow:
                statusText.text = "Press keys to the rhythm!";
                ButtonPressCheck(tutorialSlowMusic, tutorialSlowBeats);
                break;
            case State.TutorialFast:
                ButtonPressCheck(tutorialFastMusic, tutorialFastBeats);
                break;
            case State.MainLevel:
                ButtonPressCheck(mainLevelMusic, mainLevelBeats, 17.455f);
                break;
            case State.End:
                if( Input.anyKeyDown )
                {
                    state = State.PressToStart;
                    debugText.text = "Press Any Button to Start Again!";
                    musicSource.clip = pressAnyButtonToStart;
                    musicSource.loop = true;
                    musicSource.Play();
                }
                break;
        }
    }

    IEnumerator IntroSequence()
    {
        Debug.Log("Intro Sequence");

        // play the first intro voice line
        musicSource.loop = false;
        musicSource.PlayOneShot(introVoiceLine1);

        yield return new WaitForSeconds(55.620f);

        debugText.text = "Introduction 2";
        musicSource.PlayOneShot(introVoiceLine2);

        yield return new WaitForSeconds(6.6f);

        debugText.text = "Introduction 3";
        musicSource.PlayOneShot(introVoiceLine3);

        yield return new WaitForSeconds(152.316f);

        state = State.TutorialSlow;
        debugText.text = "Tutorial 1";

        StartCoroutine(TutorialSlowSequence());

        yield break;
    }

    // modifies score
    public void ButtonPressCheck(AudioClip music, List<float> beats)
    {
        ButtonPressCheck(music, beats, 0f);
    }

    public void ButtonPressCheck(AudioClip music, List<float> beats, float introOffset)
    {
        if( Input.anyKeyDown )
        {
            // calculate timing
            float currentPlayThroughTime = (Time.time - startTime) % music.length;
            Debug.Log("currentPlayThroughTime:"+currentPlayThroughTime);
            // find the nearest beat
            int beatIndex = beats.BinarySearch(currentPlayThroughTime-introOffset);
            Debug.Log("1 beatIndex:"+beatIndex);
            if( beatIndex < 0 )
            {
                int nextLargestIndex = ~beatIndex;
                Debug.Log("nextLargestIndex:"+nextLargestIndex+" beats:"+beats);
                if( nextLargestIndex == beats.Count )
                {
                    beatIndex = beats.Count - 1;
                }
                else if( nextLargestIndex == 0 )
                {
                    beatIndex = 0;
                }
                else
                {
                    if( Mathf.Abs(introOffset + beats[nextLargestIndex] - currentPlayThroughTime) < Mathf.Abs(introOffset + beats[nextLargestIndex-1] - currentPlayThroughTime))
                    {
                        beatIndex = nextLargestIndex;
                    }
                    else
                    {
                        beatIndex = nextLargestIndex - 1;
                    }
                }
            }
            Debug.Log("2 beatIndex:"+beatIndex);
            float distanceFromNearestBeat = Mathf.Abs(introOffset + beats[beatIndex] - currentPlayThroughTime);
            Debug.Log("distanceFromNearestBeat:"+distanceFromNearestBeat);
            // score the button press
            if( distanceFromNearestBeat < perfectDistanceThreshold )
            {
                debugText.text = "button sound perfect";
                soundSource.PlayOneShot(buttonSoundPerfect);
                score += successDistanceThreshold - distanceFromNearestBeat;
            }
            else if( distanceFromNearestBeat < successDistanceThreshold )
            {
                debugText.text = "button sound success";
                soundSource.PlayOneShot(buttonSoundSuccess);
                score += successDistanceThreshold - distanceFromNearestBeat;
            }
            else
            {
                debugText.text = "button sound error";
                soundSource.PlayOneShot(buttonSoundError);
                score -= successDistanceThreshold;
            }
            scoreText.text = "Score: "+score;
        }
    }

    public IEnumerator TutorialSlowSequence()
    {
        Debug.Log("Tutorial Slow Sequence");
        musicSource.clip = tutorialSlowMusic;
        musicSource.loop = true;
        musicSource.Play();
        startTime = Time.time;
        score = 0f;

        bool passed = false;
        while(!passed)
        {
            yield return new WaitForSeconds(17.455f);

            // check button press times
            debugText.text = "score:"+score+" target:"+scoreTutorialSlowThreshold;
            if( score >= scoreTutorialSlowThreshold )
            {
                passed = true;
            }
        }

        state = State.TutorialFast;
        debugText.text = "Tutorial 2";
        StartCoroutine(TutorialFastSequence());

        yield break;
    }

    public IEnumerator TutorialFastSequence()
    {
        Debug.Log("Tutorial Fast Sequence");

        musicSource.clip = tutorialFastMusic;
        musicSource.loop = true;
        musicSource.Play();
        startTime = Time.time;
        score = 0f;

        bool passed = false;
        while( !passed )
        {
            yield return new WaitForSeconds(17.455f);

            // check button press times
            debugText.text = "score:"+score+" target:"+scoreTutorialFastThreshold;
            if( score >= scoreTutorialFastThreshold )
            {
                passed = true;
            }
        }

        state = State.MainLevel;
        debugText.text = "Main Level";
        StartCoroutine(MainLevelSequence());

        yield break;
    }

    public IEnumerator MainLevelSequence()
    {
        Debug.Log("Main Level Sequence");

        musicSource.clip = mainLevelMusic;
        musicSource.loop = true;
        musicSource.Play();
        startTime = Time.time;
        score = 0f;

        bool passed = false;
        while( !passed )
        {
            yield return new WaitForSeconds(142.909f);

            // check button press times
            debugText.text = "score:"+score+" target:"+scoreNeutralEndingThreshold+" target:"+scoreGoodEndingThreshold;
            if( score >= scoreNeutralEndingThreshold )
            {
                passed = true;
            }
        }

        musicSource.Stop();
        state = State.Ending;
        StartCoroutine(EndingSequence());

        yield break;
    }

    public IEnumerator EndingSequence()
    {
        Debug.Log("Ending Sequence");

        if( score < scoreGoodEndingThreshold )
        {
            debugText.text = "Ending";
            statusText.text = "(Ending playing)";
            // play the neutral ending voice line
            musicSource.loop = false;
            musicSource.PlayOneShot(neutralEndingVoiceLine1);
            yield return new WaitForSeconds(16.524f);
            musicSource.PlayOneShot(neutralEndingVoiceLine2);
            yield return new WaitForSeconds(2.136f);
            musicSource.PlayOneShot(neutralEndingVoiceLine3);
            yield return new WaitForSeconds(36.396f);
            musicSource.PlayOneShot(neutralEndingVoiceLine4);
            yield return new WaitForSeconds(3.576f);
            musicSource.PlayOneShot(neutralEndingVoiceLine5);
            yield return new WaitForSeconds(10.98f);
        }
        else
        {
            debugText.text = "Good Ending";
            statusText.text = "(Good Ending playing)";
            // play the good ending voice line
            musicSource.loop = false;
            musicSource.PlayOneShot(goodEndingVoiceLine1);
            yield return new WaitForSeconds(16.524f);
            musicSource.PlayOneShot(goodEndingVoiceLine2);
            yield return new WaitForSeconds(2.136f);
            musicSource.PlayOneShot(goodEndingVoiceLine3);
            yield return new WaitForSeconds(23.724f);
            musicSource.PlayOneShot(goodEndingVoiceLine4);
            yield return new WaitForSeconds(0.936f);
            musicSource.PlayOneShot(goodEndingVoiceLine5);
            yield return new WaitForSeconds(9.504f);
            musicSource.PlayOneShot(goodEndingVoiceLine6);
            yield return new WaitForSeconds(1.8f);
            musicSource.PlayOneShot(goodEndingVoiceLine7);
            yield return new WaitForSeconds(11.7f);
        }

        state = State.End;
        debugText.text = "End";
        statusText.text = "Thank you for playing!";

        yield break;
    }
}
