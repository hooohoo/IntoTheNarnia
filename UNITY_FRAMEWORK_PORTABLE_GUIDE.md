# IntoTheNarnia 프레임워크 적용 가이드

이 파일은 Chalk 코드를 그대로 붙여 넣는 템플릿이 아닙니다. 기존 전체 템플릿은 Git 이력 `50afba21`에서 확인할 수 있습니다. 현재 프로젝트의 구현 기준은 ARCHITECTURE.md와 Roles 문서입니다.

위 단계는 이 저장소에 적용했습니다. 같은 기반을 확장할 때 아래 순서와 검증 기준을 유지합니다.

1. Unity 2021.3.6f1/Built-in 및 실제 패키지 버전을 유지합니다. UniTask를 버전 고정하여 추가합니다.
2. IManager, Singleton, 의존성 주입, 위상 정렬, Facade 부트스트랩을 먼저 구현하고 누락·순환·해제 순서를 검증합니다.
3. 수명주기 어댑터와 프레임 루프를 만들고 개별 Update/Coroutine을 취소 가능한 작업으로 옮깁니다.
4. 기존 8개 서비스를 책임별로 정리하고 Event/Input/GameState 서비스를 추가합니다. 불필요한 Flutter/API/HexGrid 매니저는 도입하지 않습니다.
5. 씬 진입점을 명시하고 Title 경유 및 Professor_House 직접 실행을 지원합니다. 화면과 상태, 데이터 Provider와 UI를 분리합니다.
6. 기존 컴포넌트 GUID, 필드·UnityEvent·JSON 호환을 유지하며 폴더를 .meta와 함께 옮깁니다.
7. 컴파일, 서비스/파서/이벤트 테스트, 씬 참조와 플레이 흐름 검증 후 문서를 실제 구현에 맞춥니다.

Resources는 이 프로젝트에서 사용하는 콘텐츠 제공 방식입니다. Addressables, URP, Flutter가 필요한 별도 프로젝트에서는 패키지·리소스 설정을 함께 설계해야 합니다. 이름만 바꾸어 도입했다고 기록하지 않습니다.
