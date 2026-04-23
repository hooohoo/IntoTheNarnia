# 이벤트 시스템

> 예시의 `{Feature}`는 실제 기능명으로 치환. 실제 네이밍은 `/feature`·`/event` 커맨드의 "최근 참고 코드 자동 탐색" 결과 사용. `Assets/@Project/01.Scripts/Manager/Manager/EventManager/`

## 1. 구조

```
EventManager : Singleton<EventManager>, IManager
├── listenerTables: Dict<Type, object>  — 실제 List<IListener<TEnum>>
├── queueTables:    Dict<Type, object>  — 실제 Queue<(TEnum, object)>
│
├── AddListener<T>() / RemoveListener<T>()
├── PostNotification<T>(eventType, param)  — 역순 순회, null/비활성 스킵, 예외 격리
├── Enqueue<T>() / ProcessQueue<T>()       — FIFO 지연 발행
└── Init() / Release()

IListener<TEventType> where TEventType : Enum
└── void OnEvent(TEventType eventType, object param = null)
```

**PostNotification 설계 포인트:**
- 역순 순회 → 순회 중 RemoveListener 안전
- `MonoBehaviour.activeInHierarchy` 체크 → 비활성 UI 스킵
- try-catch → 한 리스너 예외가 다른 리스너를 차단하지 않음

## 2. 이벤트 Enum 목록

경로: `Assets/@Project/01.Scripts/Enum/Event/`

| Enum | 주요 값 (요약) |
|------|---------------|
| **GameStateEvent** | StateEntered / StateExited / StatePaused / StateResumed |
| **SceneEvent** | LoadStarted / LoadCompleted / SceneReady |
| **InputEvent** | Tap / DoubleTap / LongPress* / Drag* / Pinch* 11종 |
| **StyleRoomEvent** | MainCategoryChanged / ItemSelected / PurchaseRequested / Undo·Redo·Refresh 등 17종 |
| **CreateAvatarEvent** | MainCategoryChanged / ItemSelected / Undo·Redo·Save·Refresh / HistoryStateChanged 등 11종 |
| **CostumeEvent** | SetAllCostume / SetCostume / ClearCostume |
| **CaptureEvent** | Capture |
| **TTSEvent** | AudioChunkReceived / StopRequested / EmotionChanged / Playback* |
| **FlutterEvent** | LectureComplete / GNBChange / GNBClose 등 |
| **WebEvent** | ViewportChanged |
| **LayoutEvent** | ModeChanged / ScalerUpdate |

## 3. ErrorEventManager (API 에러 전용)

```
ErrorEventManager : Singleton<ErrorEventManager>, IManager
├── listenerTable: Dict<int, List<IErrorListener>>    — int 에러코드 키
└── PostNotification(ApiErrorResponse) → errorCode 매칭 구독자만 호출

흐름: APIManager 에러 → ApiErrorResponse 파싱 → ErrorEventManager → 해당 코드 구독자
```

## 4. 구현 가이드 — 새 이벤트 추가

### Step 1: Enum 정의

```csharp
// Assets/@Project/01.Scripts/Enum/Event/{Feature}Event.cs
public enum {Feature}Event
{
    ItemAdded,
    ItemRemoved,
    ItemEquipped,
}
```

### Step 2: 발행 측 (값 타입 파라미터 2개 이상 → class로 묶고 캐싱)

```csharp
public class ItemEventData { public string itemId; public int quantity; }

private ItemEventData itemEventData = new ItemEventData();  // 필드에 1회 생성, 재사용

private void OnItemAdded(string id, int qty)
{
    itemEventData.itemId   = id;
    itemEventData.quantity = qty;
    eventManager.PostNotification({Feature}Event.ItemAdded, itemEventData);
}
```

### Step 3: 구독 측 시그니처

```csharp
public class UI_{Feature}ItemList : UI_Scene, IListener<{Feature}Event>
{
    [Singleton(typeof(EventManager))] private EventManager eventManager;

    public override void Init()    { InjectUtil.InjectSingleton(this); eventManager.AddListener(this); }
    public override void Release() { eventManager.RemoveListener(this); base.Release(); }

    public void OnEvent({Feature}Event e, object p = null)
    {
        switch (e)
        {
            case {Feature}Event.ItemAdded:
                var data = (ItemEventData)p;
                RefreshList(data.itemId, data.quantity);
                break;
            // 나머지 케이스는 동일 패턴
        }
    }
}
```

### 체크리스트

**Event 고유**
- [ ] Enum 파일 생성 (`Assets/@Project/01.Scripts/Enum/Event/`)
- [ ] 값 타입 파라미터 2개 이상이면 **class로 묶고 필드에 미리 생성하여 재사용** (매번 new 금지)
- [ ] 발행: `eventManager.PostNotification(EventType, data)`
- [ ] 구독: `IListener<T>` 구현 + Init에서 `AddListener(this)` + Release에서 `RemoveListener(this)`

**컨벤션 핵심** (상세는 `Roles/Convention/`)
- [ ] `private static readonly string LogTag = "[클래스명]";` + `LogUtil` 로깅
- [ ] 비동기 메서드 `Async` 접미어 + Fire-and-Forget `.Forget()`
- [ ] `Update/FixedUpdate` 금지 — UniTask 또는 `Initialize()`
- [ ] LINQ / 람다 금지 (외부 라이브러리 제외)
- [ ] `Camera.main` 금지 → `cameraManager.MainCamera`
