# 교수의 집 v2 검증 — 2026-10-21

승인된 [v2 레퍼런스](../Art/References/Lucy_ProfessorHouse_Reference_v2.png)를 기준으로 `Professor_House`의 메인 홀·계단·2층 회랑과 두 퀘스트 문 입구를 교체했다. [제작 범위·에셋 소유권·재생성 방법](../Art/ProfessorHouseV2.md)을 함께 참고한다. Lucy의 새 모델과 별도 옷장방 내부는 후속 범위다.

Unity 2022.3.62f3 / URP 14.0.12 / macOS에서 검사했다. 원본 저장소를 직접 빌드하지 않고 동일 코드·패키지·설정의 격리된 프로젝트에서 작업과 검증을 진행한 뒤, 검증한 파일과 `.meta`를 작업 브랜치에 반영했다. 커밋·병합·push는 수행하지 않았다.

## 결과

| 검사 | 결과와 근거 |
| --- | --- |
| 구조·Chalk 계약 | [정적 검사](ProfessorHouseV2/StaticChecks.txt) 모두 실패 0. 기존 스크립트 46개 GUID 유지 |
| 환경·조명·참조 | [씬 검사](ProfessorHouseV2/SceneValidation.txt) 실패 0. 메시·머티리얼·라이트맵 UV와 두 문 Animator, NavMesh 검사 |
| 이동 경로 | [7개 경로 검사](ProfessorHouseV2/Navigation.txt) 통과. 1층→회랑, 두 문 접근 및 잠금 전후 경로 |
| EditMode | [18개 통과 / 실패 0](ProfessorHouseV2/EditMode.xml) |
| PlayMode | [6개 통과 / 실패 0](ProfessorHouseV2/PlayMode.xml). 기존 리소스·UI·퀘스트·재진입과 새 계단·문 이동 검사 |
| 빌드 콘텐츠 | [콘텐츠 참조](ProfessorHouseV2/Content.txt), [Addressables 주소·라벨](ProfessorHouseV2/Addressables.txt) 실패 0 |
| macOS 개발 빌드 | [성공, 오류 0](ProfessorHouseV2/PlayerBuild.txt). Addressables 콘텐츠와 플레이어를 함께 빌드 |
| 실제 플레이어 | [Title→Professor_House→Title 통과](ProfessorHouseV2/Player/Result.txt). 플레이어 생성, 대화 종료, 탐험·설정 UI, 씬 자원 소유권 해제 확인 |

`ProfessorHouseTraversalTests`는 실제 조이스틱 입력을 `InputManager`에 전달해 플레이어가 계단을 올라가도록 한다. 두 문 앞에서 실제 트리거 접촉을 확인하고, 문짝이 회전해 열리는지와 NavMesh Area 잠금 해제 후 경로가 완성되는지 검사한다. 테스트에서 이동을 순간이동으로 대체하지 않는다.

빌드에 기록된 [경고 4개](ProfessorHouseV2/PlayerBuildMessages.txt)는 `-nographics` 빌드 프로세스에서 SkyManager가 주변광·반사 프로브를 갱신할 수 없다는 내용이다. 실제 조명은 그래픽이 있는 에디터에서 별도로 베이크했으며 최종 베이크의 UV 중첩 경고는 0개였다. GUI 에디터와 패키징된 플레이어에서 머티리얼·라이트맵·그림자를 직접 확인했다. iOS·Android·WebGL 및 IL2CPP 빌드는 이번 검증 범위에 포함하지 않는다.

## 화면 방향

[모델·카메라 좌표 검사](ProfessorHouseV2/Orientation.txt)에서 바닥 윗면은 약 y=0.025m, 회랑은 y=3.65m, 천장은 회랑 위에 있으며 환경의 모든 월드 스케일은 양수였다. 메인 카메라와 세 촬영 카메라의 위쪽 방향도 정상이었다. 기존 Scene View는 원점을 향하는 시점으로 저장돼 있어 `Frame Hall in Scene View` 메뉴로 홀 내부를 바라보도록 맞췄다. 기존 시점이 실제 상하 반전의 원인이었다고 단정하지 않는다.

아래 화면은 생성형 레퍼런스가 아니라 **실제 macOS 실행 파일의 캡처**다. 모델이나 게임 카메라가 위아래로 뒤집히지 않았음을 확인했다.

![실제 플레이 화면](ProfessorHouseV2/Player/Exploration.png)

![패키징된 홀 전경](ProfessorHouseV2/Player/Hall_Hall.png)

추가 화면: [계단](ProfessorHouseV2/Player/Hall_Stair.png), [회랑](ProfessorHouseV2/Player/Hall_Gallery.png), [설정 UI](ProfessorHouseV2/Player/Settings.png). UI를 숨긴 전경은 동일 패키지 씬의 저장된 촬영 카메라로 캡처했다. 에디터 촬영 원본은 [홀](ProfessorHouseV2/Hall.png)·[계단](ProfessorHouseV2/Stair.png)·[회랑](ProfessorHouseV2/Gallery.png)에 있다.

## 에셋과 예산

[측정 결과](ProfessorHouseV2/Geometry.txt): 환경 MeshRenderer 29개, 모든 submesh를 합한 삼각형 124,344개, 방향성 라이트맵 2세트. 움직이는 문과 Lucy는 환경 통계에서 제외된다. 29개는 draw call 수가 아니며, 현재 범위가 기존 전체 집보다 작으므로 이전 전체 환경과의 성능 개선 비율을 주장하지 않는다.

씬 GUID, 포털·필드 루트와 기존 문 이름·NavMesh Area·`State` 파라미터를 유지했다. 새 프리팹 파일은 `OBJ_`, 모델은 `FBX_`, 머티리얼은 `MAT_`, 애니메이션은 `대상@동작` 규칙을 따른다. 씬의 [실제 의존성 목록](ProfessorHouseV2/SceneDependencies.txt)에 포함된 라이트맵·반사 큐브맵·NavMesh와 `.meta`를 함께 반영했다. 옛 환경 에셋은 이번 씬에서 참조하지 않으며 다른 콘텐츠와 복원 이력을 위해 남겨 두었다.

## 다시 실행하기

- Unity에서 `Assets/@Project/00.Scenes/Title.unity`를 열고 Play하거나 `Professor_House.unity`를 직접 실행한다.
- 에디터 시점: `Narnia/Professor House/Frame Hall in Scene View`.
- 로컬 검토용 실행 파일: `Builds/macOS/ProfessorHouse_v2.app` (Git 제외).
- `NARNIA_PREVIEW_HOUSE=1`로 개발 플레이어를 실행하면 교수의 집을 열고 첫 대화를 넘긴 뒤 직접 조작할 수 있다.
- `NARNIA_SMOKE_OUTPUT=<결과 폴더>`로 실행하면 검증·캡처 후 자동 종료한다. `NARNIA_HOUSE_CAPTURE=1`을 함께 설정하면 세 전경도 촬영한다. 이 진입점은 개발 빌드 전용이다.

2026-10-23에 현재 프로젝트에서 다시 검사한 결과와 기준점 통합 범위는 [제작 기준점 검증](ProfessorHouseBaseline.md)에 기록한다. 위 표는 2026-10-21 당시 격리 프로젝트의 결과다. 현재 프로젝트에서는 EditMode 17개·PlayMode 6개를 실행했다.
