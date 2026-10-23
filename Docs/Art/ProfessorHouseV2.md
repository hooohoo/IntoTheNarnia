# 교수의 집 v2 구현

> 이 문서는 현재 Unity v2 홀과 기존 생성 도구의 구현 기록이다. 이후 공간 제작은 2026-10-22 승인된 [확정 도면 v1](ProfessorHouse_ApprovedLayout_v1/README.md)을 따른다. 아래 +3.65m 회랑·18단 계단과 기존 두 문의 배치는 새 도면으로 아직 교체하지 않았다. 기존 재생성 명령은 확정된 전체 저택 배치를 만들지 않는다.

`Professor_House.unity`의 메인 홀, 계단, L자형 2층 회랑을 승인된 [v2 이미지](References/Lucy_ProfessorHouse_Reference_v2.png)에 맞춰 재구성했다. 영화 교수의 집을 게임 크기에 맞게 해석한 공간이며 정확한 영화 평면도의 복원은 아니다.

## 적용 범위

- 두꺼운 월넛 구조재·벽 패널, 크림 벽, 모서리를 다듬은 밝은 석재 바닥.
- 붉은 러그와 계단 러너, 구형 난간 머리와 단순한 회전형 난간 기둥.
- 납선 창문, 초상화, 흉상, 올리브색 의자, 책과 꽃병, 벽 촛대.
- 1층 스폰 → 계단 → 2층 형제방·옷장방 입구까지 연결된 이동 경로.
- 기존 문 퀘스트·대화·UI·씬 전환 계약 유지. 문짝은 독립 메시와 `State` 정수 파라미터로 개폐한다.

Lucy는 기존 플레이어 프리팹을 사용한다. 갈색 단발의 새 캐릭터 모델·리깅, 별도 `TheWardrobeRoom` 내부와 옷장의 새틴 광택은 아직 적용하지 않았다. 원래 홀 밖의 방들을 새 디자인으로 모두 완성했다는 의미도 아니다. 현재 플레이 가능한 재구성 범위는 위 동선이다.

## 에셋 소유권

| 위치 | 역할 |
| --- | --- |
| `ArtSource/ProfessorHouse/ProfessorHouse_v2.blend` | 편집 가능한 Blender 원본. Unity 자동 임포트 대상 밖에 보관 |
| `Tools/Art/build_professor_house.py` | 원본과 FBX를 재생성하는 결정적 Blender 스크립트 |
| `Assets/@Project/10.Models/ProfessorHouse` | 구조 FBX, 독립 문 FBX, 크기·팔레트·통계 manifest |
| `Assets/@Project/04.Materials/ProfessorHouse` | 공유 URP Lit 머티리얼과 흰색 기본 텍스처 |
| `Assets/@Project/02.Prefabs/Environment_Prefab/ProfessorHouse` | 환경·충돌·NavMeshSurface 프리팹, 두 퀘스트 문 |
| `Assets/@Project/05.Animations/ProfessorHouse` | 문 개폐 애니메이션과 컨트롤러 |
| `Assets/@Project/00.Scenes/Professor_House/V2` | NavMesh, 계단 충돌 메시, 조명 설정·볼륨 |
| `Assets/@Project/00.Scenes/Professor_House` | 베이크된 라이트맵 2장과 방향·섀도 마스크, LightingData, 반사 큐브맵 |
| `Assets/@Project/Editor/ProfessorHouseBuilder.cs` | 임포트, 배치, 연결, 베이크, 촬영 도구 |

환경은 29개 공간·기능별 메시로 묶었다. 새 프리팹 파일에는 `OBJ_` 접두어를 사용하고 퀘스트가 바인딩하는 두 문 루트 이름은 유지한다. 문은 별도로 움직인다. 하나의 전체 건물 메시로 합치지 않으며 새 홀은 64개 환경 Renderer 이내를 제작 예산으로 검사한다. 재질이 여러 개인 메시는 여러 submesh/draw로 렌더링되므로 메시 수를 draw call 수로 해석하지 않는다.

환경 Renderer 28개는 라이트맵을, 작은 촛대 메시 1개는 Light Probe를 사용한다. 이동 캐릭터와 문은 Light Probe를 사용한다.

고정 환경과 문은 씬의 직렬화 의존성으로 로드·해제한다. 동적으로 생성하는 Lucy·UI·대사 데이터는 기존 ProfessorHouse/Story Addressables 소유권을 유지한다. 고정 씬 에셋을 같은 라벨로 중복 프리로드하거나 Resources에 넣지 않는다. 옛 환경 에셋은 이번 씬에서 참조하지 않지만 다른 씬·이력 보존을 위해 일괄 삭제하지 않았다.

## 크기와 이동

단위는 미터이며 Unity Y가 위다. 홀은 약 12×14m, 회랑 바닥은 y=3.65m, 계단은 폭 2.6m의 18단이다. 두 문의 열림 폭은 약 1.8m이고 문짝 높이는 2.54m다. 현행 약 2m 플레이어와 기존 Agent 타입에 맞춘 첫 구현이다. 새 Lucy의 최종 신체 크기가 확정되면 캐릭터·카메라·동선의 비율을 함께 다시 검토한다.

계단 시각 메시와 충돌 경사로를 분리한다. NavMeshSurface는 환경 아래의 단순 충돌체를 사용해 베이크하며 문짝을 고정 장애물로 베이크하지 않는다. 통로의 `NavMeshModifierVolume`은 기존 `TheSiblingsRoomDoor` / `TheWardrobeRoomDoor` Area를 유지하고, 퀘스트 이벤트가 플레이어 areaMask를 열어 준다. 새 베이크 데이터가 연결된 환경 프리팹을 씬이 소유한다.

## 다시 생성하기

작업 브랜치에서 실행한다. 아래 명령은 생성된 아트 파일을 다시 쓰므로 수동 아트 수정은 먼저 원본 스크립트에 반영하거나 별도 작업본으로 보존한다.

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup --threads 4 --python Tools/Art/build_professor_house.py -- --project .
```

Unity 2022.3.62f3에서 `Narnia/Professor House/Rebuild v2 Hall`을 실행한 다음 조명을 베이크한다. CLI에서는 그래픽이 있는 Unity 프로세스에 `-executeMethod ProfessorHouseBuilder.BuildAndBakeBatch`를 사용한다. 이 메서드는 완료 후 종료하므로 `-quit`을 함께 주지 않는다. Apple Silicon은 Progressive GPU를 사용한다.

재구성은 `Environment_ProfessorHouse_v2`, `Lighting`, `HouseInteractions`, `HousePreviewCameras`를 다시 생성한다. 이 루트에 수동으로 추가한 변경은 재구성 도구에 옮겨야 유지된다. 런타임 소유권·씬 GUID·필드 루트·포털 참조는 유지한다.

`Narnia/Professor House/Frame Hall in Scene View`로 에디터 시점을 홀 내부의 정상 방향으로 맞출 수 있다. 이 시점 조정은 게임 카메라나 모델 좌표를 바꾸지 않는다.

`Narnia/Professor House/Capture v2 Previews`는 실제 URP 카메라에서 이미지를 저장한다. CLI 촬영은 그래픽이 있는 Unity에서 `ProfessorHouseBuilder.PreviewBatch`를 사용한다. 이 macOS 환경의 배치 모드에서는 Metal 텍스처 바인딩이 누락되어 빈 렌더가 발생했으므로 최종 확인은 GUI 에디터와 패키징된 플레이어에서 수행한다.

## 검증

`Tools/validate_structure.py`, Unity의 `ProfessorHouseValidation.Run`, EditMode/PlayMode 테스트와 패키징된 플레이어 결과는 [교수의 집 v2 검증](../Validation/ProfessorHouseV2.md)에 기록한다. PlayMode의 `ProfessorHouseTraversalTests`는 실제 입력 경로로 계단을 걸어 올라가 두 문 앞까지 이동하며, 열린 문짝의 회전과 잠금 전후의 통과 가능 여부를 확인한다.
