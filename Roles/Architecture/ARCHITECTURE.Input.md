# Input

장치 입력은 InputManager에 모읍니다. 상태별 입력 전략이 이동과 시점 회전을 허용하거나 차단합니다. 조이스틱은 입력 값만 전달하고 플레이어는 정규화된 입력을 소비합니다. UniTask 프레임 루프가 입력을 먼저 갱신한 뒤 객체를 갱신합니다. 카메라 후처리는 별도 타이밍을 사용합니다.

[전체 구조](../../ARCHITECTURE.md)와 해당 [컨벤션](../Convention/UNITY_CODING_CONVENTION.md)을 함께 적용합니다.
