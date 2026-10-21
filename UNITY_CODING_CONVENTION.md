# IntoTheNarnia 코딩 컨벤션

규칙의 단일 원본은 Roles/Convention의 분야별 문서입니다. CLAUDE.md는 요약이며 ARCHITECTURE.md는 구조와 소유권을 정의합니다. 전환 이력과 호환성 경계는 Docs/Architecture/Migration.md에 기록합니다.

- [공통](Roles/Convention/CONVENTION.common.md)
- [매니저](Roles/Convention/CONVENTION.manager.md)
- [UI](Roles/Convention/CONVENTION.ui.md)
- [데이터](Roles/Convention/CONVENTION.data.md)
- [이벤트](Roles/Convention/CONVENTION.event.md)
- [입력](Roles/Convention/CONVENTION.input.md)

에셋은 .meta와 함께 이동합니다. 새 에셋은 UI_, OBJ_, CAM_, MAT_, TEX_, FBX_ 등 역할이 분명한 이름을 사용하되 기존 콘텐츠 이름과 외부 패키지 이름은 보존합니다. Addressable 주소는 실제 파일명과 일치해야 합니다. 애니메이션은 대상@동작, Animator Controller는 대상 이름을 사용합니다.
