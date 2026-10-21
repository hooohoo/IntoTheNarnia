# IntoTheNarnia 작업 지침

## 읽는 순서

README.md → ARCHITECTURE.md → UNITY_CODING_CONVENTION.md → 해당 Roles/Architecture 및 Roles/Convention 문서를 읽습니다. 전환 중에는 Docs/Architecture/Migration.md의 상태를 확인합니다. 사용자의 명시적 작업 지시가 문서의 일반 지침보다 우선합니다.

## 프로젝트 기준

Unity 2021.3.6f1, Built-in 렌더러, Cinemachine 2.8.9, 기존 Input Manager를 사용합니다. Flutter, Web bridge, HexGrid, URP 프로젝트가 아닙니다. 엔진·렌더러 교체는 구조 개편에 포함하지 않습니다. Resources 경로는 콘텐츠 계약이며 ResourceManager를 통해 접근합니다. Addressables는 현재 사용하지 않으며 이름만 다른 가짜 래퍼를 만들지 않습니다.

현재 구조는 Assets/@Project 아래의 Framework, Manager, Scene, State, UI, Game, Data, Samples, Editor, Tests입니다. FacadeManager가 11개 서비스를 등록합니다. GameManager는 기존 씬 GUID를 유지하는 호환 컴포넌트입니다. 상세 이동표와 검증 범위는 Docs/Architecture/Migration.md를 확인합니다.

## 구현 원칙

- FacadeManager에 매니저를 명시적으로 등록하고 [Singleton] 의존성을 주입합니다. 의존성 순서로 Init, 역순으로 Release합니다. 미등록·중복·순환 의존성은 즉시 오류로 처리합니다.
- 일반 코드에서 Singleton.Instance를 직접 호출하지 않습니다. 런타임 부트스트랩과 직렬화 호환 어댑터만 예외입니다.
- Initialize/Release의 소유자를 명확히 합니다. 컴포넌트의 Unity 수명주기를 연결하는 공통 기반 클래스와 씬 진입점만 Awake/OnEnable/OnDisable/OnDestroy를 사용합니다.
- Update/FixedUpdate/LateUpdate, Coroutine, LINQ, 일반 람다를 사용하지 않습니다. 반복 작업은 취소 가능한 UniTask와 명시적인 프레임 수신 인터페이스로 처리합니다.
- 상태는 게임 흐름과 입력 전략을, 화면은 UI 표시를 소유합니다. 상태에서 UI 계층을 검색하지 않습니다. 퀘스트 진행은 매 프레임 폴링 대신 이벤트로 전달합니다.
- UI는 복잡도에 따라 Model/Presenter/View를 분리합니다. 단순 버튼·표시 컴포넌트는 Controller로 유지할 수 있습니다. 바인딩·입력·이벤트·비동기 작업은 재활성화와 파괴 시 정리합니다.
- private 필드는 camelCase, 공개 API는 PascalCase, 비동기 메서드는 Async 접미사를 사용합니다. UTF-8/LF, 공백 4칸을 사용합니다.
- 기존 직렬화 필드를 바꿀 때 FormerlySerializedAs를 붙이고 .meta GUID를 보존합니다. UnityEvent 메서드와 JSON 키는 명시적으로 호환합니다.
- Find 계열은 씬 구성/바인딩 기반 코드에 한정합니다. 일반 코드의 Camera.main, 반복 GetComponent는 금지합니다. 캐시된 참조를 주입하거나 전달합니다.
- 에셋은 .meta와 함께 이동합니다. 서드파티 코드는 임의로 재작성하지 않습니다. Resources 상대 경로, Animator의 State 파라미터, 태그·NavMesh Area 이름을 보존합니다.

## 검증 및 Git

- **작업 브랜치 필수**: 파일을 수정하기 전에 현재 브랜치를 확인하고, 최신 `origin/main`을 기준으로 새 작업 브랜치를 만듭니다. 후속 작업도 새 브랜치에서 진행하며 이미 머지한 브랜치를 재사용하지 않습니다.
- **main은 병합 전용**: `main`에서 파일을 수정하거나 일반 작업 커밋을 만들지 않습니다. 작업 브랜치의 변경은 두 부모를 가진 merge commit으로만 통합하며, 이 병합 결과를 원격 `main`에 push하는 것은 허용합니다.
- **1 push · 1 merge**: 문서·코드 정리와 관련 검증을 끝낸 뒤 **작업 브랜치 커밋 1개 → main에서 merge commit 1개 → push 1회**로 마무리합니다. 기본은 PR 없이 로컬에서 병합하는 흐름이며, 중간 상태를 나누어 push하지 않습니다.
- **병합 방식**: `git merge --no-ff <작업브랜치>`를 사용합니다. 작업 반영을 fast-forward, squash, rebase merge로 대체하지 않습니다. 병합 전 fetch하고 원격 `main`과 동기화합니다. 원격에 이미 있는 이력을 로컬 `main`에 가져오는 `--ff-only` 동기화는 허용합니다.
- **push 방식**: 로컬 병합을 확인한 뒤 `git push --atomic origin main <작업브랜치>`로 merge commit과 작업 브랜치를 한 번에 올립니다. 작업 커밋을 병합 없이 `main`에 직접 push하지 않습니다.
- **PR은 선택 사항**: 별도로 요청받지 않았다면 PR 생성·승인·GitHub 앱 쓰기 권한을 필수 조건으로 요구하지 않습니다. 사용자가 승인한 Git 병합과 push를 다시 확인하지 않습니다. 저장소 보호 규칙이 병합 결과의 push를 거부하면 실제 차단 사유를 보고하며 우회하지 않습니다.
- **커밋 형식**: 작업 커밋과 merge commit 모두 `type : 한글 설명` 형식을 사용합니다. 생성 도구 광고/Co-Authored-By를 추가하지 않으며 force push하지 않습니다.

코드 변경은 Unity 컴파일과 관련 EditMode/PlayMode 검증을 수행합니다. 씬 이동 시 Build Settings, GUID, 프리팹 참조, 라이트맵·NavMesh 의존성을 검사합니다. 실행하지 않은 검증을 통과했다고 쓰지 않습니다. SceneBackups는 원본 복원 및 이전 검증 증거이며 새 검증 결과와 구분합니다.

비밀 설정과 개인 인증 정보는 문서·로그에 출력하지 않습니다. .claude/settings.local.json의 실행 권한은 설계 규칙이 아닙니다.
