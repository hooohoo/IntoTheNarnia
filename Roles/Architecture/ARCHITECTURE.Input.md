# 입력 시스템

> 예시의 `{Feature}`는 실제 기능명으로 치환. 실제 네이밍은 `/feature`·`/input` 커맨드의 "최근 참고 코드 자동 탐색" 결과 사용. `Assets/@Project/01.Scripts/Manager/Manager/InputManager/`

## 1. 아키텍처 3계층

```
[저수준] InputManager.InputLoopAsync — 터치/마우스 감지 → 제스처 판정
           ↓
[중수준] IInputHandler (Strategy)    — 상태별 입력 처리, Raycast
           ↓
[고수준] IInputEventReceiver         — UI 컴포넌트가 이벤트 수신
```

## 2. InputManager 구조

```
InputManager : Singleton<InputManager>, IManager (partial 2파일)
├── .cs    — 입력 루프, 제스처 판정
│   ├── currentHandler: IInputHandler
│   ├── touchStates: Dict<int, TouchState>
│   ├── cts / doubleTapCts / scrollPinchEndCts  — CancellationTokenSource
│   ├── SetHandler(handler) → 루프 자동 시작
│   └── ClearHandler() → 루프 자동 정지
│
└── .Event — IInputEventReceiver 브로드캐스트
    ├── RegisterReceiver() / UnregisterReceiver()
    └── BroadcastInputEvent() / BroadcastPinchEvent()
```

**InputLoopAsync**: `while → Pinch(2터치) / Touch / Mouse 분기 → await UniTask.Yield(Update)`

## 3. IInputHandler

```
IInputHandler <<interface>>
  OnTap / OnDoubleTap
  OnLongPressStart / OnLongPressing / OnLongPressEnd
  OnDragStart / OnDragging / OnDragEnd
  OnPinchStart / OnPinching / OnPinchEnd
  OnClear
```

```
InputHandlerBase <<abstract>>  — RaycastUI/Raycast3D/CreateEventData 공통 유틸
├── StyleRoomInputHandler      — UI + 수평/수직 드래그 분리
└── LobbyInputHandler          — UI 전용
```

## 4. IInputEventReceiver

```
IInputEventReceiver <<interface>>
  OnInputEvent(InputEvent, InputEventData)
  OnPinchEvent(InputEvent, PinchData)
```

UI에서 `RegisterReceiver` → Handler가 `BroadcastInputEvent` → 모든 Receiver 수신.

## 5. 제스처 판정

| 제스처 | 판정 기준 | Handler 메서드 |
|--------|-----------|---------------|
| **Tap** | 드래그 미만 + 롱프레스 미만 | `OnTap()` |
| **DoubleTap** | 0.3초 내 50px 이내 | `OnDoubleTap()` |
| **Drag** | totalDelta > 10px | `OnDragStart/Dragging/DragEnd()` |
| **LongPress** | duration ≥ 0.5s | `OnLongPressStart/Pressing/End()` |
| **Pinch** | touchCount == 2 (에디터: 마우스 휠) | `OnPinchStart/Pinching/PinchEnd()` |

## 6. 데이터 구조체

| 구조체 | 주요 필드 |
|--------|----------|
| **InputData** | touchId, position, startPosition, delta, totalDelta, duration, direction(DragDirection) |
| **PinchData** | centerPosition, distance, deltaDistance, scale, startDistance |
| **InputEventData** | InputData + hitObject (Raycast 결과) |
| **DragDirection** | None, Up, Down, Left, Right |

## 7. 구현 가이드 — 새 InputHandler 추가

### Step 1: Handler 시그니처 (필요한 콜백만 override)

```csharp
public class {Feature}InputHandler : InputHandlerBase
{
    [Singleton(typeof(InputManager))] private InputManager inputManager;
    private GameObject dragTarget;

    public {Feature}InputHandler() { InjectUtil.InjectSingleton(this); }

    public override void OnTap(InputData data)
    {
        GameObject hit = RaycastUI(data.position);
        inputManager.BroadcastInputEvent(InputEvent.Tap, CreateEventData(data, hit));
    }

    public override void OnDragStart(InputData data)
    {
        dragTarget = RaycastUI(data.startPosition);
        inputManager.BroadcastInputEvent(InputEvent.DragStart, CreateEventData(data, dragTarget));
    }

    // OnDragging / OnDragEnd / OnClear도 동일 패턴 — dragTarget으로 CreateEventData + Broadcast, End/Clear에서 null 처리

    public override void OnClear() { dragTarget = null; }
}
```

`InputHandlerBase` 제공 유틸: `RaycastUI(Vector2)` / `Raycast3D(Vector2, Camera)` / `CreateEventData(InputData, GameObject)`.

### Step 2: GameState에서 설정

```csharp
// OnStateEnter
inputManager.SetHandler(new {Feature}InputHandler());  // 루프 자동 시작

// OnStateExit
inputManager.ClearHandler();                           // 루프 자동 정지
```

### 체크리스트

**Input 고유**
- [ ] `InputHandlerBase` 상속 (IInputHandler 직접 구현 금지)
- [ ] 생성자에서 `InjectUtil.InjectSingleton(this)` 호출
- [ ] 필요한 콜백만 `override` (사용 안 하는 것은 override 자체를 안 함)
- [ ] `RaycastUI` / `Raycast3D`로 hitObject 획득 → `BroadcastInputEvent`로 전파
- [ ] `OnDragStart`에서 `data.startPosition`으로 Raycast (시작 시점 객체 기억)
- [ ] 드래그 상태 변수(`dragTarget` 등)는 `OnClear()`에서 초기화
- [ ] GameState에서 `SetHandler` / `ClearHandler`

**컨벤션 핵심** (상세는 `Roles/Convention/`)
- [ ] `private static readonly string LogTag = "[클래스명]";` + `LogUtil` 로깅
- [ ] 비동기 메서드 `Async` 접미어 + Fire-and-Forget `.Forget()`
- [ ] `Update/FixedUpdate` 금지 — UniTask 또는 `Initialize()`
- [ ] LINQ / 람다 금지 (외부 라이브러리 제외)
- [ ] `Camera.main` 금지 → `cameraManager.MainCamera`
