using System.IO;
using System.Collections.Generic;
using UnityEngine;

// BMS 파일을 파싱하고 비트 체크가 가능하도록 재가공
public class BMSLoader : MonoBehaviour
{
    public Dictionary<string, BMS> bmsDic;

    const int BEAT_LENGTH = 32;     // 비트의 구조 형식 (32비트)

    private void Awake()
    {
        bmsDic = new();
    }

    public void BMSLoad(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("[func]BMSLoad -> fileName error.");
            return;
        }

        // 존재하는 파일인지 확인
        string dataPath = Path.Combine(Application.dataPath, "Resources/BMS", fileName);
        if (File.Exists(dataPath) == false)
        {
            Debug.LogError("[func]BMSLoad -> file not exist.");
            return;
        }

        // 파일을 로드한 후 데이터 파싱 및 저장
        BMS bms = new BMS();
        string[] lineDataArray = File.ReadAllLines(dataPath);
        foreach(string line in lineDataArray)
        {
            if (line.StartsWith("#") == false) continue;
           
            string[] data = line.Split(' ');

            // 데이터 섹션이 아니면서 헤더 데이터가 없는 경우 스킵
            if (data[0].IndexOf(":") == -1 && data.Length == 1) continue;

            // WAV 정보 파일에 대한 예외처리 (현재는 WAV 파일은 사용하지 않으므로, 추후에는 사용될 여지 있음)
            if (data[0].StartsWith("#WAV")) continue;
            

            // 헤더 섹션 처리
            switch (data[0])
            {
                case "#PLAYER":
                    bms.player = int.Parse(data[1]);
                    break;
                case "#TITLE":
                    bms.title = data[1];
                    break;
                case "#ARTIST":
                    bms.artist = data[1];
                    break;
                case "#GENRE":
                    bms.genre = data[1];
                    break;
                case "#BPM":
                    bms.bpm = float.Parse(data[1]);
                    break;
                case "#PLAYLEVEL":
                    int.TryParse(data[1], out bms.playLevel);
                    break;
                case "#RANK":
                    bms.rank = int.Parse(data[1]);
                    break;
                case "#LNTYPE":
                    bms.longNoteType = int.Parse(data[1]);
                    break;
                default:
                    // 위의 case에 해당사항이 없을 경우
                    // 데이터 섹션 처리
                    int bar = 0;
                    int.TryParse(data[0].Substring(1, 3), out bar);

                    int channel = 0;
                    int.TryParse(data[0].Substring(4, 2), out channel);

                    string noteStr = data[0].Substring(7);
                    List<int> noteDataList = GetNoteData(noteStr);
                    bms.totalNoteCount += noteDataList.Count;

                    NoteInfo note = new NoteInfo();
                    note.bar = bar;
                    note.channel = channel;
                    note.noteData = noteDataList;

                    if (note != null)
                    {
                        bms.SetNoteDataDic(note);
                    }

                    break;
            }
        }

        GameMusicPlayManager.Instance.SetPlayBmsData(bms);
    }

    // 데이터 섹션의 데이터 문자열 구조를 분할하여 각 리스트에 노트 정보 저장
    private List<int> GetNoteData(string dataStr)
    {
        if (string.IsNullOrEmpty(dataStr)) return null;

        string tempStr = dataStr;
        int addBeatLength = BEAT_LENGTH / tempStr.Length;     // 비트구조를 32비트형식으로 유지하기 위한 비트길이 
        
        List<int> noteList = new();

        // 첫마디와 중간마디만 있을경우에 의한 예외처리
        // 첫한마디만 있을경우 ex) 02 와같이 0이 생략되거나
        // 첫마디와 중간마디만 있을경우 ex) 0202 형태로 나오기 때문에 추가 분기처리 필요
        while (tempStr.Length >= 2)
        {
            int data = 0;
            int.TryParse(tempStr.Substring(0, 2), out data);

            noteList.Add(data);

            if (addBeatLength.Equals(0) == false)
            {
                for (int i = 0; i < addBeatLength - 1; ++i)
                {
                    noteList.Add(0);
                }
            }     
     
            tempStr = tempStr.Substring(2);
        }

        return noteList;
    }
}
    
