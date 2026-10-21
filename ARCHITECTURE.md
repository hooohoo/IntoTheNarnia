# IntoTheNarnia 아키텍처

문서 기준일: 2026-10-21. 아래 구조는 구현된 코드 기준입니다. 전환 전 구조는 [기준선 기록](Docs/Status/2026-10-21-baseline.md), 적용 내역과 검증 제한은 [Migration](Docs/Architecture/Migration.md)을 확인합니다.

## 실행 구조

FacadeManager가 런타임 시작 전에 11개 서비스를 등록합니다. ServiceRegistry가 의존성을 정렬·주입하고 초기화하며, 부분 초기화 실패도 역순으로 정리합니다. GameManager는 기존 씬의 MonoScript GUID를 보존하는 호환 컴포넌트입니다.

FieldManager는 카메라, 플레이어 생성, HUD 연결을 구성합니다. ProfessorHouseMapManager는 문·포털을 연결하고 초기 대화를 요청합니다. QuestManager는 문 이벤트로 진행하며 같은 플레이 세션의 씬 재진입에서도 진행을 유지합니다. LoadingController는 로딩 표시만 담당하고 GameSceneManager가 중복 요청을 차단하며 실제 비동기 씬 전환을 소유합니다.

## 폴더 구조

```text
Assets/@Project/
  00.Scenes/                 게임 씬, 라이트맵, NavMesh, 씬 전용 메시·재질
  01.Scripts/
    Framework/              Facade, Singleton, DI, 수명주기, 프레임 루프
    Manager/                전역 서비스
    Scene/                  Title/Loading/필드 구성과 화면
    State/                  게임 상태와 입력 정책
    UI/                     View/Presenter/Model, UI 입력 어댑터
    Game/                   캐릭터, 문, 포털, 퀘스트
    Data/                   DTO, Provider, 저장
    Util/                   공통 상수·열거형·바인딩
    Samples/                교사 예제, 프로토타입 코드
    Editor/                 검증 도구
    Tests/                  EditMode/PlayMode 검증
  02.Resources/Resources/   기존 Resources 상대 경로 유지
  03.Models/                프로젝트 모델
  04.Environment/           프로젝트 라이팅 설정
  05.Animations/            프로젝트 Animator Controller
  06.LevelData/             Polybrush 설정
  07.UI Toolkit/            프로젝트 UI Toolkit 설정
```

Assets/Asset, TextMesh Pro, Plugin 등 외부 패키지와 Unity 생성 폴더, StreamingAssets 특수 경로는 원래 경로에 유지합니다. 무관한 리소스의 일괄 이름 변경은 하지 않습니다. 자체 씬·스크립트·데이터·프리팹·모델·애니메이션은 새 루트로 모으고 GUID를 보존합니다.

## 서비스와 소유권

| 서비스 | 책임 | 핵심 의존성 |
| --- | --- | --- |
| EventManager | 타입 기반 구독/해제와 동기 알림 | 없음 |
| ResourceManager | Resources 콘텐츠 캐시와 검색 | 없음 |
| DataManager | 대화 Provider 및 저장 경로 | ResourceManager |
| ObjectManager | 현재 씬의 플레이어·상호작용 대상 참조 | 없음 |
| InputManager | 이동·카메라 입력, 상태별 입력 허용 | 없음 |
| CameraManager | 현재 카메라 및 Cinemachine 연결 | ObjectManager, InputManager |
| UIManager | 활성 화면과 HUD 바인딩 | EventManager, GameStateManager |
| QuestManager | 퀘스트 진행·문 해제, 이벤트 발행 | EventManager, ObjectManager |
| FactoryManager | 플레이어 생성 및 씬 소유권 | ResourceManager, ObjectManager |
| GameStateManager | 상태 Enter/Exit, 입력 정책 | InputManager, EventManager |
| GameSceneManager | 비동기 씬 이동·중복 요청 차단 | GameStateManager |

의존성은 실제 필드의 [Singleton] 선언이 기준입니다. FacadeManager가 명시적인 목록을 등록하고 Kahn 위상 정렬로 초기화합니다. 부팅 실패 시 초기화한 서비스만 역순으로 해제합니다. 씬 종료 시 씬 참조를 해제하고 애플리케이션 종료 시 전역 서비스를 해제합니다. 게임 씬을 직접 실행해도 부트스트랩이 동작해야 합니다.

## 흐름

부트스트랩 → 서비스 Init → 씬 구성 → 객체·HUD·카메라 연결 → 상태 Enter.
상태는 입력 전략과 게임 진행만 담당합니다. 화면은 상태 이벤트를 받아 UI를 갱신합니다. 문 상호작용 → DoorChanged 이벤트 → QuestManager의 진행·잠금 갱신 → QuestChanged 이벤트로 연결합니다. 현재 HUD는 StateChanged를 구독하며 QuestChanged는 확장용 도메인 알림입니다. 씬 이동은 Loading 상태를 거쳐 목표 씬이 준비된 뒤 상태를 전환합니다.

비동기 작업은 UniTask로 처리하며 소유자의 취소 토큰을 받습니다. 컴포넌트 반복 작업은 공통 프레임 루프에 등록하고 비활성화 시 해제합니다. UI 버튼·드래그·물리 트리거는 Unity 이벤트 진입점을 유지합니다. 외부 라이브러리의 Update는 이 규칙의 대상이 아닙니다.

## 수명주기와 어셈블리

RuntimeBehaviour가 OnEnable에서 DI/컴포넌트 바인딩과 Initialize를 실행하고 OnDisable/OnDestroy에서 프레임 등록·취소 토큰·Release를 정리합니다. IFrameTick/ILateFrameTick만 Facade 소유의 UniTask FrameLoop에 등록합니다. 입력 갱신이 게임 객체 갱신보다 먼저 실행됩니다. FindComponents는 현재 단일 Component 필드의 자기 자신/상대 경로 바인딩을 지원합니다. 컬렉션 탐색은 SceneBinding의 씬 범위 검색으로 수행합니다.

IntoTheNarnia.Runtime에 자체 런타임과 호환 샘플을 배치합니다. Editor, EditModeTests, PlayModeTests는 별도 asmdef입니다. link.xml은 리플렉션 주입/JSON 필드가 스트리핑되지 않도록 보존합니다. 샘플의 기존 MonoScript GUID와 씬 참조는 남아 있으며, Test_QuestBoxController 같은 호환 트리거도 보존합니다.

MessageBoxController는 직렬화 View와 기존 UnityEvent 진입점을, DialoguePresenter는 표시 진행·타이핑 취소·상태 복구를, DialogueModel은 대사 큐를 소유합니다. 기존 버튼 바인딩이 있으면 같은 콜백을 다시 등록하지 않습니다. FieldScreen이 HUD 표시를 담당하고 UIManager가 상태 이벤트와 설정 팝업을 연결합니다.

## 호환성 경계

기존 MonoBehaviour의 .meta GUID와 UnityEvent 메서드를 유지하거나 명시적인 어댑터로 연결합니다. 필드 이름 변경에는 FormerlySerializedAs, 데이터 필드 이름에는 JSON 매핑을 사용합니다. Lucy, UI 프리팹, 대화 데이터의 Resources 상대 경로를 유지합니다. 기존 연결 JSON 형식을 읽되 표준 배열도 지원하도록 Provider를 분리합니다. 저장은 persistentDataPath를 사용합니다.

교수의 집의 817개 벽 메시와 3개 천장 메시, 2개 러그 재질, 37개 라이트맵 및 NavMesh는 구조 전환의 보존 대상입니다. SceneBackups의 과거 검사 결과를 현재 실행 검증으로 간주하지 않습니다.

## 상세 규칙

[State](Roles/Architecture/ARCHITECTURE.State.md) · [UI](Roles/Architecture/ARCHITECTURE.UI.md) · [Input](Roles/Architecture/ARCHITECTURE.Input.md) · [Event](Roles/Architecture/ARCHITECTURE.Event.md) · [Data](Roles/Architecture/ARCHITECTURE.Data.md)
