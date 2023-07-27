using System.Collections.Generic;
using UnityEngine;

// Beat Event Listener의 등록, 해제, 이벤트 발생등 이벤트처리와 관련된 전반적인 기능을 관리 
public class BeatEventManager : MonoBehaviour
{
    private static BeatEventManager instance = null;

    public static BeatEventManager Instance
    {
        get { return instance; }
    }

    // 이벤트 dic 목록
    Dictionary<CHANNEL_TYPE, Dictionary<NOTE_BEAT_EVENT, INoteBeatListener>> noteBeatListenerDic = new();

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
    }

    // 이벤트 리스트 추가
    public void AddNoteBeatListener(CHANNEL_TYPE channelType, NOTE_BEAT_EVENT eventType, INoteBeatListener listener)
    {
        if(noteBeatListenerDic.ContainsKey(channelType) == false)
        {
            noteBeatListenerDic[channelType] = new(); 
        }

        noteBeatListenerDic[channelType][eventType] = listener;
    }

    // 이벤트 전송
    public void PostNoteBeatEvent(CHANNEL_TYPE channelType, NOTE_BEAT_EVENT eventType, Component sender, object param = null)
    {
        if (noteBeatListenerDic.ContainsKey(channelType))
        {
            if(noteBeatListenerDic[channelType].ContainsKey(eventType))
            {
                noteBeatListenerDic[channelType][eventType].OnEvent(channelType, eventType, sender, param);
            }
        }
    }

    // 해당 이벤트 타입 dic에서 제거
    public void RemoveNoteBeatEvent(CHANNEL_TYPE channelType)
    {
        noteBeatListenerDic.Remove(channelType);
    }
}
