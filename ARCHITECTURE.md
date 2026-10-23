# IntoTheNarnia 아키텍처

Chalk의 공통 설계 계약을 Narnia 게임 도메인에 적용한다. 엔진·패키지와 프레임워크 계약은 [통합 점검](Docs/Architecture/ChalkParity.md)을 따른다. 제품 ID, 씬·퀘스트·태그·NavMesh, 서비스 연동 목적지는 Narnia 소유다.

## 폴더

```text
Assets/@Project/
  00.Scenes/          씬과 씬 전용 베이크·메시
  01.Scripts/
    Base/            RuntimeBehaviour, FrameLoop, 주소·라벨 상수
    Manager/Facade/  FacadeManager, ServiceRegistry, DependencyResolver
    Manager/Interface/
    Manager/Manager/ 전역 매니저와 도메인별 State/Handler
    Singleton/
    Scene/           SceneRoot와 씬 구성, Screen
    UI/              Base, UI_Scene, UI_Popup, 컴포넌트와 대화 MVP
    Object/          OBJ_Base, 캐릭터·문·퀘스트
    Data/            DTO·Provider·저장 데이터
    Enum/            도메인 이벤트
    Util/            DI, 바인딩, 로그
  02.Prefabs/        UI·캐릭터·환경 프리팹
  03.Sprites/        UI 이미지·텍스처
  04.Materials/
  05.Animations/
  06.Data/
  07.Sounds/
  08.Fonts/
  09.Shaders/
  10.Models/
  Settings/          URP, 라이팅, Polybrush, UI Toolkit
  Editor/            이관·검증·빌드 도구
  Tests/             EditMode, PlayMode
  Samples/           별도 예제 어셈블리 (빌드 씬 참조 금지)
```

Asset Store, TextMesh Pro 등의 외부 폴더는 원래 소유 경로를 유지한다. TMP의 패키지 설정용 Resources는 자체 게임 Resources와 구분한다. 원본 스크립트 46개와 콘텐츠 .meta GUID를 보존하며 [ScriptMigration.json](Docs/Architecture/ScriptMigration.json)이 현재 목적지를 가리킨다.

## 서비스와 소유권

| 서비스 | 책임 |
| --- | --- |
| EventManager | enum 도메인, IListener, Add/RemoveListener, PostNotification |
| AddressableManager | 라벨/타입별 프리로드 합류, 이름 캐시, 소유자 추적, 핸들 해제 |
| DataManager | Addressable JSON, IDialogueProvider, 데이터 캐시·교체, persistentDataPath 저장 |
| ObjectManager | 씬 소유 비UI Spawn/Despawn 풀, 플레이어·상호작용 참조 |
| InputManager | 장치 입력 수집, 상태별 InputHandlerBase 연결 |
| CameraManager | 현재 카메라·Cinemachine 대상과 입력 델리게이트 연결·복원 |
| UIManager | Scene 캐시, Popup 순서, Sub 풀·부모별 정리, Screen 연결 |
| QuestManager | Narnia 퀘스트 진행과 문·NavMesh 잠금 |
| FactoryManager | 도메인 객체 생성 요청, 플레이어 스폰 위치 결정 |
| GameStateManager | 씬별 상태 등록, 입력 전략 전환, 중첩 상태와 재진입 순서 |
| GameSceneManager | 로딩 씬·목표 씬 전환, SceneRoot Ready 대기 |

FacadeManager가 명시적으로 등록한 서비스를 위상 정렬로 초기화하고 역순 해제한다. 원래 초기화 예외와 정리 예외를 함께 보존한다. RuntimeBehaviour는 주입·바인딩·생명주기·프레임 등록을 담당한다. 일반 객체는 Singleton.Instance를 사용하지 않는다. 복제한 매니저/샘플 MonoScript를 추가로 씬에 붙이지 않는다.

## 리소스와 씬

씬 루트가 상태 등록 → Loading 상태 → 필요한 라벨 비동기 프리로드 → 데이터/객체/UI/카메라/Screen 구성 → 초기 상태 → Ready를 소유한다. GameSceneManager는 Unity의 sceneLoaded만으로 준비 완료를 판단하지 않는다. 필수 프리로드 실패 시 화면 구성과 입력을 열지 않는다.

라벨은 Title / Loading / ProfessorHouse / Story다. 그룹은 같은 피처명이며 주소는 확장자 없는 파일명이다. 공용 번들 의존성만 라벨 없는 Dependencies 그룹을 쓸 수 있다. `AssetKeys`, `AddressableLabels`를 통해 접근한다. 기존 Lucy 등 레거시 에셋 이름은 실제 등록 주소와 일치하며 신규 UI는 `UI_` 이름을 쓴다.

`PreloadAsync<T>(label, owner, token)`은 동일 라벨/타입 진행 중 로드에 합류한다. 같은 소유자의 반복 요청은 중복 획득하지 않는다. `ReleaseOwner` 또는 `ReleaseLabel(label, owner)`로 소유를 반환한다. 다른 소유자가 남으면 캐시를 유지하며, 마지막 소유자가 떠난 진행 중 로드는 완료 시 결과를 폐기하고 핸들을 해제한다. 로드 실패는 로그+false이며 호출 씬이 예외로 진입을 중단한다. 외부 취소는 대기를 취소하므로 소유자는 항상 Release 경로에서 소유를 반환해야 한다.

UIManager/ObjectManager가 프리로드 캐시를 동기 조회해 Instantiate한다. 이 인스턴스는 Addressables.InstantiateAsync 인스턴스가 아니므로 Destroy/풀 반환으로 정리하고, 그 뒤 씬이 에셋 핸들을 해제한다. 핸들 해제가 즉시 모든 메모리 반환을 의미하지는 않는다.

## UI·상태·입력

UI_Base 아래 UI_Scene/UI_Popup/UI_Sub 3계층을 사용한다. Scene UI는 캐시, Popup은 중복 방지와 표시 순서(100 + 깊이×10), Sub는 부모 소유와 OnSpawn/OnDespawn 쌍을 가진다. CanvasScaler와 시각 배치는 프리팹이 소유한다. 현재 화면 프리팹은 Scale With Screen Size, 기준 1920×1080, Match 0.5를 사용한다. 실제 화면은 UI_Title/UI_Loading/UI_Field/UI_Settings/UI_Dialogue다. FieldScreen은 표시 조정, Dialogue Model/Presenter/View는 대사 진행을 맡는다.

상태는 씬 루트가 등록하며 전역 Init에서 게임 상태를 미리 만들지 않는다. 입력 핸들러는 상태마다 한 번 생성해 Enter/Exit에서 연결·초기화한다. ExplorationInputHandler는 이동·카메라, BlockedInputHandler는 로딩/대화/설정의 입력 차단을 담당한다. 키보드·마우스·터치와 조이스틱의 포인터 소유권을 유지하며 UI에서 시작한 드래그는 카메라로 넘어가지 않는다.

## 이벤트·어셈블리·검증

도메인별 enum과 IListener<TEnum>을 사용하고 payload는 명시적으로 캐스팅한다. 이벤트는 수신자 스냅샷을 사용해 등록/해제·중첩 발행을 처리한다. 상태 이벤트 처리 중 전환 요청은 현재 알림 뒤에 실행하여 현재 상태와 알림 순서를 일치시킨다.

Runtime, Editor, Samples, EditModeTests, PlayModeTests를 나눈다. 프로덕션 씬의 퀘스트 트리거는 Object에 있으며 샘플 어셈블리에 의존하지 않는다. link.xml은 리플렉션·직렬화 호환을 위해 Runtime을 보존한다. IL2CPP 타깃 검증 범위는 결과 보고서에 별도로 기록한다.

Chalk 통합 당시 메시 820개·참조 2663개·라이트맵 37개 보존 검사는 이전 환경을 유지하기 위한 전환 기준이었다. 교수의 집 v2 재구성부터는 새 환경 프리팹·FBX의 참조, 라이트맵 UV·베이크 데이터, 계단 이동과 문별 NavMesh Area 연결을 검사한다. 씬 GUID와 `FieldManager`·`ProfessorHouseMapManager`·두 문 이름·`State` 애니메이션 파라미터는 유지한다. [교수의 집 제작 계약](Docs/Art/ProfessorHouseV2.md)과 [검증 결과](Docs/Validation/ProfessorHouseV2.md)가 현재 근거다.

후속 저택 공간 제작의 기준은 2026-10-22 승인된 [확정 도면 v1](Docs/Art/ProfessorHouse_ApprovedLayout_v1/README.md)이다. 전체 방 배치·계단·정원·문 연결·서사 동선을 정의하며 현재 v2 씬 구현과 구분한다. 씬 분할·로딩 경계는 이 접속 관계를 유지하며 후속 구현에서 결정한다.

연결 구조 검증 씬 `Professor_House_Connected`는 `ProfessorHouseWalkthroughRoot : SceneRoot`에서 기존 Addressables 프리로드, Factory/ObjectManager, 입력, CameraManager, UIManager를 사용한다. 원본 구조와 새 마감 참고자료의 등록 및 검증 상태는 [구현 기록](Docs/Art/ProfessorHouseConnected.md)을 따른다. 이 저작용 씬은 제품 Build Settings에 추가하지 않으며 기존 Title → Professor_House 흐름을 유지한다.

중앙 홀 마감은 `OBJ_ProfessorHouse_HallFinish`와 홀 전용 머티리얼을 기존 연결 씬에 추가한다. 가구의 단순 충돌체도 같은 NavMesh 베이크에 포함한다. 연결 씬 카메라가 선택하는 `ProfessorHouseHallRenderer`를 URP 목록에 추가하며 기본 렌더러 인덱스 0은 유지한다. 고정 환경과 그림 텍스처는 직렬화 참조가 소유한다. [중앙 홀 제작 기록](Docs/Art/ProfessorHouseHallFinish.md)을 따른다.

2층 마감은 `OBJ_ProfessorHouse_UpperFinish`를 추가하며, 기존 구조 FBX에서 위치 확인용 침대·책상만 제외한다. `ProfessorHouseHallFinishBuilder.AddLayer`가 홀과 2층의 재질 매핑·축/경계 검증·단순 충돌체 생성을 공유하고, `ProfessorHouseUpperFinishBuilder`가 방별 조명과 내부 접근 검사를 담당한다. 공통 Blender 부품·교차 검사는 `Tools/Art/professor_house_finish_kit.py`에 둔다. [2층 제작 기록](Docs/Art/ProfessorHouseUpperFinish.md)을 따른다.

전시실·복도 마감은 `OBJ_ProfessorHouse_GalleryFinish`와 `ProfessorHouseGalleryFinishBuilder`가 소유한다. 같은 AddLayer 임포트 계약과 NavMesh 베이크를 사용하고, 원본의 임시 갑옷·진열대만 GroundRooms 파생 FBX에서 교체한다. 별도 투구·어깨 메시는 향후 사고 연출의 제작 단위이며 런타임 상호작용은 아직 추가하지 않았다. [제작·검증 기록](Docs/Art/ProfessorHouseGalleryFinish.md)을 따른다.
