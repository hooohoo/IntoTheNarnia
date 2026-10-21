# UI 컨벤션

UI_Base/Scene/Popup/Sub 3계층을 사용한다. 씬 진입 프리로드 후 UIManager.ShowSceneUI/ShowPopupUI/ShowSubUI가 캐시를 동기 조회해 생성한다. Scene 캐시·Popup 순서·Sub 부모 소유와 OnSpawn/OnDespawn을 관리한다. 대화는 Model/Presenter/View, 씬 표시는 Screen이 조정한다. 이탈 시 자식 UI·인스턴스가 에셋보다 먼저 정리된다.

[공통 컨벤션](CONVENTION.common.md)을 함께 적용한다.
