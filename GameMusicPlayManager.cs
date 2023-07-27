using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 게임 플레이 시 음악을 재생하고 BMS 데이터를 기반으로 비트 이벤트를 발생과 관련된 전반적인 기능을 담당
public class GameMusicPlayManager : MonoBehaviour
{
    private static GameMusicPlayManager instance = null;

    public static GameMusicPlayManager Instance
    {
        get { return instance; }
    }

    AudioSource BGMPlayer;

    // Game Play Sound
    string bgmFileName;       // 현재 플레이할 음악 파일
    Dictionary<string, AudioClip> bgmDic;

    // Game Play BMS File
    BMS playBMS;       // 현재 플레이할 bms 데이터 저장용

    // 플레이 관련 데이터
    public int playBar = 0;
    public int playBeat = 0;

    decimal prevBGMTime = 0;

    const decimal BMSC_BEAT_TIME = 0.25m;   // BMSC의 1비트 시간 (bmsc가 1마디 16비트 기반이므로 0.25초 고정)

    // 빨리감기 모드
    bool isFastWindingMode = false;
    public bool IsFastWindingMode
    {
        set
        {
            isFastWindingMode = value;
            Debug.Log("FAST WINDING MODE STATE ===> " + isFastWindingMode);
        }

        get { return isFastWindingMode; }
    } 
        
    int windingBar = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(this);
        }

        BGMPlayer = GetComponent<AudioSource>();
        bgmDic = new Dictionary<string, AudioClip>();

        playBMS = new BMS();
    }

    private void Start()
    {
        LoadAllBGMResource();
    }

    // GameMusic 폴더에 있는 모든 BGM 리소스를 로드
    void LoadAllBGMResource()
    {
        AudioClip[] bgmClips = Resources.LoadAll<AudioClip>("GameMusic");

        foreach (var clip in bgmClips)
        {
            if (bgmDic.ContainsKey(clip.name) == false)
            {
                bgmDic[clip.name] = clip;

                Debug.Log("Load clip name -->" + clip.name);
            }
        }
    }

    // BGM 파일을 Set
    public void SetBGMFileName(string fileName)
    {
        if(string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("[func]SetBGMFileName -> file name error");
            return;
        }

        bgmFileName = fileName;
    }

    // 플레이할 BMS 파일을 Set
    public void SetPlayBmsData(BMS bmsData)
    {
        if (bmsData == null)
        {
            Debug.LogError("[func]SetPlayBmsData -> bmsData is null.");
            return;
        }

        playBMS = bmsData;
    }

    // 게임음악을 재생
    public bool PlayBGMSound(string bgmName)
    {
        if (bgmDic[bgmName] == null || BGMPlayer == null || string.IsNullOrEmpty(bgmName))
        {
            Debug.LogError("[func]PlayBGMSound -> null error.");
            return false;
        }

        BGMPlayer.clip = bgmDic[bgmName];
        BGMPlayer.Play();

        prevBGMTime = (decimal)BGMPlayer.time;

        return true;
    }

    // 게임음악 재생버튼을 누름
    public void BeginGamePlay()
    {
        if (playBMS == null)
        {
            Debug.LogError("[func]BeginGamePlay -> playBMS is null.");
            return;
        }

        // 음악 재생에 문제가 있을 경우 실행X
        if(PlayBGMSound(bgmFileName) == false)
        {
            Debug.LogError("[func]BeginGamePlay -> PlayBGMSound() return false.");
            return;
        }

        ResetMusicPlayData();

        decimal checkTime = BeatCheckTimeCaculator(playBMS.bpm);
        StartCoroutine(BeatChecker(checkTime));
    }

    // 재생할 음악의 BPM 기준으로 1비트 체크시간을 계산
    decimal BeatCheckTimeCaculator(float bpm)
    {
        decimal resTime = 0;

        if (bpm <= 0)
        {
            Debug.LogError("[func]BeatCheckTimeCaculator -> bpm value error.");
            return resTime;
        }

        decimal averageBeat = (decimal)bpm / 60;
        resTime = BMSC_BEAT_TIME / averageBeat;        
        
        return resTime;
    }

    // 비트 체크관련 변수 초기화
    void ResetMusicPlayData()
    {
        playBar = 0;
        playBeat = 0;

        prevBGMTime = 0;
    }
    
    // 실제 비트를 체크하여 노트가 존재하는지 루프를 돌며 검사
    // 노트가 존재할 경우 비트 이벤트 생성 함수 호출
    IEnumerator BeatChecker(decimal beatCheckTime)
    {
        decimal nowPlayTime = 0;                            // 재생시간 체크용

        decimal fixBeatCheckTime = 0;                       // 실제 비트에서 엇나간 값
        decimal totalBeatCheckTime = 0;                     // 실제 비트가 재생되야하는 시간 누적값
        decimal resultBeatCheckTime = beatCheckTime;        // 현재 비트와 엇나간 비트를 교정한 값

        while (playBar < 100)
        {

            nowPlayTime = (decimal)BGMPlayer.time - prevBGMTime;

            //playBar = (int)(BGMPlayer.time / (double)resultBarCheckTime);

            if (nowPlayTime >= resultBeatCheckTime)
            {
                if (playBMS.noteDataDic.ContainsKey(playBar) && playBMS.noteDataDic[playBar].Count > 0)
                {
                    foreach (var beat in playBMS.noteDataDic[playBar])
                    {
                        var noteType = beat.Value[playBeat];

                        if (noteType.Equals(0) == false)
                        {
                            //Debug.Log(noteType + "   " + playBeat + "   " + playBar + "   " + beat.Key);
                            BeatEventManager.Instance.PostNoteBeatEvent((CHANNEL_TYPE)beat.Key, (NOTE_BEAT_EVENT)noteType, this);
                        }
                    }
                }
               

                playBeat++;
                
                prevBGMTime = (decimal)BGMPlayer.time;
                nowPlayTime = 0;

                totalBeatCheckTime += beatCheckTime;

                // 비트가 진행될수록 틀어지는 비트속도를 교정
                if ((decimal)BGMPlayer.time > totalBeatCheckTime)
                {
                    fixBeatCheckTime = (decimal)BGMPlayer.time - totalBeatCheckTime;
                    resultBeatCheckTime = beatCheckTime - fixBeatCheckTime;
                }
                else
                {
                    fixBeatCheckTime = totalBeatCheckTime - (decimal)BGMPlayer.time;
                    resultBeatCheckTime = beatCheckTime + fixBeatCheckTime;
                }
            }

            // 1마디를 넘어갈 경우 다음 마디로 진행
            if (playBeat > 15)
            {
                playBeat = 0;

                playBar++;
            }

            #if UNITY_EDITOR

            // 빨리감기 관련
            if (IsFastWindingMode)
            {
                if (windingBar <= playBar)
                {
                    ResetFastWindingMode();
                }
            }

            #endif

            yield return null;
        }
    }

    #region 노트 제작 전용

    public void ResetFastWindingMode()
    {
        Time.timeScale = 1.0f;
        BGMPlayer.pitch = 1.0f;

        IsFastWindingMode = false;
    }

    public void BeginGamePlayForNoteEditMode(int fastValue, float bar)
    {
        if (playBMS == null)
        {
            Debug.LogError("[func]BeginGamePlay -> playBMS is null.");
            return;
        }

        // 음악 재생에 문제가 있을 경우 실행X
        if (PlayBGMSound(bgmFileName) == false)
        {
            Debug.LogError("[func]BeginGamePlay -> PlayBGMSound() return false.");
            return;
        }

        // 빨리감기모드일 경우 배속과 마디를 설정
        if (IsFastWindingMode)
        {
            Time.timeScale = fastValue;
            BGMPlayer.pitch = fastValue;

            windingBar = (int)bar;
        }

        ResetMusicPlayData();
        decimal checkTime = BeatCheckTimeCaculator(playBMS.bpm);
        StartCoroutine(BeatChecker(checkTime));
    }

    #endregion
}
