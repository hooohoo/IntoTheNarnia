# IntoTheNarnia

나니아 세계를 배경으로 한 Unity 3D 탐험 프로젝트입니다. Title → LoadingScene → Professor_House 흐름에서 Lucy 이동, 카메라, 대화, 설정, 문 상호작용과 방 이동을 구현합니다.

## 실행 환경

| 항목 | 설정 |
| --- | --- |
| Unity | 2021.3.6f1 |
| 렌더링 | Built-in Render Pipeline |
| 입력 | 기존 Input Manager + uGUI 조이스틱 |
| 카메라 | Cinemachine 2.8.9 |
| 텍스트 | TextMeshPro 3.0.6 / 기존 uGUI Text |
| 비동기 | UniTask 2.5.10, Git 커밋 7c0f199fe0d3fc528024488ccd671e6c7b27745b |
| JSON | Newtonsoft.Json 3.0.2 |
| Navigation | AI Navigation 1.0.0-exp.4 |
| 에디터 도구 | MCP for Unity v10.1.2 |
| 통합 브랜치 | main |

Unity Hub에서 프로젝트를 열고 패키지 임포트 후 `Assets/@Project/00.Scenes/Title.unity`를 실행합니다. Professor_House를 직접 실행해도 FacadeManager가 자동으로 부팅합니다. 조이스틱 또는 방향키/WASD로 이동하고, UI 밖에서 마우스 왼쪽 버튼을 누른 채 시점을 조절합니다. 대화·설정·로딩 중에는 이동과 시점 조작을 차단합니다.

빌드에는 Title, Professor_House, LoadingScene, TheSiblingsRoom, TheWardrobeRoom이 등록되어 있습니다. 두 방은 현재 플레이 콘텐츠가 없는 자리표시자입니다. Main, Test, WinterForest는 빌드에 포함되지 않은 실험 씬이며 기존 참조를 유지합니다. Title의 Continue/Setting 버튼은 기존 미구현 진입점을 유지합니다.

## 구조

자체 콘텐츠는 `Assets/@Project`에 모읍니다. `01.Scripts`의 Framework/Manager/Scene/State/UI/Game/Data를 런타임 계층으로 사용하며 Samples, Editor, Tests를 구분합니다. 외부 에셋은 Assets/Asset, TextMesh Pro, Plugin 등의 기존 위치를 유지합니다. `02.Resources/Resources` 안의 상대 로드 키는 이전과 같습니다. Resources를 실제 콘텐츠 제공 방식으로 사용하며 Addressables/Flutter/HexGrid/URP는 이 프로젝트에 도입하지 않았습니다.

## 검증

```sh
python3 Tools/validate_structure.py
```

Unity Test Runner에서 IntoTheNarnia.EditModeTests 및 IntoTheNarnia.PlayModeTests를 실행합니다. 배치 실행은 아래 형식을 사용합니다. `-runTests`에는 `-quit`을 함께 사용하지 않습니다.

```sh
<UNITY_EDITOR> -batchmode -nographics -projectPath <PROJECT_PATH> -runTests -testPlatform EditMode -testResults <RESULT.xml> -logFile <LOG.txt>
<UNITY_EDITOR> -batchmode -nographics -projectPath <PROJECT_PATH> -runTests -testPlatform PlayMode -testResults <RESULT.xml> -logFile <LOG.txt>
<UNITY_EDITOR> -batchmode -nographics -quit -projectPath <PROJECT_PATH> -executeMethod ProjectValidation.Run -logFile <LOG.txt>
```

2026-10-21 검증에서는 Unity 2021 에디터의 라이선스 갱신이 실패하여, 2021 API 참조 컴파일 및 해당 Mono 런타임의 도메인 테스트와 임시 복제본의 Unity 2022.3.62f3 에디터 테스트를 병행했습니다. 원본 프로젝트 버전은 변경하지 않았습니다. 자세한 범위와 제한은 [검증 기록](Docs/Validation/2026-10-21.md)을 확인합니다.

## 문서

- [작업 지침](CLAUDE.md), [아키텍처](ARCHITECTURE.md), [컨벤션](UNITY_CODING_CONVENTION.md)
- [구조 전환 기록](Docs/Architecture/Migration.md), [스크립트 GUID 이동표](Docs/Architecture/ScriptMigration.json)
- [전환 전 상태](Docs/Status/2026-10-21-baseline.md)
- [교수의 집 변경 이력](SceneBackups/Professor_House_20260910_124917/README.md), [천장 변경 이력](SceneBackups/Professor_House_Ceilings_20260910_153250/README.md)
- [UniTask 원본과 설치 설명](https://github.com/Cysharp/UniTask#upm-package)
