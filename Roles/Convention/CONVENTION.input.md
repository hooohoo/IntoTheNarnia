# Input 컨벤션

장치 폴링은 InputManager, 동작 정책은 InputHandlerBase 파생 클래스가 맡는다. 상태 Enter/Exit가 핸들러를 교체·정리한다. UI Raycast 결과와 pointerId로 입력 시작 소유권을 정하고 끝까지 유지한다. 조이스틱의 다중 포인터 간섭을 막고 비활성화·전환 시 값과 소유권을 초기화한다.

[공통 컨벤션](CONVENTION.common.md)을 함께 적용한다.
