# GameState 상태 머신

> 예시의 `{Feature}`는 실제 기능명으로 치환. 실제 네이밍은 `/feature`·`/state` 커맨드의 "최근 참고 코드 자동 탐색" 결과 사용. `Assets/@Project/01.Scripts/Manager/Manager/GameStateManager/`

## 1. 구조

```
IGameState <<interface>>
├── GameStateType StateType { get; }
├── OnStateEnter()   — 진입 (입력/카메라/UI 활성화)
├── OnStateExit()    — 종료 (리소스 정리)
├── OnStatePause()   — 일시정지 (팝업 등)
├── OnStateResume()  — 재개
└── OnStateUpdate()  — 수동 업데이트

GameStateManager : Singleton<GameStateManager>, IManager
├── states: Dict<GameStateType, IGameState>
├── currentState / previousState
├── RegisterState() / ChangeState() / ReturnToPreviousState()
├── PauseCurrentState() / ResumeCurrentState()
└── ClearAllStates() — 씬 전환 시 전체 정리
```

## 2. ChangeState 시퀀스

```
1. currentState.OnStateExit() + PostNotification(StateExited)
2. previousState = currentState, currentState = newState
3. currentState.OnStateEnter() + PostNotification(StateEntered)
```

## 3. 현재 상태 목록

```csharp
public enum GameStateType
{
    None, Lobby, StyleRoom,
    WebStyleRoom, WebCreateAvatar, WebTalkMotion
}
```

| 상태 | 씬 | InputHandler | Screen |
|------|-----|--------------|--------|
| Lobby | Main | LobbyInputHandler | LobbyScreen |
| StyleRoom | Main | StyleRoomInputHandler | StyleRoomScreen |
| WebStyleRoom | WebStyleRoom | WebStyleRoomInputHandler | WebStyleRoomScreen |
| WebCreateAvatar | WebCreateAvatar | WebCreateAvatarInputHandler | WebCreateAvatarScreen |
| WebTalkMotion | WebTalkMotion | (없음) | WebTalkMotionScreen |

## 4. Screen 시스템

> `Assets/@Project/01.Scripts/Scene/`

`IScreenInitializer`: `Initialize()` / `Release()`. State와 Screen은 1:1 매핑. Screen은 UI 표시/데이터 로드를 담당하고, State는 입력/상태 전환을 담당.

### State ↔ Screen 연결

```csharp
// Screen.Initialize()에서 State에 자신을 주입
public void Initialize()
{
    InjectUtil.InjectSingleton(this);
    gameStateManager.GetState<{Feature}State>(GameStateType.{Feature}).SetScreen(this);
}

// State.OnStateEnter()에서 Screen 사용
public void OnStateEnter()
{
    inputManager.SetHandler(inputHandler);
    screen.ShowUI();
    screen.LoadDataAsync().Forget();
}
```

### 씬별 등록

| 씬 파일 | State | Screen | 초기 상태 |
|---------|-------|--------|----------|
| Main.cs | Lobby, StyleRoom | LobbyScreen, StyleRoomScreen | Lobby |
| WebStyleRoom.cs | WebStyleRoomState | WebStyleRoomScreen | WebStyleRoom |
| WebCreateAvatar 씬 | WebCreateAvatarState | WebCreateAvatarScreen | WebCreateAvatar |
| WebTalkMotion.cs | WebTalkMotionState | WebTalkMotionScreen | WebTalkMotion |

### 자체 씬 공통 초기화 패턴

```csharp
// 1. Addressable 프리로드
await addressableManager.PreloadAsync<GameObject>(label);
// 2. 기존 상태 정리
gameStateManager.ClearAllStates();
// 3. State 등록 + Screen 초기화
gameStateManager.RegisterState(new XxxState());
screen.Initialize();
// 4. 상태 전환
gameStateManager.ChangeState(GameStateType.Xxx);
```

Screen 내부: `ObjectManager.SpawnAtPrefabTransform`으로 아바타/카메라 Spawn, `DataManager` 병렬 로드, `CostumeEvent.SetAllCostume`으로 초기 코스튬 적용, `IErrorListener` 구현(에러 핸들링).

## 5. 구현 가이드 — 새 GameState 추가

### Step 1: State 시그니처 골격

```csharp
public class {Feature}State : IGameState
{
    [Singleton(typeof(InputManager))] private InputManager inputManager;
    [Singleton(typeof(UIManager))] private UIManager uiManager;

    private {Feature}InputHandler inputHandler;
    private {Feature}Screen screen;

    public GameStateType StateType => GameStateType.{Feature};

    public {Feature}State()
    {
        InjectUtil.InjectSingleton(this);
        inputHandler = new {Feature}InputHandler();
    }

    public void SetScreen({Feature}Screen s) { screen = s; }

    public void OnStateEnter()  { inputManager.SetHandler(inputHandler); screen.ShowUI(); /* + 데이터 로드 .Forget() */ }
    public void OnStateExit()   { inputManager.ClearHandler(); screen.CloseUI(); }
    public void OnStatePause()  { inputManager.ClearHandler(); }
    public void OnStateResume() { inputManager.SetHandler(inputHandler); }
    public void OnStateUpdate() { }
}
```

### Step 2: `GameStateType.cs`에 enum 값 추가

```csharp
public enum GameStateType { ..., {Feature} }
```

### Step 3: Screen 시그니처 골격

```csharp
public class {Feature}Screen : IScreenInitializer
{
    [Singleton(typeof(GameStateManager))] private GameStateManager gameStateManager;
    [Singleton(typeof(UIManager))] private UIManager uiManager;
    [Singleton(typeof(DataManager))] private DataManager dataManager;

    public {Feature}Screen() { InjectUtil.InjectSingleton(this); }

    public void Initialize()
    {
        gameStateManager.GetState<{Feature}State>(GameStateType.{Feature}).SetScreen(this);
    }

    public void Release() { /* 리소스 정리 */ }
}
```

### Step 4: 씬 초기화에서 등록

```csharp
gameStateManager.RegisterState(new {Feature}State());

var screen = new {Feature}Screen();
screen.Initialize();

gameStateManager.ChangeState(GameStateType.{Feature});
```

### 체크리스트

**State 고유**
- [ ] `IGameState` 5개 메서드 전부 구현
- [ ] `GameStateType` enum에 값 추가
- [ ] 생성자에서 `InjectUtil.InjectSingleton(this)` + InputHandler 생성
- [ ] OnStateEnter: `SetHandler` + 데이터 로드(`.Forget()`) + UI 표시
- [ ] OnStateExit: `ClearHandler` + UI 닫기 + 리소스 해제
- [ ] OnStatePause: `ClearHandler` / OnStateResume: `SetHandler`
- [ ] Screen: `IScreenInitializer` 구현, `Initialize()`에서 `State.SetScreen(this)` 연결
- [ ] 씬 초기화 파일에서 `RegisterState` + `Screen.Initialize()` 호출 (자체 씬이면 `PreloadAsync` → `ClearAllStates` → `ChangeState` → `SceneReady` 순서)

**컨벤션 핵심** (상세는 `Roles/Convention/`)
- [ ] `private static readonly string LogTag = "[클래스명]";` + `LogUtil` 로깅
- [ ] 비동기 메서드 `Async` 접미어 + Fire-and-Forget `.Forget()`
- [ ] `Update/FixedUpdate` 금지 — UniTask 또는 `Initialize()`
- [ ] LINQ / 람다 금지 (외부 라이브러리 제외)
- [ ] `Camera.main` 금지 → `cameraManager.MainCamera`
