# CHALK 4.0 - Project Architecture

> FacadeManager 기준 프로젝트 아키텍처 구조 문서
> 기준 경로: `Assets/@Project/01.Scripts/`

---

## 0. 시스템 개요도

> 문서를 열자마자 전체 아키텍처를 한눈에 볼 수 있는 **계층형 개요도**.
> FacadeManager가 5개 레이어의 18개 매니저를 수집 → 정렬 → 초기화한다.

```
┌────────────────────── Presentation Layer ──────────────────────┐
│   UIManager (Scene/Popup/Sub)   ·   LoadingManager            │
│   UILayoutManager (가로/세로 레이아웃 전환)                    │
├────────────────────── Game Logic Layer ────────────────────────┤
│   GameStateManager   ·   HexGridManager   ·   InputManager    │
├────────────────────── Core Service Layer ──────────────────────┤
│   EventManager   ·   ErrorEventManager   ·   GameSceneManager │
│   CameraManager                                               │
├────────────────────── Data & Resource Layer ───────────────────┤
│   DataManager   ·   AddressableManager   ·   ObjectManager    │
│   APIManager    ·   SoundManager         ·   FactoryManager   │
├────────────────────── Platform Layer ─────────────────────────┤
│   PlatformManager (플랫폼 판별 · Provider 팩토리)             │
│   FlutterMessageManager (Flutter ↔ Unity Bridge)              │
│   WebMessageManager (Web ↔ Unity Bridge)                      │
└───────────────────────────────────────────────────────────────┘
                              ▲
                       FacadeManager
                  (수집 → 정렬 → 초기화)
```

---

## 1. 전체 아키텍처 계층도

```
┌── Base Classes ──────────────────────────────────────────────────────┐
│                                                                      │
│  MonoBehaviour (Unity)                                               │
│       ├──> Singleton<T>     -static T instance / +static T Instance  │
│       │        +static SetupInstance()                               │
│       │                                                              │
│       └──> FacadeManager    -List<IManager> managers                 │
│                -Awake() → CollectManagers() → SortByDependency()     │
│                -InitAll() → ReleaseAll() → OnApplicationQuit()       │
│                                                                      │
│  IManager <<interface>>     +Init() / +Release()                     │
└──────────────────────────────────────────────────────────────────────┘

┌── Singleton<T> : IManager  (18개 매니저) ────────────────────────────┐
│                                                                      │
│  EventManager            AddressableManager       FactoryManager     │
│  ErrorEventManager       CameraManager            InputManager*      │
│  PlatformManager         APIManager*              DataManager*       │
│  ObjectManager           UIManager*               LoadingManager*    │
│  FlutterMessageManager*  WebMessageManager*       GameSceneManager*  │
│  GameStateManager        SoundManager             HexGridManager     │
│  UILayoutManager                                                     │
│                                                                      │
│  FacadeManager ──manages──> 위 18개 IManager                        │
└──────────────────────────────────────────────────────────────────────┘

┌── Partial Class 분할 (* 표시 매니저) ────────────────────────────────┐
│  DataManager          : 10파일 .cs / .KnowledgeGraph / .Avatar /    │
│                                .Friend / .GNB / .Notice /           │
│                                .Dashboard / .Topic /                │
│                                .CreateAvatar / .StyleRoom           │
│  APIManager           : 4파일  .cs / .Request / .Response / .Token  │
│  InputManager         : 2파일  .cs / .Event                         │
│  LoadingManager       : 2파일  .cs / .FakeLoading                   │
│  FlutterMessageManager: 3파일  .cs / .Send / .Receive               │
│  GameSceneManager     : 3파일  .cs / .Transition / .ReturnData      │
│  WebMessageManager    : 3파일  .cs / .Send / .Receive               │
│  UIManager            : 2파일  .cs / .Layout                        │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 2. 매니저 의존성 그래프

`[Singleton(typeof(...))]` 어트리뷰트 기반 의존성 4-Tier 구조.
`DependencyResolver.Sort()`가 Kahn's Algorithm으로 위상 정렬하여 초기화 순서를 결정한다.

```
┌── Tier 0 (의존성 없음) ──────────────────────────────────────────────┐
│  EventManager       AddressableManager   FactoryManager              │
│  ErrorEventManager  CameraManager        InputManager                │
│  PlatformManager                                                     │
└──────────────────────────────────────────────────────────────────────┘
        ▲ [Singleton]             ▲ [Singleton]
        │                         │
┌── Tier 1 (Tier 0에 의존) ────────────────────────────────────────────┐
│  DataManager ─────────────────> AddressableManager                   │
│  ObjectManager ───────────────> AddressableManager                   │
│  LoadingManager ──────────────> AddressableManager                   │
│  FlutterMessageManager ──────> EventManager                          │
│  GameStateManager ────────────> EventManager                         │
│  APIManager ──────────────────> PlatformManager, ErrorEventManager   │
│  UILayoutManager ─────────────> EventManager                         │
└──────────────────────────────────────────────────────────────────────┘
        ▲ [Singleton]
        │
┌── Tier 2 (Tier 0~1에 의존) ─────────────────────────────────────────┐
│  UIManager ───────────────────> AddressableManager, UILayoutManager  │
│  SoundManager ────────────────> FlutterMessageManager                │
│  HexGridManager ──────────────> DataManager, EventManager            │
│  GameSceneManager ────────────> EventManager, AddressableManager,    │
│                                 LoadingManager, UIManager            │
└──────────────────────────────────────────────────────────────────────┘
        ▲ [Singleton]
        │
┌── Tier 3 (Tier 0~2에 의존) ─────────────────────────────────────────┐
│  WebMessageManager ──────────> EventManager, GameSceneManager        │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 2-1. 매니저 상호작용 맵

> 의존성 그래프(Section 2)와 별개로, **런타임에 매니저끼리 어떤 메커니즘으로 소통하는지** 시각화.
> 3가지 소통 방식을 화살표 스타일로 구분한다.

```
┌─────────────────────────────────────────────────────────────────┐
│  ① [Singleton] 직접 참조  ────>                                 │
│                                                                 │
│  GameStateManager  ────> EventManager                           │
│  GameSceneManager  ────> EventManager                           │
│  GameSceneManager  ────> AddressableManager                     │
│  GameSceneManager  ────> LoadingManager                         │
│  GameSceneManager  ────> UIManager                              │
│  FlutterMessageMgr ────> EventManager                           │
│  HexGridManager    ────> EventManager                           │
│  HexGridManager    ────> DataManager                            │
│  SoundManager      ────> FlutterMessageManager                  │
│  APIManager        ────> PlatformManager                        │
│  APIManager        ────> ErrorEventManager                      │
│  UILayoutManager   ────> EventManager                           │
│  UIManager         ────> UILayoutManager                        │
├─────────────────────────────────────────────────────────────────┤
│  ② EventManager 이벤트  · · ·>                                  │
│                                                                 │
│  GameStateManager  · · ·> UIManager       (GameStateEvent)      │
│  GameStateManager  · · ·> CameraManager   (GameStateEvent)      │
│  GameStateManager  · · ·> InputManager    (GameStateEvent)      │
│  FlutterMessageMgr · · ·> GameStateManager(FlutterEvent)        │
│  InputManager      · · ·> GameStateManager(InputEvent)          │
│  WebMessageManager · · ·> UILayoutManager (WebEvent)            │
│  UILayoutManager   · · ·> UIManager      (LayoutEvent.ScalerUpdate)│
│  UILayoutManager   · · ·> UI Components  (LayoutEvent.ModeChanged)│
├─────────────────────────────────────────────────────────────────┤
│  ③ Addressable 리소스  ════>                                    │
│                                                                 │
│  DataManager     ════> AddressableManager                       │
│  ObjectManager   ════> AddressableManager                       │
│  UIManager       ════> AddressableManager                       │
│  LoadingManager  ════> AddressableManager                       │
└─────────────────────────────────────────────────────────────────┘
```

| 화살표 스타일 | 소통 방식 | 설명 |
|:---:|-----------|------|
| **실선 (→)** | `[Singleton]` 직접 참조 | Init 시 어트리뷰트 기반 주입, 동기 호출 |
| **점선 (-→)** | `EventManager` 이벤트 | `PostNotification()` → `OnEvent()` 간접 통신 |
| **이중선 (⇒)** | `AddressableManager` 리소스 | 프리팹/에셋 비동기 로드 요청 |

---

## 3. 초기화 / 해제 시퀀스

```
  Unity          FacadeManager         DependencyResolver      IManager[]
    │                  │                       │                    │
    │  ═══ 초기화 (Awake) ═════════════════════════════════════════ │
    │                  │                       │                    │
    ├── Awake() ─────>│                       │                    │
    │                  ├── CollectManagers()   │                    │
    │                  │   18개 Singleton<T>   │                    │
    │                  │   인스턴스 수집        │                    │
    │                  │                       │                    │
    │                  ├── Sort(managers) ────>│                    │
    │                  │                       ├─ BuildDependencyGraph()
    │                  │                       │  [Singleton] 리플렉션
    │                  │                       ├─ TopologicalSort()
    │                  │                       │  Kahn's Algorithm
    │                  │                       ├─ 순환 의존성 감지
    │                  │<── sorted List<IManager> ─┤               │
    │                  │                       │                    │
    │                  ├── LogInitOrder()      │                    │
    │                  │                       │                    │
    │                  │  [loop i = 0 → count-1]                   │
    │                  ├── managers[i].Init() ─────────────────────>│
    │                  │                       │  InjectUtil.       │
    │                  │                       │  InjectSingleton() │
    │                  │                       │                    │
    │  ═══ 해제 (OnApplicationQuit) ═══════════════════════════════ │
    │                  │                       │                    │
    ├── OnAppQuit() ─>│                       │                    │
    │                  ├── ReleaseAll()        │                    │
    │                  │  [loop i = count-1 → 0  역순]             │
    │                  ├── managers[i].Release() ──────────────────>│
    │                  │                       │  캐시 정리,        │
    │                  │                       │  이벤트 해제       │
```

---

## 3-1. 런타임 데이터 흐름도

> 사용자의 터치부터 화면 업데이트까지, 실제 데이터가 시스템을 관통하는 경로를 시각화한다.

### A. 사용자 입력 흐름

```
[User Touch] ──터치/드래그/핀치──> [InputManager] ──InputData/PinchData──> [IInputHandler]
     (입력 감지·분류)        (Strategy 위임)
                                        │
                                  OnTap/OnDrag/...
                                        ▼
  [UIManager] <──OnEvent()── [EventManager] <──PostNotification()── [GameStateManager] <──ChangeState()── [GameState Logic]
  (화면 업데이트)       (GameStateEvent 발행)          (상태 전환 판단)              (상태별 처리)
```

### B. 데이터 로딩 흐름

```
[Flutter DataString] ──JSON 문자열──> [FlutterMessageMgr] ──파싱·Provider 설정──> [APIManager]
                                        (메시지 파싱)                    (HTTP 요청·JWT)
                                                                              │
                                                                          API 응답
                                                                              ▼
                            [ObjectManager] <──Spawn 요청── [HexGridManager] <──캐시 조회── [DataManager]
                            (오브젝트 Spawn)          (그리드 생성)             (캐시 저장)
```

---

## 4. 이벤트 시스템 구조

```
┌── EventManager ──────────────────────────────────────────────────────┐
│                                                                      │
│  listenerTables                      queueTables                     │
│  Dict<Type, List<IListener<T>>>      Dict<Type, Queue<(T, obj)>>    │
│                                                                      │
│  Public API:                                                         │
│    AddListener<T>(listener)  ──────> listenerTables                  │
│    RemoveListener<T>(listener) ────> listenerTables                  │
│    PostNotification<T>(e, param) ──> listenerTables ──> IListener   │
│    Enqueue<T>(e, param) ───────────> queueTables                     │
│    ProcessQueue<T>() ──────────────> queueTables ──> Post            │
│    ClearQueue<T>()                                                   │
│                                                                      │
│  IListener<TEventType>                                               │
│    OnEvent(eventType, param)                                         │
└──────────────────────────────────────────────────────────────────────┘
                              │
                    TEnum 타입 파라미터
                              │
┌── 이벤트 Enum 타입 ─────────────────────────────────────────────────┐
│  SceneEvent     : LoadStarted, LoadCompleted,                      │
│                   TransitionProgressUpdated, SceneReady            │
│  GameStateEvent : StateEntered, StateExited, StatePaused,           │
│                   StateResumed                                      │
│  FlutterEvent   : LectureCompleteMission, LectureCompleteTest,     │
│                   GNBEnterFriendPlanet, GNBChangeTeacher,           │
│                   GNBChangeRoadmap, GNBClose* (12종)                │
│  InputEvent     : Tap, LongPressStart/Pressing/End,                │
│                   DragStart/Dragging/HorizontalDragging/            │
│                   VerticalDragging/DragEnd,                         │
│                   PinchStart/Pinching/PinchEnd                      │
│  HexGridEvent   : GridLoaded, GridCleared, BlockClicked,           │
│                   BlockHighlighted, PathFound                       │
│  StyleRoomEvent : MainCategoryChanged, SubCategorySelected,        │
│                   SubCategoryInitialized, CreateSlotRequested,      │
│                   SlotRecycled, ItemSelected, EquippedItemsChanged, │
│                   PurchaseRequested, SubCategoryBadgeChanged,       │
│                   PurchaseStateChanged, SaveRequested,              │
│                   HistoryStateChanged, Undo, Redo, Refresh,        │
│                   PageChanged, PageDataLoaded                       │
│  CreateAvatarEvent : MainCategoryChanged, SubCategorySelected,     │
│                   SubCategoryInitialized, CreateSlot, SlotRecycled, │
│                   ItemSelected, Undo, Redo, Save, Refresh,         │
│                   HistoryStateChanged                               │
│  Depth1_5CameraEvent : InitialTransitionComplete, FocusStarted,    │
│                   ZoomInStarted, NodeTransitionStarted,             │
│                   NodeTransitionComplete                            │
│  WebCreateAvatarEvent : Complete                                    │
│  CostumeEvent   : SetAllCostume, SetCostume, ClearCostume          │
│  WebEvent       : ViewportChanged                                  │
│  LayoutEvent    : ModeChanged                                      │
└─────────────────────────────────────────────────────────────────────┘
```

**이벤트 발행 흐름:**
1. `PostNotification<TEnum>()` 호출
2. 리스너 목록을 역순 순회 (null 체크)
3. `MonoBehaviour` 리스너는 `activeInHierarchy` 확인
4. `try-catch`로 개별 리스너 오류 격리
5. `OnEvent(eventType, param)` 호출

### 4-1. ErrorEventManager (API 에러 이벤트)

> EventManager와 별도로, API 에러 전용 이벤트 시스템.
> **int 키(에러 코드)** 기반 구독으로 특정 에러 코드에 관심 있는 리스너만 호출한다.

```
┌── ErrorEventManager ─────────────────────────────────────────────────┐
│                                                                      │
│  listenerTable                                                       │
│  Dict<int, List<IErrorListener>>                                    │
│                                                                      │
│  Public API:                                                         │
│    AddListener(int errorCode, listener) ──> listenerTable            │
│    RemoveListener(int errorCode, listener) ──> listenerTable         │
│    PostNotification(ApiErrorResponse) ──> errorCode로 리스너 조회    │
│                                                                      │
│  IErrorListener                                                      │
│    OnErrorEvent(ApiErrorResponse error)                              │
└──────────────────────────────────────────────────────────────────────┘
                              │
                    int 에러 코드 키
                              │
┌── ApiErrorCodes (const int) ────────────────────────────────────────┐
│  Avatar.AvatarNotFound = 123201                                     │
│  Purchase.ItemNotFound = 123001                                     │
│  Purchase.ItemNotOnSale = 123002                                    │
│  Purchase.AlreadyOwned = 123106  ...                                │
└─────────────────────────────────────────────────────────────────────┘
```

**에러 이벤트 발행 흐름:**
1. `APIManager` HTTP 에러 수신 → `NotifyApiError(responseBody)`
2. 응답 바디를 `ApiErrorResponse` 구조체로 파싱
3. `ErrorEventManager.PostNotification(apiErrorResponse)` 호출
4. `apiErrorResponse.ErrorCode`로 해당 에러 구독자만 호출
5. 구독자의 `OnErrorEvent(error)` 실행

---

## 5. GameState 상태 머신

```
  ChangeState() ───>                ReturnToPreviousState() <───

  [*] ──> None ──┬──> Dashboard ──> Depth0 ──> Depth1 ──> Depth1_5 ──┬──> Depth2 ──auto──> Depth3
                 │       ↕ Lobby      <──        <──         <──      │                      │
                 └──> Lobby                              Depth1_5Restore                     │
                                                          (Planet→Main 복귀)      DoubleTap  │
                                                                                     ▼       │
                 StyleRoom (아바타 탭)                                         Depth2Restore  │
                                                                        DoubleTap│  Tap(블록)│
                                                                     (→Main씬)  │     ▼     │
                                                                                 └──> Depth3 ┘

  GameStateType Enum: None | Dashboard | Lobby | Depth0 | Depth1 | Depth1_5 | Depth1_5Restore
                      | Depth2 | Depth3 | Depth2Restore | StyleRoom
                      | WebDepth1 | WebDepth1_5 | WebStyleRoom | WebCreateAvatar
```

```
┌── IGameState <<interface>> ──────────────────────────────────────────┐
│  +GameStateType StateType                                            │
│  +OnStateEnter() / +OnStateUpdate() / +OnStateExit()                │
│  +OnStatePause() / +OnStateResume()                                  │
└──────────────────────────────────────────────────────────────────────┘
         ▲ implements
         ├── DashboardState
         ├── Depth0State
         ├── Depth1State
         ├── Depth1_5State
         ├── Depth1_5RestoreState
         ├── Depth2State
         ├── Depth3State
         ├── Depth2RestoreState
         ├── StyleRoomState
         ├── WebDepth1State
         ├── WebDepth1_5State
         ├── WebStyleRoomState
         └── WebCreateAvatarState

┌── GameStateManager ──────────────────────────────────────────────────┐
│  -Dict<GameStateType, IGameState> states                             │
│  -IGameState currentState / previousState                            │
│                                                                      │
│  +RegisterState(IGameState)          +HasState(type) bool            │
│  +ChangeState(GameStateType)         +GetState<T>(type) T            │
│  +UpdateCurrentState()               +ReturnToPreviousState()        │
│  +PauseCurrentState()                +ResumeCurrentState()           │
│                                                                      │
│  ──manages──> IGameState                                             │
│  · · ·posts· · ·> EventManager (GameStateEvent)                     │
└──────────────────────────────────────────────────────────────────────┘
```

**상태 전환 흐름:**
1. `ChangeState(stateType)` 호출
2. `currentState.OnStateExit()` → `PostNotification(GameStateEvent.StateExited)`
3. `previousState = currentState`
4. `currentState = newState`
5. `currentState.OnStateEnter()` → `PostNotification(GameStateEvent.StateEntered)`

---

## 5-1. 게임 흐름도 (사용자 여정)

> GameState 상태 전환을 **사용자 관점**에서 시각화. 각 상태에서 활성화되는 씬 / UI / 입력 핸들러를 표시한다.

```
                          ┌─────────┐
                     ┌───>│  Lobby  │
                     │    └────┬────┘
                     │         │ Return
                     │         ▼                                                      ┌──────────────┐
[Intro] ──> [Dashboard] ──> [Depth0] ──> [Depth1] ──> [Depth1.5] ──> [Depth2]═auto═>[Depth3]        │
                  ▲    \         ▲            ▲             ▲                           │  ▲           │
                  │     \        │  Return    │   Return    │                  DoubleTap│  │Tap(블록)  │
                  │  [StyleRoom] └────────────┘             │                           ▼  │           │
                  │   Return     │                  [Depth1_5Restore]◄──────[Depth2Restore]│           │
                  └──────────────┘                   (Planet→Main 복귀)      DoubleTap(→Main씬)       │
                                                                                          │           │
                                                                                          └───────────┘

═══> 자동 전환 (사용자 입력 없이)      ────> ChangeState() (전진)
────> ReturnToPreviousState() (복귀)
```

| 상태 | 씬 | UI | Input Handler | 비고 |
|------|-----|-----|---------------|------|
| **Intro** | Intro | - | - | 진입점 |
| **Dashboard** | Main | Dashboard | DashboardHandler | 메인 화면 |
| **Lobby** | Main | Lobby | - | 아바타 |
| **Depth0** | Main | Depth0 | Depth0Handler | 은하 뷰 |
| **Depth1** | Main | Depth1 | Depth1Handler | 노드 뷰 |
| **Depth1.5** | Main | Depth1_5 | Depth1_5Handler | 노드 확대 뷰 |
| **Depth1_5Restore** | Main | Depth1_5 | Depth1_5Handler | Planet→Main 복귀 시 카메라/노드 복원 |
| **StyleRoom** | Main | StyleRoom | StyleRoomHandler | 아바타 탭 → 스타일룸 UI + 비동기 데이터 로드 |
| **Depth2** | Planet | Depth2 | Depth2Handler | 경유 상태 (자동으로 Depth3 전환) |
| **Depth3** | Planet | Depth3 | Depth3Handler | Depth3Screen 뷰 관리 |
| **Depth2Restore** | Planet | - | Depth2RestoreHandler | DoubleTap→Main씬, Tap(블록)→Depth3 재진입 |

> **이중선 화살표(═)** = 자동 전환, **실선 화살표** = `ChangeState()` (전진), **점선 화살표** = `ReturnToPreviousState()` (복귀)

### 씬별 상태 등록

각 씬의 초기화 시점에 `GameStateManager.RegisterState()`로 해당 씬에서 사용할 상태를 등록한다.

| 씬 | 등록 클래스 | 등록 상태 |
|-----|------------|----------|
| **Main** | `Main.cs` | LobbyState, DashboardState, Depth1State, Depth1_5State, Depth1_5RestoreState, StyleRoomState |
| **Planet** | `Planet.cs` | Depth2State, Depth3State, Depth2RestoreState |
| **WebGalaxy** | `WebGalaxy.cs` | WebDepth1State, WebDepth1_5State |
| **WebStyleRoom** | `WebStyleRoom.cs` | WebStyleRoomState |

---

## 6. UI 시스템 구조

```
┌── UI_Base <<abstract>> ──────────────────────────────────────────────┐
│  +string UIKey / +int SortOrder                                      │
│  +Init()* / +Open() / +Close() / +Release()*                        │
│  +SetUIKey(string) / +SetSortOrder(int)                              │
└──────────┬───────────────────────────────────────────────────────────┘
           │ inherits
     ┌─────┼──────────────────┐
     ▼     ▼                  ▼
┌─ UI_Scene ──┐  ┌─ UI_Popup ──┐  ┌─ UI_Sub ─────────────────────────┐
│  +Release() │  │  +Release() │  │  +UI_Base OwnerUI                 │
└─────────────┘  └─────────────┘  │  +SetOwnerUI() / +OnSpawn()      │
                                  │  +OnDespawn()                     │
                                  └───────────────────────────────────┘

┌── UIManager ─────────────────────────────────────────────────────────┐
│                                                                      │
│  Scene UI (Dict 캐싱, sortOrder 0+, +1씩 증가):                     │
│    +ShowSceneUI<T>(key) / +CloseSceneUI(key)                        │
│    +GetSceneUI<T>(key)  / +ResetSceneUIOrder()                      │
│                                                                      │
│  Popup UI (Stack LIFO, sortOrder 100+, +10씩):                      │
│    +ShowPopupUI<T>(key) / +CloseTopPopup() / +CloseAllPopups()      │
│    +GetPopupUI<T>(key)  / +IsPopupOpen(key) / +PopupCount           │
│                                                                      │
│  Sub UI (오브젝트 풀링):                                             │
│    +ShowSubUI<T>(key, parent) / +CloseSubUI() / +CloseSubUIs()     │
│    +GetActiveSubUICount(key) / +GetSubUIPoolCount(key)              │
│                                                                      │
│  ──[Singleton]──> AddressableManager (프리팹 로드)                   │
│  UI_Scene / UI_Popup  ──Release()──> CloseSubUIs(this)              │
│  UI_Sub ──> OwnerUI (부모 UI_Base 자동 탐색)                        │
└──────────────────────────────────────────────────────────────────────┘
```

| 레이어 | 클래스 | sortOrder | 관리 방식 | 특징 |
|--------|--------|-----------|-----------|------|
| Scene UI | `UI_Scene` | 0 ~ 99 | Dictionary 캐싱 | 고정 HUD, 생성 시 +1 자동 증가 |
| Popup UI | `UI_Popup` | 100+ | Stack(LIFO) + 중복 방지 | 팝업 순서 +10씩, `CloseTopPopup()` |
| Sub UI | `UI_Sub` | 부모 따름 | Queue 기반 오브젝트 풀링 | `OnSpawn()/OnDespawn()`, 부모 UI 자동 연동 |

---

## 6-1. 오브젝트 수명주기

> 3가지 풀링 시스템(ObjectManager, UIManager Sub, SoundManager SFX)의 오브젝트 수명주기를 순환 다이어그램으로 시각화.

```
┌── ObjectManager 풀링 ──────────────────────────────────┐
│                                                        │
│  [Prefab] ──최초 생성──> [Instantiate] ──> [Active]    │
│                                              │  ▲      │
│                                    Despawn() │  │      │
│                                              ▼  │      │
│                                           [Pool]       │
│                                              Spawn()   │
└────────────────────────────────────────────────────────┘

┌── UIManager Sub 풀링 ─────────────────────────────────┐
│                                                        │
│  [Prefab] ──최초 생성──> [Create] ──> [Pool]           │
│                                        │  ▲            │
│                           ShowSubUI()  │  │            │
│                                        ▼  │            │
│                                     [Active]           │
│                                        CloseSubUI()    │
└────────────────────────────────────────────────────────┘

┌── SoundManager SFX 풀링 ──────────────────────────────┐
│                                                        │
│           [Pool (AudioSource)] ──PlaySFX()──> [Playing]│
│                  ▲                               │     │
│                  └──── Complete / StopSFX() ─────┘     │
└────────────────────────────────────────────────────────┘
```

| 풀링 시스템 | 생성 | 반환 | 재사용 | 콜백 |
|-------------|------|------|--------|------|
| **ObjectManager** | `Spawn<T>(key)` → Instantiate | `Despawn(obj)` → Queue | Pool에서 꺼내 재활성화 | - |
| **UIManager (Sub)** | `ShowSubUI<T>()` → Create | `CloseSubUI()` → Queue | Pool에서 꺼내 `OnSpawn()` | `OnSpawn()` / `OnDespawn()` |
| **SoundManager (SFX)** | AudioSource Pool 생성 | Complete/Stop → Available | Pool에서 꺼내 Play | - |

---

## 7. 데이터 레이어

```
┌── DataManager (partial class 10파일) ────────────────────────────────┐
│                                                                      │
│  DataManager.cs                                                      │
│    -AddressableManager addressableManager                            │
│    +Init() / +Release()                                              │
│                                                                      │
│  DataManager.KnowledgeGraph.cs                                       │
│    -IKnowledgeGraphProvider knowledgeGraphProvider                    │
│    -Dictionary caches (galaxy/node/userNode/branchNode/roadMap)      │
│    +GetAllGalaxiesAsync(userId)      +GetGalaxyAsync(userId, id)    │
│    +GetNodeAsync(userId, id)         +GetNodesByGalaxyAsync()       │
│    +GetUserNodeAsync(userId, id)     +GetUserNodesByGalaxyAsync()   │
│    +GetBranchNodesAsync(userId, nodeId)                              │
│    +GetAllRoadMapsAsync(userId)      +GetRoadMapAsync(userId, id)   │
│    +SetKnowledgeGraphProvider()      +ClearKnowledgeGraphCache()    │
│    +SelectRandomRoadMap()                                            │
│                                                                      │
│  DataManager.Avatar.cs                                               │
│    -IAvatarDataProvider avatarProvider                                │
│    +GetAvatarEquipDataAsync() / +SaveAvatarEquipDataAsync()         │
│                                                                      │
│  DataManager.Friend.cs    DataManager.GNB.cs    DataManager.Notice  │
│  DataManager.Dashboard.cs DataManager.Topic.cs                     │
│  DataManager.CreateAvatar.cs  DataManager.StyleRoom.cs             │
│    -IFriendDataProvider                                              │
│                                                                      │
│  ──[Singleton]──> AddressableManager                                │
└──────────────────────────────────────────────────────────────────────┘
                              │
                      Provider Pattern
                              ▼
┌── Provider Interfaces & Implementations ─────────────────────────────┐
│                                                                      │
│  IKnowledgeGraphProvider                                             │
│    +GetGalaxiesAsync()         +GetGalaxyByIdAsync()                │
│    +GetNodesByGalaxyIdAsync()  +GetNodeByIdAsync()                  │
│    +GetBranchNodesByNodeIdAsync()                                    │
│    +GetAllRoadMapsAsync()      +GetRoadMapByIdAsync()               │
│    +GetUserNodeByIdAsync()     +GetUserNodesByGalaxyAsync()         │
│       └── JsonKnowledgeGraphProvider                                 │
│                                                                      │
│  IAvatarDataProvider                                                 │
│       ├── JsonAvatarDataProvider                                     │
│       └── ApiAvatarDataProvider                                      │
│                                                                      │
│  IFriendDataProvider                                                 │
│       ├── JsonFriendDataProvider                                     │
│       └── ApiFriendDataProvider                                      │
│                                                                      │
│  IDashboardContentProvider                                           │
│  ITopicDataProvider                                                  │
│  IGNBDataProvider                                                    │
│  INoticeDataProvider                                                 │
│                                                                      │
│  IStyleRoomDataProvider                                              │
│       ├── JsonStyleRoomDataProvider                                  │
│       └── ApiStyleRoomDataProvider                                   │
│                                                                      │
│  ICreateAvatarDataProvider                                           │
│       └── CreateAvatarDataProvider                                   │
└──────────────────────────────────────────────────────────────────────┘
```

```
┌── APIManager (partial class 4파일) ──────────────────────────────────┐
│                                                                      │
│  APIManager.cs                                                       │
│    -string baseUrl / -Dict<string,string> defaultHeaders             │
│    +SetBaseUrl() / +SetDefaultHeader() / +RemoveDefaultHeader()     │
│    +ClearDefaultHeaders()                                            │
│    ──[Singleton]──> PlatformManager, ErrorEventManager               │
│                                                                      │
│  APIManager.Request.cs                                               │
│    -IRequestHandler requestHandler                                   │
│    +SetRequestHandler() / +RetryRequestAsync<T>()                   │
│    +GetAsync<T>(endpoint)          +PostAsync<T,R>(endpoint, body)  │
│    +PutAsync<T,R>(endpoint, body)  +DeleteAsync<T>(endpoint)        │
│    -SendRequestInternalAsync() → requestHandler.SendAsync() 위임    │
│                                                                      │
│  APIManager.Response.cs                                              │
│    +ParseResponse<T>(json) : Newtonsoft.Json                         │
│    +HandleError(responseCode, error)                                 │
│    +NotifyApiError(errorBody) → ErrorEventManager 에러 이벤트 발행  │
│                                                                      │
│  APIManager.Token.cs                                                 │
│    -ITokenProvider tokenProvider                                     │
│    +HasToken / +SetTokenProvider() / +RequestTokenAsync()           │
│    +RefreshTokenAsync() / +ClearToken()                              │
└──────────────────────────────────────────────────────────────────────┘
                         │                │
                  Token Provider    Request Handler
                         ▼                ▼
┌── ITokenProvider ────────────────────────────────────────────────────┐
│  +string Token / +bool HasToken                                      │
│  +RequestTokenAsync() UniTask<string>                                │
│  +RefreshTokenAsync() UniTask<bool>                                  │
│  +ClearToken()                                                       │
│       ├── FlutterTokenProvider                                       │
│       ├── WebTokenProvider                                           │
│       └── EditorTokenProvider                                        │
└──────────────────────────────────────────────────────────────────────┘
┌── IRequestHandler ───────────────────────────────────────────────────┐
│  +SendAsync<T>(request, endpoint, method, jsonBody, isRetry)        │
│  +SendBinaryAsync(request)                                           │
│       ├── RequestHandler     (앱/에디터: if result 체크)             │
│       └── WebRequestHandler  (WebGL: catch 에러 감지)                │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 8. 입력 시스템 구조

```
┌── InputManager (partial 2파일) ───────────────────────────────────────┐
│                                                                       │
│  InputLoopAsync (UniTask)         TouchState Dictionary:              │
│  await UniTask.Yield(Update)      startPos, lastPos, startTime,      │
│       │                           isDragging, isLongPressing          │
│       ├──> ProcessTouchInput()         Touch.fingerId 기반            │
│       ├──> ProcessMouseInput()         mouseId = -1                   │
│       │       └──> ProcessScrollPinchInput()                         │
│       └──> ProcessPinchInput()         touchCount == 2               │
│                                                                       │
│  DoubleTap 판정: Threshold 0.3s / Distance 50px                      │
└───────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌── IInputHandler (Strategy) ───────────────────────────────────────────┐
│  OnTap(InputData)                OnDragStart/Dragging/DragEnd         │
│  OnDoubleTap(InputData)          OnPinchStart/Pinching/PinchEnd       │
│  OnLongPressStart/Pressing/End   OnClear()                            │
└───────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌── 이벤트 브로드캐스트 (Event partial) ────────────────────────────────┐
│  BroadcastInputEvent(InputEvent, InputEventData)                      │
│  BroadcastPinchEvent(InputEvent, PinchData)                           │
│       └──> List<IInputEventReceiver>                                  │
└───────────────────────────────────────────────────────────────────────┘

┌── 데이터 구조체 ──────────────────────────────────────────────────────┐
│  InputData      : touchId, position, startPosition,                   │
│                   delta, totalDelta, duration, DragDirection           │
│  PinchData      : centerPosition, distance, deltaDistance,            │
│                   scale, startDistance                                 │
│  InputEventData : InputData + hitObject (Raycast)                     │
└───────────────────────────────────────────────────────────────────────┘
```

| 제스처 | 판정 기준 | Handler 메서드 |
|--------|-----------|---------------|
| Tap | 드래그 미만 + 롱프레스 미만 | `OnTap()` |
| DoubleTap | 0.3초 내 50px 이내 두 번째 탭 | `OnDoubleTap()` |
| Drag | `totalDelta > 10px` | `OnDragStart/Dragging/DragEnd()` |
| LongPress | `duration >= 0.5s` (드래그 아닌 경우) | `OnLongPressStart/Pressing/End()` |
| Pinch | `touchCount == 2` | `OnPinchStart/Pinching/PinchEnd()` |
| Editor Pinch | `UNITY_EDITOR` 마우스 스크롤 | 동일 Pinch 핸들러 |

`DragDirection` Enum: `None`, `Up`, `Down`, `Left`, `Right`

---

## 9. 각 매니저 책임 요약

| 매니저 | 핵심 책임 | [Singleton] 의존성 | 주요 메서드 |
|--------|----------|-------------------|------------|
| **FacadeManager** | 18개 매니저 수집/정렬/초기화/해제 | (MonoBehaviour, Singleton 아님) | `CollectManagers()`, `SortByDependency()`, `InitAll()`, `ReleaseAll()` |
| **EventManager** | 제네릭 이벤트 발행/구독 시스템 | 없음 (Tier 0) | `AddListener<T>()`, `RemoveListener<T>()`, `PostNotification<T>()`, `Enqueue<T>()`, `ProcessQueue<T>()` |
| **AddressableManager** | Addressable 에셋 프리로드/캐싱/해제 | 없음 (Tier 0) | `PreloadAsync<T>(label)`, `Load<T>(key)`, `TryLoad<T>(key)`, `ReleaseLabel(label)`, `InstantiateAsync(key)` |
| **FactoryManager** | 오브젝트 생성 전략 (예약) | 없음 (Tier 0) | `Init()`, `Release()` |
| **CameraManager** | Cinemachine 카메라 관리, 핸들러 패턴 | 없음 (Tier 0) | `SetHandler(ICameraHandler)`, `RegisterVirtualCamera(key, vcam)`, `SetActiveVirtualCamera(key)`, `SetCutBlend()`, `SetSmoothBlend()` |
| **InputManager** | UniTask 입력 루프, Handler 위임, 이벤트 브로드캐스트 | 없음 (Tier 0) | `SetHandler(IInputHandler)`, `ClearHandler()`, `RegisterReceiver()`, `BroadcastInputEvent()` |
| **PlatformManager** | 플랫폼 판별, Provider 팩토리, IPlatformSetup 실행 | 없음 (Tier 0) | `GetBaseUrl()`, `CreateTokenProvider()`, `CreateRequestHandler()` |
| **ErrorEventManager** | API 에러 이벤트 구독/발행 (int 에러코드 키 기반) | 없음 (Tier 0) | `AddListener(errorCode, listener)`, `RemoveListener()`, `PostNotification(ApiErrorResponse)` |
| **APIManager** | HTTP 통신, JWT 토큰 관리, 요청 핸들러 위임 | `PlatformManager`, `ErrorEventManager` | `GetAsync<T>()`, `PostAsync<T,R>()`, `SetTokenProvider()`, `SetRequestHandler()`, `NotifyApiError()` |
| **DataManager** | 도메인별 데이터 Provider 관리, 캐싱 | `AddressableManager` | `GetAllGalaxiesAsync()`, `GetNodeAsync()`, `GetAvatarEquipDataAsync()`, `ClearKnowledgeGraphCache()` |
| **ObjectManager** | 비UI GameObject 오브젝트 풀링 | `AddressableManager` | `Spawn<T>(key)`, `Despawn(obj)`, `DespawnAll(key)`, `ReleaseAll()`, `Clear()` |
| **UIManager** | Scene/Popup/Sub 3계층 UI 관리 + CanvasScaler 통합 관리 | `AddressableManager`, `UILayoutManager`, `EventManager` | `ShowSceneUI<T>()`, `ShowPopupUI<T>()`, `ShowSubUI<T>()`, `CloseTopPopup()`, `CloseAllPopups()` |
| **LoadingManager** | 씬 전환 효과 (Fade/Circle/Checkerboard/Directional) | `AddressableManager` | `PreloadAsync()`, `FadeOutAsync(type)`, `FadeInAsync(type)`, `TransitionAsync(type)`, `TransitionWithIconAsync(type)` |
| **FlutterMessageManager** | Flutter-Unity 양방향 통신 | `EventManager` | `Init()` (DataString 처리), `pendingRequests` (UniTaskCompletionSource) |
| **GameSceneManager** | Unity 씬 비동기 로드/전환, 히스토리 관리, 복귀 데이터 보존 | `EventManager`, `AddressableManager`, `LoadingManager`, `UIManager` | `LoadSceneAsync()`, `TransitionToSceneAsync()`, `LoadPreviousSceneAsync()`, `ClearHistory()`, `SaveReturnData()`, `SetPendingInitialState()`, `ConsumePendingInitialState()` |
| **GameStateManager** | FSM 상태 관리, 이벤트 연동 | `EventManager` | `RegisterState()`, `ChangeState()`, `ReturnToPreviousState()`, `PauseCurrentState()`, `ResumeCurrentState()` |
| **SoundManager** | BGM (Provider 패턴) + SFX (오브젝트 풀링) | `FlutterMessageManager` | `PlayBGM(name)`, `StopBGM()`, `PlaySFX(clip)`, `PlaySFXLoop(clip)`, `SetSFXVolume(volume)` |
| **HexGridManager** | 헥스 그리드 생성/탐색/이벤트 | `DataManager`, `EventManager` | `SetGrid()`, `LoadGridAsync(nodeId)`, `OnBlockClicked()`, `FindPath(start, goal)`, `GetBlock(x, y)` |
| **UILayoutManager** | 가로/세로 레이아웃 모드 판정/전환 관리 | `EventManager` | `Register(UILayoutState)`, `Unregister(UILayoutState)`, `GetReferenceResolution(mode)`, `CurrentMode` |

---

## 10. 디자인 패턴 정리

| 패턴 | 적용 위치 | 설명 |
|------|----------|------|
| **Facade** | `FacadeManager` | 18개 매니저의 단일 진입점. 수집/정렬/초기화/해제를 통합 관리 |
| **Singleton** | `Singleton<T>` | `MonoBehaviour` 기반 제네릭 싱글톤. 자동 생성 + `DontDestroyOnLoad` |
| **Observer** | `EventManager` + `IListener<TEnum>`, `ErrorEventManager` + `IErrorListener` | 제네릭 이벤트 시스템 + API 에러 이벤트 시스템 (int 에러코드 키 기반) |
| **State** | `GameStateManager` + `IGameState` | FSM 패턴. `OnStateEnter/Exit/Update/Pause/Resume` 생명주기 |
| **Strategy** | `IInputHandler`, `ICameraHandler`, `IBGMProvider`, `IRequestHandler` | 런타임 핸들러 교체. 상태별 입력/카메라/BGM, 플랫폼별 HTTP 요청 처리 |
| **Object Pooling** | `ObjectManager`, `UIManager(Sub)`, `SoundManager(SFX)` | `Queue<T>` 기반 재사용. `Spawn/Despawn`, `OnSpawn/OnDespawn` 콜백 |
| **Provider (DI)** | `DataManager`, `APIManager(Token)`, `PlatformManager` | 인터페이스 기반 데이터 소스/플랫폼 구현체 교체. `Json/Api/Web/Editor` 구현체 |
| **Partial Class** | `DataManager(10)`, `APIManager(4)`, `GameSceneManager(3)`, `FlutterMessageManager(3)`, `WebMessageManager(3)`, `InputManager(2)`, `LoadingManager(2)` | 도메인별 코드 분리. 하나의 클래스를 역할 단위로 파일 분할 |
| **DI (Attribute 기반)** | `[Singleton(typeof(T))]` + `InjectUtil` + `DependencyResolver` | 리플렉션 기반 싱글톤 주입. Kahn's Algorithm 위상 정렬로 초기화 순서 보장 |

---

## 부록: Enum 값 전체 목록

### GameStateType
```csharp
None, Dashboard, Lobby, Depth0, Depth1, Depth1_5, Depth1_5Restore, Depth2, Depth3, Depth2Restore, StyleRoom,
// Web
WebDepth1, WebDepth1_5, WebStyleRoom, WebCreateAvatar
```

### SceneType
```csharp
Intro, Lobby, InGame, Result, Loading, Planet, Main
```

### LoadingType
```csharp
Fade,           // SimpleFade (단순 알파 페이드)
Circle,         // CircleWipe (원형 와이프)
Checkerboard,   // CheckerboardFade (체스보드 패턴)
Directional     // DirectionalFade (방향성 페이드)
```

### SceneEvent
```csharp
LoadStarted, LoadCompleted, TransitionProgressUpdated, SceneReady
```

### GameStateEvent
```csharp
StateEntered, StateExited, StatePaused, StateResumed
```

### InputEvent
```csharp
Tap,
LongPressStart, LongPressing, LongPressEnd,
DragStart, Dragging, HorizontalDragging, VerticalDragging, DragEnd,
PinchStart, Pinching, PinchEnd
```

> **Note:** `DoubleTap`은 `InputEvent` Enum에 포함되지 않고, `IInputHandler.OnDoubleTap()`으로 직접 Handler에 위임됩니다.

### HexGridEvent
```csharp
GridLoaded, GridCleared, BlockClicked, BlockHighlighted, PathFound
```

### FlutterEvent
```csharp
// Lecture
LectureCompleteMission, LectureCompleteTest,
// GNB Action
GNBEnterFriendPlanet, GNBChangeTeacher, GNBChangeRoadmap,
// GNB Close (12종)
GNBCloseRoadmap, GNBCloseAbilities, GNBCloseWrongAnswersNotebook,
GNBCloseWeaknessDiagnosis, GNBCloseLearningRecords, GNBCloseReports,
GNBCloseLevel, GNBCloseAchievements, GNBClosePointmall,
GNBCloseFriends, GNBCloseRankings, GNBCloseSettings, GNBCloseProfile
```

### StyleRoomEvent
```csharp
MainCategoryChanged, SubCategorySelected, SubCategoryInitialized,
CreateSlotRequested, SlotRecycled, ItemSelected, EquippedItemsChanged,
PurchaseRequested, SubCategoryBadgeChanged, PurchaseStateChanged,
SaveRequested, HistoryStateChanged, Undo, Redo, Refresh,
PageChanged, PageDataLoaded
```

### CreateAvatarEvent
```csharp
MainCategoryChanged, SubCategorySelected, SubCategoryInitialized,
CreateSlot, SlotRecycled, ItemSelected, Undo, Redo, Save, Refresh,
HistoryStateChanged
```

### WebCreateAvatarEvent
```csharp
Complete
```

### Depth1_5CameraEvent
```csharp
InitialTransitionComplete, FocusStarted, ZoomInStarted,
NodeTransitionStarted, NodeTransitionComplete
```

### CostumeEvent
```csharp
SetAllCostume, SetCostume, ClearCostume
```

### WebEvent
```csharp
ViewportChanged
```

### LayoutEvent
```csharp
ScalerUpdate, ModeChanged
```

### LayoutMode
```csharp
Landscape, Portrait
```

### DragDirection
```csharp
None, Up, Down, Left, Right
```
