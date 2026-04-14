# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity 프로젝트 (chalk_4_game) - Flutter 앱에 임베딩되는 교육용 3D 게임. 학습 콘텐츠를 은하/노드/블록 기반 헥스 그리드로 시각화하며, Flutter와 양방향 메시징으로 통신한다.

- **Unity Version**: URP (Universal Render Pipeline) 14.x 기반
- **Async**: UniTask 사용 (코루틴 사용 금지)
- **주요 패키지**: Addressables, Cinemachine, TextMeshPro, Newtonsoft.Json, FlutterUnityIntegration

## Build & Development

Unity Editor에서 직접 빌드. CLI 빌드 스크립트 없음.

```bash
# Solution 파일
chalk_4_game.sln

# 프로젝트 Assets 기준 경로
Assets/@Project/
```

## Architecture

### FacadeManager 기반 싱글톤 시스템

`FacadeManager`가 16개의 `Singleton<T>` 매니저를 리플렉션(`[Singleton(typeof(...))]` 어트리뷰트)으로 수집하고, `DependencyResolver`가 Kahn's Algorithm 위상 정렬로 초기화 순서를 결정한다.

**계층 구조:**
```
Presentation:  UIManager, LoadingManager
Game Logic:    GameStateManager, HexGridManager, InputManager
Core Service:  EventManager, GameSceneManager, CameraManager
Data/Resource: DataManager, AddressableManager, ObjectManager, APIManager, SoundManager, FactoryManager
Platform:      FlutterMessageManager (Flutter ↔ Unity Bridge), WebMessageManager (Web ↔ Unity Bridge)
```

**의존성 주입**: `[Singleton(typeof(T))]` 어트리뷰트 + `InjectUtil.InjectSingleton(this)` 패턴으로 매니저 간 참조를 주입.

### 매니저 간 통신 3가지 방식
1. **[Singleton] 직접 참조** - Init 시 어트리뷰트 기반 주입, 동기 호출
2. **EventManager 이벤트** - `PostNotification<TEnum>()` / `OnEvent()` 간접 통신
3. **AddressableManager 리소스** - 프리팹/에셋 비동기 로드

### GameState FSM

`GameStateManager`가 `IGameState` 구현체를 관리하는 상태 머신. 상태 전환 시 `EventManager`를 통해 `GameStateEvent` 발행.

**상태 흐름**: Dashboard → Depth0 → Depth1 → Depth1.5 → Depth2 (auto→) Depth3

**씬별 상태 등록**:
- **Main 씬** (`Main.cs`): LobbyState, DashboardState, Depth1State, Depth1_5State, Depth1_5RestoreState, StyleRoomState
- **Planet 씬** (`Planet.cs`): Depth2State, Depth3State, Depth2RestoreState

### UI 3계층 시스템

`UIManager`가 관리하는 3종류의 UI:
- **UI_Scene** (sortOrder 0~99): Dictionary 캐싱, 고정 HUD
- **UI_Popup** (sortOrder 100+): Stack(LIFO), +10씩 증가
- **UI_Sub**: Queue 기반 오브젝트 풀링, 부모 UI 자동 연동

모든 UI는 `UI_Base`를 상속하며, Addressable로 프리팹을 비동기 로드한다.

### Data Layer - Provider 패턴

`DataManager`(partial class 6파일)와 `APIManager`(partial class 4파일)가 인터페이스 기반 Provider로 데이터 소스를 추상화:
- `IKnowledgeGraphProvider` → `JsonKnowledgeGraphProvider`
- `IAvatarDataProvider` → `JsonAvatarDataProvider` / `ApiAvatarDataProvider`
- `ITokenProvider` → `FlutterTokenProvider` / `MockTokenProvider`

### Flutter ↔ Unity 통신

`FlutterMessageManager`(partial class 3파일)가 Flutter와 JSON 메시지 기반 양방향 통신을 담당. `pendingRequests`에 `UniTaskCompletionSource`를 사용한 비동기 요청/응답 패턴.

### Web ↔ Unity 통신

`WebMessageManager`(partial class 3파일)가 Web(JavaScript)과 JSON 메시지 기반 양방향 통신을 담당. `U2WMessageType` 상수 + `U2WMessage<T>` 구조로 단방향 전송, `U2WRequest<T>` + `UniTaskCompletionSource`로 요청-응답 패턴 지원.

## Project Structure

```
Assets/@Project/
├── 00.Scenes/          # Unity 씬 파일
├── 01.Scripts/
│   ├── Base/           # 기반 클래스
│   ├── Manager/
│   │   ├── Facade/     # FacadeManager, DependencyResolver
│   │   ├── Interface/  # IManager 등 매니저 인터페이스
│   │   └── Manager/    # 15개 싱글톤 매니저 (partial class 포함)
│   ├── Singleton/      # Singleton<T> 제네릭 베이스
│   ├── Scene/          # 씬별 초기화 (Main.cs, Planet.cs) + Screen 클래스
│   ├── UI/             # UI_Base, UI_Scene, UI_Popup, UI_Sub, Component
│   ├── Game/           # HexGrid 게임 로직
│   ├── Data/           # 데이터 모델, Provider 구현체
│   ├── Enum/           # GameStateType, SceneEvent 등 열거형
│   ├── Obj/Object/     # 게임 오브젝트 관련
│   ├── Rendering/      # 렌더링 유틸리티
│   └── Util/           # InjectUtil, 공통 유틸리티
├── 02.Prefabs/
├── 03.Sprites/
├── 04.Materials/
├── 05.Animations/
├── 06.Data/            # JSON 데이터 파일
├── 07.Sounds/
└── 08.Fonts/
```

## Coding Convention (핵심 요약)

자세한 내용은 `UNITY_CODING_CONVENTION.md` 참조.

- **한 파일 = 한 클래스**, 파일명과 클래스명 동일
- **private 필드**: camelCase (`_`, `m_` 접두어 금지)
- **UI 클래스**: `UI_` 접두어 (예외적으로 언더바 허용)
- **비동기 메서드**: `Async` 접미어, UniTask 사용
- **상수**: `static readonly` 권장 (`const`보다 선호)
- **Update()/FixedUpdate() 사용 금지** - UniTask 기반 루프 또는 `Initialize()` 메서드 사용
- **Awake()/Start()**: 프레임워크/씬 초기화에만 허용, 일반 객체는 `Initialize()`
- **람다식 사용 금지** (외부 라이브러리 호환 시에만 허용)
- **LINQ 사용 금지** - foreach 반복문으로 직접 구현
- **Camera.main 사용 금지** - CameraManager를 통해 접근
- **델리게이트**: 해당 객체 내부에서만 사용, 외부 체인 연결 금지
- **매직 넘버/스트링 금지** - 상수 클래스로 관리
- **멤버 정렬**: 상수 → static → public → SerializeField → private → 프로퍼티 → Unity 콜백 → Initialize → public 메서드 → private 메서드 → Release

## Architecture Review

변경사항은 `ARCHITECTURE.md`의 계층 구조와 일치해야 한다. 매니저 간 의존성 추가 시 `[Singleton(typeof(...))]` 어트리뷰트를 정확히 설정하고, 순환 의존이 발생하지 않는지 확인할 것.

## Git Conventions

- **Main Branch**: `dev`
- **커밋 메시지**: 한글, `feat:` / `fix:` / `refactor:` 등 타입 접두어 사용
- **Co-Authored-By 문구 제거**: 커밋 메시지에 "Generated with Claude Code" 등의 자동생성 문구 포함하지 않음
