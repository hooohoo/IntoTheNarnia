# Event

EventManager는 도메인 이벤트를 전달합니다. 문 변화, 퀘스트 진행, 상태 전환을 UI 및 지도 로직에 알립니다. 구독 변경과 재진입을 고려한 스냅샷으로 전달하며 Release에서 해제합니다. 전역 C# 이벤트/정적 델리게이트에 씬 객체가 남지 않아야 합니다. Cinemachine 입력 델리게이트도 연결한 소유자가 복원합니다.

[전체 구조](../../ARCHITECTURE.md)와 해당 [컨벤션](../Convention/UNITY_CODING_CONVENTION.md)을 함께 적용합니다.
