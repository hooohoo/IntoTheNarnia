# UI

복수 상태·데이터·비동기 작업을 다루는 UI는 Model/Presenter/View로 나눕니다. 단순 버튼, 조이스틱, 단일 표시 컴포넌트는 Controller를 유지할 수 있습니다. View는 Unity 컴포넌트 참조와 표시만 담당합니다. Presenter는 입력 바인딩과 Model 변경, 이벤트 연결을 소유합니다. 모델은 Unity 계층 검색을 하지 않습니다.

Initialize/Bind와 Release/Unbind는 대칭입니다. 닫힘·재활성화·씬 이동 시 중복 구독과 이전 타이핑 작업이 남지 않아야 합니다. 설정 팝업이 열린 동안 이동·카메라 입력을 차단하고 닫힐 때 이전 상태로 복구합니다. 기존 Inspector UnityEvent를 유지할 경우 코드에서 같은 콜백을 중복 등록하지 않습니다.

State는 HUD 오브젝트에 접근하지 않습니다. Screen/UIManager가 상태·퀘스트 이벤트를 구독하여 화면을 갱신합니다. 런타임 생성 UI를 도입할 때만 캐시/풀의 OnSpawn/OnDespawn 소유권을 추가합니다. 현재 씬 배치 UI에 필요 없는 풀을 만들지 않습니다.
