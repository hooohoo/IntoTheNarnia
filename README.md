# Chalk 4 Game

> Flutter 앱에 임베딩되는 Unity 기반 교육용 3D 게임.

`Unity 2022.3.62f3` · `URP 14` · `iOS` · `Android` · `WebGL`

---

## 목차

- [핵심 특징](#핵심-특징)
- [기술 스택](#기술-스택)
- [빠른 시작](#빠른-시작)
- [프로젝트 구조](#프로젝트-구조)
- [아키텍처 한눈에 보기](#아키텍처-한눈에-보기)
- [씬 & 게임 상태 플로우](#씬--게임-상태-플로우)
- [플랫폼 통합 (Flutter / Web)](#플랫폼-통합-flutter--web)
- [추가 문서](#추가-문서)

---

## 핵심 특징

- **듀얼 런타임** — Flutter 임베딩 모드와 Web 독립 실행 모드를 동시에 지원
- **매니저 기반 아키텍처** — `FacadeManager` + `DependencyResolver`(위상 정렬)로 18개 `Singleton<T>` 매니저를 자동 초기화
- **UniTask 기반 비동기** — 코루틴 없이 `async` / `await` 스타일로 게임 로직 작성
- **Addressables 리소스 스트리밍** — 플랫폼별 에셋 번들을 분리 관리

---

## 기술 스택

| 영역 | 기술 |
| --- | --- |
| 엔진 | Unity 2022.3.62f3 (LTS) |
| 렌더 파이프라인 | Universal Render Pipeline 14.0.12 |
| 비동기 | UniTask |
| 리소스 | Addressables 1.22.3 |
| 카메라 | Cinemachine 2.10.5 |
| UI 텍스트 | TextMeshPro 3.0.7 |
| JSON | Newtonsoft.Json 3.2.2 |
| 플랫폼 브리지 | FlutterUnityIntegration |
| 지원 플랫폼 | iOS · Android · WebGL |

---

## 빠른 시작

**요구사항**

- Unity Hub
- Unity **2022.3.62f3** (LTS)
- Git

**클론**

```bash
git clone <repo-url> chalk_4_game
cd chalk_4_game
```

**프로젝트 열기**

1. Unity Hub → `Add Project` → `chalk_4_game` 폴더 선택
2. Unity Editor가 해당 버전으로 프로젝트를 엽니다.

**에디터 실행**

- `Assets/@Project/00.Scenes/Main.unity` 를 열고 Play
- Web 독립 모드 테스트는 `WebMain.unity` / `WebGalaxy.unity` 등 `Web` 접두 씬 사용

**빌드**

- `File > Build Settings` 에서 iOS / Android / WebGL 타겟 선택
- Flutter 임베딩 및 WebGL 배포의 상세 절차는 내부 배포 문서를 참조

> **주의** — Play 전에 최소 한 번은 `Window > Asset Management > Addressables > Groups` 에서 Addressables 빌드가 필요합니다.

---

## 프로젝트 구조

```
Assets/@Project/
├── 00.Scenes/          # Unity 씬 (Main, Planet, Loading, Web*)
├── 01.Scripts/
│   ├── Base/           # 기반 클래스
│   ├── Manager/
│   │   ├── Facade/     # FacadeManager, DependencyResolver
│   │   ├── Interface/  # 매니저 인터페이스
│   │   └── Manager/    # 18개 Singleton 매니저
│   ├── Singleton/      # Singleton<T> 제네릭 베이스
│   ├── Scene/          # Main.cs / Planet.cs + Screen 클래스
│   ├── UI/             # UI_Base, UI_Scene / UI_Popup / UI_Sub
│   ├── Game/           # 게임 로직
│   ├── Data/           # 데이터 모델 + Provider 구현
│   ├── Enum/           # GameStateType 등 열거형
│   └── Util/           # InjectUtil 등 유틸리티
├── 02.Prefabs/
├── 03.Sprites/
├── 04.Materials/
├── 05.Animations/
├── 06.Data/            # JSON 데이터
├── 07.Sounds/
└── 08.Fonts/
```

---

## 아키텍처 한눈에 보기

```
┌──────────────────────────────────────────────────┐
│ Presentation    UIManager · LoadingManager ·     │
│                 UILayoutManager                  │
├──────────────────────────────────────────────────┤
│ Game Logic      GameStateManager ·               │
│                 HexGridManager · InputManager    │
├──────────────────────────────────────────────────┤
│ Core Service    EventManager · ErrorEventManager │
│                 GameSceneManager · CameraManager │
├──────────────────────────────────────────────────┤
│ Data / Resource DataManager · AddressableManager │
│                 ObjectManager · APIManager ·     │
│                 SoundManager · FactoryManager    │
├──────────────────────────────────────────────────┤
│ Platform        FlutterMessageManager ·          │
│                 WebMessageManager ·              │
│                 PlatformManager                  │
└──────────────────────────────────────────────────┘
```

- 18개 `Singleton<T>` 매니저를 `FacadeManager` 가 리플렉션으로 수집한다.
- `DependencyResolver` 가 `[Singleton(typeof(...))]` 어트리뷰트를 기준으로 **위상 정렬**을 수행하여 초기화 순서를 결정한다.
- 매니저 간 통신은 세 가지 방식을 사용한다 — `[Singleton]` 직접 주입 · `EventManager` 이벤트 · `AddressableManager` 리소스.

> 의존성 그래프, 전체 매니저 리스트, 계층(Tier) 규칙 상세는 [ARCHITECTURE.md](./ARCHITECTURE.md) 를 참조하세요.

---

## 씬 & 게임 상태 플로우

**씬 구성**

| 씬 | 용도 |
| --- | --- |
| `Main.unity` | 로비 / 대시보드 / Depth1 ~ 1.5 / StyleRoom (Flutter 모드 메인) |
| `Planet.unity` | 행성 뷰 / Depth2 ~ 3 |
| `Loading.unity` | 로딩 화면 |
| `WebMain.unity` · `WebGalaxy.unity` · `WebCreateAvatar.unity` · `WebStyleRoom.unity` | Web 독립 실행 모드 전용 |

**게임 상태 흐름**

```
Lobby → Dashboard → Depth0 → Depth1 → Depth1.5 → Depth2 → (auto) → Depth3
```

- `Main.cs` 가 Lobby / Dashboard / Depth1 계열의 `IGameState` 구현체를 등록합니다.
- `Planet.cs` 가 Depth2 / Depth3 계열의 `IGameState` 구현체를 등록합니다.

> 각 State 의 책임, Restore 흐름, Web 전용 State 의 상세는 [ARCHITECTURE.md](./ARCHITECTURE.md) 를 참조하세요.

---

## 플랫폼 통합 (Flutter / Web)

**Flutter ↔ Unity**

- `FlutterMessageManager` (partial class, 3파일)
- JSON 메시지 기반 양방향 통신
- `pendingRequests` 딕셔너리 + `UniTaskCompletionSource` 로 비동기 요청 / 응답 처리

**Web ↔ Unity**

- `WebMessageManager` (partial class, 3파일)
- `U2WMessageType` 상수 + `U2WMessage<T>` 구조로 단방향 전송
- `U2WRequest<T>` + `UniTaskCompletionSource` 로 요청 / 응답 패턴 지원

> 메시지 타입과 핸들러 등록 프로토콜의 상세는 [ARCHITECTURE.md](./ARCHITECTURE.md) 를 참조하세요.

---

## 추가 문서

| 문서 | 용도 |
| --- | --- |
| [ARCHITECTURE.md](./ARCHITECTURE.md) | 계층 구조 · 의존성 규칙 · 매니저 상세 |
| [UNITY_CODING_CONVENTION.md](./UNITY_CODING_CONVENTION.md) | 네이밍 · 금지 항목 · 비동기 · 멤버 정렬 규칙 |
| [CLAUDE.md](./CLAUDE.md) | AI 페어 프로그래밍 컨텍스트 |

---

> 현재 내부 개발 중 · Bundle Version `0.1.0` · 외부 공개 전
