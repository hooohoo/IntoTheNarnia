# UI

씬에 배치된 HUD를 FieldManager가 한 번 바인딩하여 UIManager에 제공합니다. 복합 대화 UI는 데이터/표시/진행 제어를 나눕니다. 화면은 이벤트 구독·해제를 책임지고 상태는 UI 계층을 검색하지 않습니다. 설정 창과 대화 창의 입력 정책은 GameStateManager를 통합니다. 기존 버튼 UnityEvent와 프리팹 GUID를 보존합니다.

[전체 구조](../../ARCHITECTURE.md)와 해당 [컨벤션](../Convention/UNITY_CODING_CONVENTION.md)을 함께 적용합니다.
