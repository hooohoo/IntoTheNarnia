# 공통

- UTF-8/LF, 공백 4칸. 한 파일에 주된 클래스 하나, 클래스명과 파일명을 맞춥니다.
- 타입·공개 API PascalCase, private/지역 변수 camelCase. `_`, `m_` 접두어를 새 코드에 사용하지 않습니다.
- 직렬화 이름 변경에는 FormerlySerializedAs를 붙입니다. 기존 JSON 키와 UnityEvent 연결은 유지합니다. enum 값의 순서를 바꾸지 않습니다.
- 상수는 static readonly를 기본으로 하되 attribute/switch 등 컴파일 상수에는 const를 사용합니다.
- 명시적 Initialize/Release를 구현합니다. 공통 수명주기 어댑터/씬 진입점만 Unity 생성·파괴 콜백을 소유합니다. 물리·uGUI 이벤트 콜백은 입력 경계입니다.
- Update/FixedUpdate/LateUpdate와 Coroutine을 프로젝트 코드에 추가하지 않습니다. UniTask 작업은 Async 접미사, 취소 토큰, 취소·해제 쌍을 갖춥니다. 취소 예외를 오류로 기록하지 않습니다.
- LINQ/일반 람다를 사용하지 않습니다. 외부 API가 요구하는 콜백은 최소한으로 허용합니다. Unity Test Framework가 요구하는 IEnumerator/ToCoroutine은 테스트 어셈블리에서만 허용합니다.
- 컴포넌트는 초기화 때 캐시합니다. [FindComponents] 또는 명시적 serialized reference를 사용합니다. FindComponents는 단일 Component 필드만 지원합니다. 씬 범위 컬렉션은 SceneBinding에서 구성하여 전달합니다.
- 전역 Find/Camera.main은 일반 객체에서 금지합니다. 씬 구성이나 바인딩 인프라에서 한 번 검색하여 전달할 수 있습니다. 경계의 누락 참조는 원인을 포함한 오류로 보고합니다.
- 일반 코드에서 Singleton.Instance를 직접 참조하지 않습니다. [Singleton] 주입 또는 생성자 인자를 사용합니다.
- UI/비동기/이벤트의 소유권은 필드와 Release에서 확인 가능해야 합니다. 중복 Init, 재활성화, 종료 중 콜백을 고려합니다.
- 멤버 순서는 상수 → static → 직렬화/인스턴스 필드 → 프로퍼티 → 초기화 → 공개 API → 내부 구현 → 해제로 정리합니다.

기존 예제는 Samples에 격리하되 컴파일·씬 참조를 유지합니다. 서드파티와 Unity 생성 코드는 별도 규칙을 적용합니다.
