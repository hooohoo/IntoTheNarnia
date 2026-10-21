# Manager 컨벤션

Singleton<T>/IManager와 Init/Release 계약, 명시적인 Facade 등록, [Singleton] 의존성 주입과 위상 초기화를 사용한다. 미등록·중복·순환 의존성은 실패한다. 초기화 역순으로 Release하고 부분 초기화 자원도 정리한다.

매니저의 전역 수명과 씬 객체·라벨의 수명을 분리한다. SceneRoot가 UI/Object/Camera/Data와 리소스 소유를 끝낸다. 매니저의 책임이 도메인별로 커질 때 partial로 나누고 실제 Init/Release 호출을 연결한다. 빈 부분 클래스나 사용하지 않는 플랫폼 서비스를 모양만 맞추려고 추가하지 않는다.

AddressableManager가 리소스 핸들, UIManager가 UI 인스턴스, ObjectManager가 비UI 인스턴스를 소유한다. Factory는 게임 도메인의 생성 인자를 조정한다.
