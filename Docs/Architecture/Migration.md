# 프로젝트 구조 전환 기록

2026-10-21, 기준선 `486a49e8` 이후 적용했습니다. 기존 변경·백업·문서 75개는 먼저 커밋, push, main fast-forward 병합했습니다. GitHub 연결 앱이 PR 생성에 403을 반환하여 사용자에게 알린 뒤, 이미 push 가능한 Git 인증으로 보호 규칙에 따라 병합했습니다.

## 구현 결과

- FacadeManager, ServiceRegistry, Kahn 의존성 정렬, Singleton 주입, FindComponents 및 RuntimeBehaviour/FrameLoop를 도입했습니다.
- 기존 8개 서비스를 11개 서비스로 구성했습니다. 씬 로딩, 입력, 이벤트, 상태, 데이터, UI, 카메라와 퀘스트의 소유권을 구분했습니다.
- 자체 게임 코드의 Update/FixedUpdate/LateUpdate 및 코루틴을 제거했습니다. UniTask 작업은 취소 토큰과 해제 경로를 갖습니다.
- 46개 기존 스크립트 GUID를 유지하고 CP949 35개를 UTF-8로 전환했습니다. 각 대응 경로는 ScriptMigration.json에 기록했습니다.
- 자체 씬·스크립트·Resources·모델·라이팅·애니메이션·Polybrush·UI Toolkit 자료를 Assets/@Project로 모았습니다. 외부 에셋 및 Unity 특수 경로는 유지했습니다.
- 직렬화 필드에는 FormerlySerializedAs, 데이터에는 JsonProperty를 적용했습니다. 기존 UnityEvent 메서드와 enum 순서, Resources 상대 키, 태그/Area 이름을 보존했습니다. 어셈블리 분리에 맞춰 UnityEvent 대상 타입 이름을 갱신했습니다.
- 연결 JSON을 안전하게 읽는 Provider와 표준 배열 지원을 추가했습니다. 재로드 시 대사가 중복 누적되지 않으며 저장은 persistentDataPath를 사용합니다.
- 퀘스트 진행은 문 이벤트로 갱신합니다. 문 영역은 비트 OR로 해제하고 반복 상호작용에서도 상태가 누적되지 않습니다. 씬 재진입 시 같은 세션의 진행을 유지합니다.
- 대화·설정·로딩 중 입력을 차단하고 중복 씬 이동을 무시합니다. 카메라 입력 델리게이트와 씬 참조를 종료 시 복원/해제합니다.
- 오래된 Joystick_ 예제 프리팹의 대상이 null인 EventTrigger 콜백 5개를 제거했습니다. 실제 입력은 기존 JoystickController 드래그 인터페이스가 처리합니다.

## 보존 범위와 검증

교수의 집의 벽 메시 817개, 천장 메시 3개, 메시 참조 2,663개, 러그 재질 2개 및 라이트맵 37개를 유지합니다. 씬 8개와 빌드 씬 5개, Inspector/UnityEvent 연결을 검사했습니다. 샘플은 삭제하지 않았습니다. 두 방의 신규 콘텐츠, Title의 미구현 메뉴, 엔진/렌더러 교체는 이 구조 개편에 추가하지 않았습니다.

검증 결과와 Unity 2021 에디터 라이선스 제한은 [검증 기록](../Validation/2026-10-21.md)에 명시합니다. 최종 통합 커밋은 Git 이력을 기준으로 확인합니다.
