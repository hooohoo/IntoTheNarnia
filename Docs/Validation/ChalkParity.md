# Chalk 기준 통합 검증 · 2026-10-21

Unity 2022.3.62f3의 독립 검증 사본에서 실행했다. 원본 저장소에 반영하는 코드·에셋은 최종 전달 시 SHA-256으로 대조한다. [검증 입력 해시](ChalkParity/SourceManifest.json)를 함께 보관한다. Unity 2021의 이전 검사 결과를 재사용하지 않았다.

| 검사 | 결과 | 증거 |
| --- | --- | --- |
| 자체 EditMode | 17/17 통과, 실패·스킵 0 | [EditMode.xml](ChalkParity/EditMode.xml) |
| 실제 번들 PlayMode | 5/5 통과, 실패·스킵 0 | [PlayMode-Packed.xml](ChalkParity/PlayMode-Packed.xml) |
| macOS Development 플레이어 | 빌드 성공, 오류 0 | [빌드 요약](ChalkParity/PlayerBuild.txt), [메시지](ChalkParity/PlayerBuildMessages.txt) |
| 패키지된 플레이어 실행 | Title → Professor_House → Title, 플레이어 존재, 마지막 Title 라벨 소유 1개 | [실행 결과](ChalkParity/Result.txt) |
| Addressables | 엔트리 11개, 피처 라벨 4개, 실패 0 | [계약](ChalkParity/Addressables.txt) |
| 중복 의존성 | setting.png 공통 의존성 분리 후 중복 0 | [분석](ChalkParity/Dependencies.txt) |
| URP 변환 | 독립 머티리얼 344개, 변환 도구 미지원 0 | [변환](ChalkParity/Rendering.txt) |
| 콘텐츠 검사 | 렌더러 6,873개, 프리팹 영속 콜백 7개, 실패 0 | [콘텐츠](ChalkParity/Content.txt) |
| 씬 검사 | 씬 8개, 기존 씬 콜백 10개, 라이트맵 37개, 실패 0 | [씬 검사](ChalkParity/SceneValidation.txt) |
| 구조 보존 | 원본 스크립트 GUID 46개, 메시 820개, 참조 2,663개 보존 | [정적 검사](ChalkParity/StaticChecks.txt) |
| 설정 대조 | ProjectSettings 36개 항목 검토, 공통 lock 패키지 60개 버전 일치 | [비교 목록](../Architecture/ChalkSettingsComparison.json) |

EditMode는 DI·정리 순서, 상태 중첩/재진입, 초기화 실패와 롤백 예외, Provider 교체를 포함한다. 패키지 내부 DocTest는 자체 테스트 수에 포함하지 않는다. PlayMode는 실제 번들의 공유 소유권, 로드 도중 이탈·재진입, 누락 라벨 실패 후 정리·복구, 교수의 집 직접 진입과 Title 경유 UI 버튼·대화·설정·퀘스트·문·재진입을 검사했다.

번들 검증은 macOS 콘텐츠를 빌드한 뒤 Use Existing Build 모드에서 실행했다. 모드 선택은 커밋하지 않는 Library/AddressablesConfig.dat에 저장된다. 새 체크아웃의 기본값은 Use Asset Database이며 검증 사본도 이 모드로 복원했다. [모드 확인](ChalkParity/PlayModeConfiguration.txt)에 실행한 빌더 타입을 기록했다. 플레이어 빌드는 항상 현재 타깃의 로컬 카탈로그·번들을 동반한다. Addressables.Release는 소유 참조를 반환하며 즉시 전체 메모리가 회수된다는 뜻은 아니다.

실제 1280×720 플레이어 창에서 [Title](ChalkParity/Title.png), [대화](ChalkParity/ProfessorHouse.png), [탐험 HUD](ChalkParity/Exploration.png), [설정 팝업](ChalkParity/Settings.png)을 확인했다. UI 프리팹은 Scale With Screen Size, 기준 1920×1080, Match 0.5를 사용한다.

## 검증 범위

- macOS 에디터·플레이어를 실행했다. WebGL/Windows/iOS/Android 타깃 플레이어 빌드와 기기 성능 검증은 실행하지 않았다.
- 기존 스타일·콘텐츠는 유지했다. 설정 팝업은 기존 닫기 기능, Title의 Continue/Settings 및 두 방 씬은 기존 미구현 콘텐츠 상태다.
- 스크립트·참조·실행 검증은 모든 기기에서의 성능이나 시각적 동일성을 보증하지 않는다. Built-in/Gamma에서 URP/Linear로 변경되어 조명 외관이 달라질 수 있다.
