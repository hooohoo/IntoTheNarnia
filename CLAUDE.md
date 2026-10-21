# IntoTheNarnia 작업 지침

## 읽는 순서

README.md → ARCHITECTURE.md → UNITY_CODING_CONVENTION.md → 해당 Roles 문서. Chalk 기준 차이와 예외는 Docs/Architecture/ChalkParity.md, 실행 결과는 Docs/Validation/ChalkParity.md를 확인한다. 사용자의 명시적 지시가 일반 문서 규칙보다 우선한다.

## 프로젝트 기준

Unity 2022.3.62f3, URP 14.0.12/Linear, Addressables 1.22.3, Cinemachine 2.10.5, TMP 3.0.7, Newtonsoft 3.2.2, UniTask 2.5.10. 자체 게임 에셋은 Resources.Load(All)을 사용하지 않는다. Addressable 프리로드 → 캐시 조회 → 인스턴스 사용 → 인스턴스 정리 → 소유 에셋 해제가 기본 계약이다.

FacadeManager와 위상 정렬 DI, 명시적 Init/Release, SceneRoot Ready, 씬별 상태 등록, InputHandlerBase, UI_Base/Scene/Popup/Sub, 도메인 enum 이벤트, Provider, LogUtil을 적용한다. 런타임은 01.Scripts, 에디터·테스트·예제는 각각 Editor/Tests/Samples에 둔다.

## 구현 원칙

- 매니저 필드는 [Singleton(typeof(T))], 컴포넌트 바인딩은 [FindComponents] 또는 명시적인 직렬화 참조를 사용한다. 일반 코드에서 Singleton.Instance, 전역 Find, Camera.main을 사용하지 않는다.
- Update/FixedUpdate/LateUpdate, Coroutine, LINQ를 사용하지 않는다. 취소 가능한 UniTask와 공통 FrameLoop를 사용한다. 테스트 어셈블리의 UnityTest IEnumerator는 예외다.
- 비동기 메서드는 Async 접미사, 반환값을 버릴 때 Forget을 명시한다. 취소·이벤트·드래그·풀의 정리는 실제 소유자의 수명주기에 맞춘다.
- Addressable 그룹/라벨은 씬·피처명, 주소는 확장자 없는 파일명이다. 동명 주소를 금지한다. 인프라 Dependencies 그룹은 라벨 없이 둘 수 있다.
- 씬 필수 에셋 준비가 실패하면 초기 UI와 입력을 열지 않는다. Unity 씬 로드 완료와 게임 SceneReady를 구분한다.
- UIManager가 프리로드 UI를 생성하고 Scene 캐시·Popup 순서·Sub 풀을 관리한다. State는 UI 계층을 검색하지 않는다. 복합 UI는 MVP/MVC로 책임을 나눈다.
- private camelCase, 공개 API PascalCase, 신규 상수 static readonly, UTF-8/LF·4칸, JSON/asmdef 2칸. 로그는 LogUtil과 클래스 LogTag를 사용한다.
- 에셋 이동은 .meta와 함께 한다. 직렬화 개명은 FormerlySerializedAs, JSON 계약은 JsonProperty로 보존한다. 태그/레이어/NavMesh Area와 애니메이션 파라미터를 임의로 바꾸지 않는다.
- Flutter/Web/NGO/AWS 목적지와 제품 식별자는 공통 규칙이 아니다. 신규 서비스 연결에는 실제 Narnia 요구와 설정이 있어야 한다.

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
