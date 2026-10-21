# IntoTheNarnia

나니아 세계를 배경으로 한 Unity 3D 탐험 프로젝트입니다. 현재 플레이 흐름은 Title → LoadingScene → Professor_House이며, Lucy의 이동, 카메라, 대화, 문 상호작용과 방 이동을 구현합니다.

## 실행 환경

| 항목 | 실제 설정 |
| --- | --- |
| Unity | 2021.3.6f1 |
| 렌더링 | Built-in Render Pipeline |
| 입력 | 기존 Input Manager + uGUI 조이스틱 |
| 카메라 | Cinemachine 2.8.9 |
| 텍스트 | TextMeshPro 3.0.6 / 기존 uGUI Text |
| Navigation | AI Navigation 1.0.0-exp.4 |
| 에디터 도구 | MCP for Unity v10.1.2 |
| 기본 브랜치 | main |

Unity Hub에서 프로젝트를 열고 패키지 임포트 후 `Assets/Scenes/Title.unity`를 실행합니다. 구조 전환 후의 경로와 실행 방법은 같은 문서에서 갱신합니다. 현재 Professor_House 단독 실행은 Title의 GameManager에 의존합니다.

## 씬

빌드에는 Title, Professor_House, LoadingScene, TheSiblingsRoom, TheWardrobeRoom 순서로 등록되어 있습니다. 두 방 씬은 플레이 로직이 없는 자리표시자입니다. Main, Test, WinterForest는 빌드에 포함되지 않은 실험 씬입니다. 방의 새 콘텐츠 제작은 구조 개편과 별개입니다.

## 문서

- [작업 지침](CLAUDE.md), [아키텍처](ARCHITECTURE.md), [코딩 컨벤션](UNITY_CODING_CONVENTION.md)
- [현재 상태와 미커밋 변경 검토](Docs/Status/2026-10-21-baseline.md)
- [구조 전환 계획과 완료 조건](Docs/Architecture/Migration.md)
- [교수의 집 변경 이력](SceneBackups/Professor_House_20260910_124917/README.md), [천장 변경 이력](SceneBackups/Professor_House_Ceilings_20260910_153250/README.md)

문서는 현재 구현과 전환 목표를 구분합니다. 예전 Chalk 템플릿의 Flutter, HexGrid, URP, Addressables 및 dev 브랜치 설명은 이 프로젝트의 구현 현황이 아닙니다.
