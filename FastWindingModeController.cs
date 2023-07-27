using UnityEngine;
using UnityEngine.UI;

// Edit용 빨리감기 모드 관련 코드
public class FastWindingModeController : MonoBehaviour
{
    [Range(1, 5)]
    public int timeSpeed = 1;      // 빨리감기 할 속도

    float selectBar = 0;  // selectBar의 마디까지 빨리감기를 진행

    Slider playControlSlider = null;
    Toggle fastModeTooggle = null;

    public Text barText;
    public Text beatText;

    public Text sliderBarValueText;  // 슬라이더 thumb에 달린 text

    private void Start()
    {
        playControlSlider = GetComponentInChildren<Slider>();
        fastModeTooggle = GetComponentInChildren<Toggle>();

        selectBar = (int)playControlSlider.value;
        
        if(sliderBarValueText != null)
        {
            sliderBarValueText.text = selectBar.ToString();
        }
    }

    private void Update()
    {
        // 현재 진행중인 마디와 비트를 표시
        if(barText != null && beatText != null)
        {
            barText.text = string.Format("Bar : {0}", GameMusicPlayManager.Instance.playBar.ToString());
            beatText.text = string.Format("Beat : {0}", GameMusicPlayManager.Instance.playBeat.ToString());
        }
    }

    public void ClickPlayButton()
    {
        GameMusicPlayManager.Instance.BeginGamePlayForNoteEditMode(timeSpeed, selectBar);
    }

    #region Slider

    // 빨리감기를 진행할 마디를 설정하는 슬라이더
    public void SetSelectBarValue()
    {
        if (playControlSlider == null)
        {
            Debug.LogError("[func]SetSelectBarValue -> playControlSlider is null");
            return;

        }

        selectBar = (int)playControlSlider.value;

        if (sliderBarValueText != null)
        {
            sliderBarValueText.text = selectBar.ToString();
        }
    }

    #endregion

    #region Toggle

    // 빨리감기 모드를 적용할 지 여부를 선택하는 토글 버튼
    public void SetFastModeToggle()
    {
        if (fastModeTooggle == null)
        {
            Debug.LogError("[func]SetFastModeToggle -> fastModeTooggle is null");
            return;
        }

        GameMusicPlayManager.Instance.IsFastWindingMode = fastModeTooggle.isOn;
    }

    #endregion
}
