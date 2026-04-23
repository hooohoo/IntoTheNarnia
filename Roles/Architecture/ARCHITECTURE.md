# CHALK 4.0 - Project Architecture

## 0. 시스템 개요도

```
┌────────────────────── Presentation Layer ──────────────────────┐
│   UIManager (Scene/Popup/Sub)   ·   LoadingManager            │
│   UILayoutManager (가로/세로 레이아웃 전환)                    │
├────────────────────── Game Logic Layer ────────────────────────┤
│   GameStateManager   ·   InputManager                         │
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

## 1. 클래스 계층도

```
┌── Base Classes ──────────────────────────────────────────────────────┐
│  MonoBehaviour (Unity)                                               │
│       ├──> Singleton<T>     -static T instance / +static T Instance  │
│       └──> FacadeManager    -List<IManager> managers                 │
│                -Awake() → CollectManagers() → SortByDependency()     │
│                -InitAll() → ReleaseAll() → OnApplicationQuit()       │
│  IManager <<interface>>     +Init() / +Release()                     │
└──────────────────────────────────────────────────────────────────────┘

┌── Partial Class 분할 (* 표시 매니저) ────────────────────────────────┐
│  DataManager          : 7파일  .cs / .Avatar / .Friend / .GNB /     │
│                                .Notice / .CreateAvatar / .StyleRoom │
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

`[Singleton(typeof(...))]` 어트리뷰트 기반. `DependencyResolver`가 Kahn's Algorithm으로 위상 정렬.

```
┌── Tier 0 (의존성 없음) ──────────────────────────────────────────────┐
│  EventManager       AddressableManager   FactoryManager              │
│  ErrorEventManager  CameraManager        InputManager                │
│  PlatformManager                                                     │
└──────────────────────────────────────────────────────────────────────┘
        ▲
┌── Tier 1 ────────────────────────────────────────────────────────────┐
│  DataManager ─────────────────> AddressableManager                   │
│  ObjectManager ───────────────> AddressableManager                   │
│  LoadingManager ──────────────> AddressableManager                   │
│  FlutterMessageManager ──────> EventManager                          │
│  GameStateManager ────────────> EventManager                         │
│  APIManager ──────────────────> PlatformManager, ErrorEventManager   │
│  UILayoutManager ─────────────> EventManager                         │
└──────────────────────────────────────────────────────────────────────┘
        ▲
┌── Tier 2 ────────────────────────────────────────────────────────────┐
│  UIManager ───────────────────> AddressableManager, UILayoutManager, │
│                                 EventManager                         │
│  SoundManager ────────────────> FlutterMessageManager                │
│  GameSceneManager ────────────> EventManager, AddressableManager,    │
│                                 LoadingManager, UIManager            │
└──────────────────────────────────────────────────────────────────────┘
        ▲
┌── Tier 3 ────────────────────────────────────────────────────────────┐
│  WebMessageManager ──────────> EventManager, GameSceneManager        │
└──────────────────────────────────────────────────────────────────────┘
```

### 런타임 통신 방식 (의존성 그래프 외 소통)

| 화살표 | 방식 | 설명 |
|:---:|--------|------|
| 실선 → | `[Singleton]` 직접 참조 | 위 Tier 그래프와 동일 |
| 점선 ··> | `EventManager` 이벤트 | `PostNotification()` → `OnEvent()` 간접 통신 |
| 이중선 ═> | `Addressable` 리소스 | 프리팹/에셋 비동기 로드 요청 |

**이벤트 기반 소통 (점선):**
```
GameStateManager  ··> UIManager, CameraManager, InputManager  (GameStateEvent)
FlutterMessageMgr ··> GameStateManager                        (FlutterEvent)
InputManager      ··> GameStateManager                        (InputEvent)
WebMessageManager ··> UILayoutManager                         (WebEvent)
UILayoutManager   ··> UIManager, UI Components                (LayoutEvent)
```

**Addressable 리소스 소통 (이중선):**
```
DataManager / ObjectManager / UIManager / LoadingManager  ═>  AddressableManager
```

---

## 3. 초기화 / 해제 흐름

```
초기화 (Awake):
  FacadeManager.Awake()
    → CollectManagers() — 18개 Singleton<T> 인스턴스 수집
    → DependencyResolver.Sort() — [Singleton] 리플렉션 → Kahn's 위상정렬
    → foreach manager.Init() — 순서대로 InjectUtil.InjectSingleton() + 초기화

해제 (OnApplicationQuit):
  FacadeManager.ReleaseAll()
    → foreach (역순) manager.Release() — 캐시 정리, 이벤트 해제
```

### InjectUtil — 의존성 주입 유틸리티

```
InjectUtil.InjectSingleton(this)
  → [Singleton(typeof(T))] 어트리뷰트가 붙은 필드를 리플렉션으로 찾아
    Singleton<T>.Instance 주입. static 캐시로 2번째부터 O(1).

InjectUtil.InjectComponents(this)
  → [FindComponents("자식이름")] 어트리뷰트가 붙은 필드를 리플렉션으로 찾아
    BindRoot 태그 기반 Hierarchy DFS 탐색 → GetComponent로 주입.
    단일(T) / 배열(T[]) / 리스트(List<T>) 자동 분기.
```

### 런타임 데이터 흐름

**사용자 입력 → 화면 업데이트:**
```
[User Touch] → InputManager → IInputHandler → EventManager.PostNotification → GameStateManager → UIManager
```

**데이터 로딩:**
```
[Flutter/API] → FlutterMessageMgr/APIManager → DataManager(캐시) → ObjectManager(Spawn)
```

---

## 4. 매니저 책임 요약

| 매니저 | 핵심 책임 |
|--------|----------|
| **FacadeManager** | 매니저 수집/정렬/초기화/해제 |
| **EventManager** | 제네릭 이벤트 발행/구독 (IListener) |
| **ErrorEventManager** | API 에러 이벤트 (int 에러코드 키 기반) |
| **AddressableManager** | 에셋 프리로드/캐싱/해제 |
| **PlatformManager** | PlatformType(Editor/WebGL/App) 판별, IPlatformSetup 구현체 선택, CreateTokenProvider/CreateRequestHandler 팩토리 |
| **DataManager** | 도메인별 Provider + Dictionary 캐싱 (partial 7파일) |
| **APIManager** | HTTP 통신, JWT 토큰, 요청 핸들러 위임 (partial 4파일) |
| **ObjectManager** | 비UI GameObject 오브젝트 풀링 (Queue + HashSet) |
| **UIManager** | Scene/Popup/Sub 3계층 UI + CanvasScaler 관리 |
| **UILayoutManager** | 가로/세로 레이아웃 모드 판정/전환 |
| **LoadingManager** | 씬 전환 효과 (Fade/Circle/Checkerboard/Directional) |
| **GameStateManager** | FSM 상태 관리, 이벤트 연동 |
| **GameSceneManager** | 씬 비동기 로드/전환, 히스토리(Stack), 복귀 데이터 |
| **InputManager** | UniTask 입력 루프, Handler 위임, 이벤트 브로드캐스트 |
| **CameraManager** | Cinemachine 카메라 관리, Handler 패턴 |
| **SoundManager** | BGM (Provider 패턴) + SFX (오브젝트 풀링) |
| **FlutterMessageManager** | Flutter-Unity 양방향 통신 (UniTaskCompletionSource) |
| **WebMessageManager** | Web(JS)-Unity 양방향 통신 |
| **FactoryManager** | 오브젝트 생성 전략 (예약) |

---

## 5. 디자인 패턴 정리

| 패턴 | 적용 위치 | 설명 |
|------|----------|------|
| **Facade** | `FacadeManager` | 18개 매니저의 단일 진입점 |
| **Singleton** | `Singleton<T>` | MonoBehaviour 제네릭 싱글톤 + DontDestroyOnLoad |
| **Observer** | `EventManager` + `IListener<TEnum>` | 제네릭 이벤트 시스템 |
| **State** | `GameStateManager` + `IGameState` | FSM. OnStateEnter/Exit/Pause/Resume |
| **Strategy** | `IInputHandler`, `ICameraHandler`, `IBGMProvider`, `IRequestHandler` | 런타임 핸들러 교체 |
| **Object Pooling** | `ObjectManager`, `UIManager(Sub)`, `SoundManager(SFX)` | Queue 기반 재사용 |
| **Provider (DI)** | `DataManager`, `APIManager(Token)`, `PlatformManager` | 인터페이스 기반 구현체 교체 |
| **Partial Class** | DataManager(7), APIManager(4), GameSceneManager(3) 등 | 도메인별 파일 분할 |
| **DI (Attribute)** | `[Singleton(typeof(T))]` + `InjectUtil` + `DependencyResolver` | Kahn's Algorithm 위상 정렬 |
| **MVC** | UI 기본 구조 — Controller + Model + View (+ Service) | View가 `Bind(model)`로 Model delegate 직접 구독. 정적 UI만 단일 Controller 예외 (상세: `ARCHITECTURE.UI.md`) |

---

## 6. 새 매니저 추가 체크리스트

- [ ] `Singleton<T>` 상속 + `IManager` 구현
- [ ] `[Singleton(typeof(...))]` 으로 의존성 선언
- [ ] Init에서 `InjectUtil.InjectSingleton(this)` 호출
- [ ] FacadeManager.CollectManagers()에 수동 등록
- [ ] Release에서 캐시/이벤트/리소스 정리
