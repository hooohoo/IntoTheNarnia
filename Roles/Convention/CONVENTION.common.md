# 공통 컨벤션

- UTF-8/LF, 공백 4칸. JSON/asmdef는 2칸. 한 파일에 주 런타임 타입 하나, 타입명과 파일명을 맞춘다.
- 공개 타입·API PascalCase, private/지역 변수 camelCase. 신규 `_`, `m_` 접두어를 금지한다. 신규 상수는 static readonly, 컴파일 상수가 필요한 경우만 const다.
- 명시적 Init/Initialize와 Release를 구현한다. Awake/Start 및 Unity 생성·파괴 콜백은 프레임워크·씬 초기화 경계가 소유한다.
- Update/FixedUpdate/LateUpdate, 코루틴, LINQ, 일반 람다를 금지한다. 외부 콜백 API와 Unity Test Framework의 요구는 예외다. 비동기는 취소 가능한 UniTask, Async 접미사, 명시적 Forget을 사용한다.
- 매니저는 [Singleton] 주입, 정적 컴포넌트는 [FindComponents] 또는 직렬화 참조를 사용한다. InjectUtil.InjectSingleton/InjectComponents가 공통 진입점이다. Component 타입만 자동 바인딩한다.
- 일반 코드의 Singleton.Instance, Camera.main, 전역 Find, 반복 GetComponent를 금지한다. 씬 구성·런타임 생성 인프라의 1회 검색은 허용하고 캐시/명시적 참조로 전달한다.
- List/배열·프레임 경로는 인덱스 for, Dictionary/Set은 foreach를 사용한다. 프레임마다 임시 컬렉션을 할당하지 않는다.
- 멤버 순서는 상수 → static → 주입 필드 → 직렬화/인스턴스 필드 → 프로퍼티 → 초기화 → 공개 API → 내부 구현 → 해제다.
- 로깅은 LogUtil, 클래스 태그는 private static readonly string LogTag다. 진단 로그만 DEBUG_LOG로 조건부 제거하고 오류/실패는 배포에서 숨기지 않는다.
- 내부 의존성 누락을 조용히 넘기지 않는다. 경계의 실패는 원인을 보존한다. 부분 초기화도 해제하며 정리 실패가 원래 오류를 덮어쓰지 않는다.
- UI·이벤트·입력·취소 토큰·캐시·에셋 핸들은 소유자를 명시하고 실제 수명주기에 맞춰 정리한다.
- 기존 직렬화 이름은 FormerlySerializedAs, JSON 키는 JsonProperty로 호환한다. enum 값·UnityEvent 진입점은 보존한다.
- 에셋은 .meta와 함께 이동한다. 신규 Prefab은 UI_/OBJ_/CAM_ 및 컴포넌트 역할, Material MAT_, Texture TEX_, Model FBX_, 애니메이션 대상@동작, AnimatorController 대상명 규칙을 따른다. 기존 콘텐츠 이름은 개별 마이그레이션 없이 일괄 개명하지 않는다.
- Addressable 그룹/라벨은 씬·피처명이다. 단순 Textures/Prefabs 등 에셋 종류 그룹을 금지한다. 라벨 없는 Dependencies 같은 인프라 그룹은 예외다. 라벨은 AddressableLabels, 주소는 AssetKeys를 사용하며 주소는 확장자 없는 실제 파일명이다. 동명 주소를 허용하지 않는다.
- 자체 런타임 Resources.Load(All)을 금지한다. 샘플의 에디터 전용 로더는 UNITY_EDITOR 조건 안에서 AssetDatabase를 쓸 수 있다. 패키지 내부 특수 Resources는 별도 소유다.
