using UnityEngine;

// 선택한 BMS 파일을 내부에서 Load 해주는 역할
public class BMSSelector : MonoBehaviour
{
    public MUSIC_LIST playMusic = MUSIC_LIST.NONE;

    BMSLoader bmsLoader;

    private void Start()
    {
        bmsLoader = gameObject.AddComponent<BMSLoader>();

        if(playMusic != MUSIC_LIST.NONE)
        {
            // bms 파일 로드
            string music_name = string.Concat(playMusic.ToString(), ".bms");
            bmsLoader.BMSLoad(music_name);

            // bgm 파일 정보 입력
            GameMusicPlayManager.Instance.SetBGM_FileName(playMusic.ToString());
        }
    }
}
