# State

GameStateManager는 Title, Loading, Exploration, Dialogue, Settings 상태의 전환과 Enter/Exit를 소유합니다. 상태는 입력 전략을 한 번 만들고 재사용합니다. Screen은 상태 이벤트를 구독하여 표시를 갱신합니다. State와 Screen은 1:1로 강제하지 않습니다. Settings/Dialogue 종료 시 이전 상태를 복구합니다. 씬 전환 중 이전 씬의 콜백이 새 상태를 덮어쓰지 않도록 씬 소유권을 정리합니다.

[전체 구조](../../ARCHITECTURE.md)와 해당 [컨벤션](../Convention/UNITY_CODING_CONVENTION.md)을 함께 적용합니다.
