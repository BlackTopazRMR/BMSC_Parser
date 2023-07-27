using System.Collections.Generic;
using UnityEngine;

// BMS 파일의 데이터를 저장하는 클래스
public class BMS
{
    // 기본 헤더 정보
    public int player;
    public string genre;
    public string title;
    public string artist;
    public float bpm;
    public int playLevel;
    public int rank;

    // 노트 데이터
    public Dictionary<int, Dictionary<int, List<int>>> noteDataDic;     // 마디, 채널, 노트데이터

    public int totalNoteCount;      // 노트의 총 갯수
    public int longNoteType;        // 롱노트 타입

    public BMS()
    {
        noteDataDic = new();
    }

    public bool SetNoteDataDic(NoteInfo noteData)
    {
        if (noteData == null)
        {
            Debug.LogError("[func]SetNoteDataDic -> noteData is null.");
            return false;
        }

        if (noteDataDic == null)
        {
            Debug.LogError("[func]SetNoteDataDic -> noteDataDic is null.");
            return false;
        }

        if (noteDataDic.ContainsKey(noteData.bar) == false)
        {
            noteDataDic[noteData.bar] = new();
        }
        noteDataDic[noteData.bar][noteData.channel] = noteData.noteData;

        return true;
    }
}
