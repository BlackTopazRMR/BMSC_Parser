using UnityEngine;

// 비트 이벤트에 반응할 오브젝트는 반드시 인터페이스 상속 받아서 사용.(상속받지 않을 시 이벤트 등록 불가)
public interface INoteBeatListener
{
    void OnEvent(CHANNEL_TYPE channelType, NOTE_BEAT_EVENT eventType, Component sender, object param = null);
}
