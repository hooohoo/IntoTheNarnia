# State

SceneRoot가 상태를 등록하고 Loading에서 프리로드·구성을 마친 뒤 초기 상태로 전환한다. 전역 서비스 Init에서 일괄 상태 등록하지 않는다. GameStateManager는 중첩 UI 상태를 보관하며 덮인 상태의 종료도 제거한다. 이벤트 처리 중 재진입 전환은 현재 알림 이후 실행한다. State는 UI 생성·검색을 소유하지 않는다.

[전체 구조](../../ARCHITECTURE.md)를 함께 적용한다.
