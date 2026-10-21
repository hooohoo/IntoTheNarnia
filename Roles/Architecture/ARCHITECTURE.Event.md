# Event

도메인 enum(GameStateEvent, DialogueEvent, DoorEvent, QuestEvent)과 IListener<TEnum>을 사용한다. AddListener/RemoveListener는 소유자의 생명주기에 대칭 배치하고 PostNotification으로 알린다. payload는 계약 타입으로 명시 캐스팅한다. 이벤트 순회는 독립 스냅샷으로 등록·해제와 중첩 발행을 지원한다.

[전체 구조](../../ARCHITECTURE.md)를 함께 적용한다.
