> 2026-10-23: 사용자 확인을 마친 연결 모형을 프로젝트 제작 원본으로 등록했습니다. 아래 검사는 다운로드 검토 당시의 Blender·도면 검사이며, 새 Unity 검증은 [연결 구조 구현 기록](../../../Docs/Art/ProfessorHouseConnected.md)에 별도로 기록합니다.

# 교수의 집 · 전체 연결 모형 01

2026-10-23 · 확정 도면 03에서 파생한 다운로드 검토본 · Unity 반영 전

[index.html](index.html)에서 이미지와 설명을, [Viewer.html](Viewer.html)에서 층별 분리·전체 조립·주요 구역·아이 눈높이 경로를 확인한다. 로컬 파일로 열 수 있으며 외부 스크립트나 인터넷 연결이 필요하지 않다. 자료 모음 전체를 함께 보관하면 확정 도면·기존 레퍼런스로 가는 링크도 유지된다.

## 기준과 이번 범위

프로젝트 `Docs/Art/ProfessorHouse_ApprovedLayout_v1/Data/layout.json`의 50개 공간, 54개 출입구, 방 배치와 서사 동선을 유지한다. `Data/layout.json`과 `Plans/*.svg`는 해당 확정본과 바이트가 같다. 기존 후정 검토 모형의 높은 홀·비대칭 계단과 검토 04의 옷장 부품을 이 배치에 연결했다.

- 2층 바닥 +4.32 m, 다락 +9.00 m, 홀 천장 +8.84 m.
- 공통 계단 10단 → 왼쪽 14단 / 오른쪽 8단+6단. 공통 계단참 3.2 × 2.5 m.
- S2는 확정된 6 × 8 m 안에서 1층→2층 12+12단, 2층→다락 13+13단. 단높이 18 cm. 2층 ER 문에 닿는 서쪽 도착부를 포함한다.
- 교수 구역은 2층 서쪽의 단일 경계문으로 연결한다. 아이들 침실은 동쪽 끝에 두고 S2와 침실 복도 K0 사이에는 문을 추가하지 않았다.
- 후정과 동측 중정을 상층 바닥으로 덮지 않는다. 아치창 뒤는 야외다.
- 사고의 공 궤적과 아이들 진입 경로는 별개다. 갑옷은 사건 위치 표시용이며 파손 애니메이션은 아직 없다.
- 비교 인형의 높이 1.20 m, 반경 0.22 m, 눈높이 1.05 m는 이 검토의 가정이다. 루시 최종 신체 비율을 확정한 것이 아니다.

이번에 구체화한 보조 계단 내부 배치, 일반 벽 두께 0.24 m, 가구·문틀 위치는 모형 검토 범위다. 지붕 외형, 일반 실내의 최종 천장·가구·조명, 문 상호작용과 파손 부속은 후속 제작한다. 보드의 재질과 둥근 형태를 적용할 때도 확정 배치와 이동 관계를 유지한다.

## 파일

| 경로 | 내용 |
| --- | --- |
| `Previews/01_Whole_Exploded.png` | 전체 층별 분리 보기 |
| `Previews/02_Ground.png` ~ `05_HideRoute.png` | 층별 단면과 세 장면의 경로 |
| `Previews/06_Hall_Eye.png` | 홀의 1.05 m 눈높이 |
| `Previews/07_Children_Wing.png` ~ `11_RearGarden.png` | 침실·옷장방·사고 지점·보조 계단·후정 |
| `Models/ProfessorHouse_ConnectedLayout_01.blend` | 미터 단위, Z 위쪽, 실제 층 높이의 공통 원본 |
| `Models/ProfessorHouse_ConnectedLayout_01.glb` | Y 위쪽으로 내보낸 검토용 교환 파일 |
| `Data/model-spec.json` | 도면에서 파생한 부품·계단·동선 정의 |
| `Data/viewer-model.js` | 오프라인 WebGL 보기용 파생 메시 |
| `Source/` | 생성·검사·렌더 재현 스크립트 |

GLB는 형상 확인용이다. 런타임 최적화, Unity 재질 매핑, 콜라이더, NavMesh, 씬 경계와 Addressables 등록은 포함하지 않는다. 조립 검증용 공통 원본에서 구역별 런타임 자산을 파생하는 것이 다음 제작 방식이다.

## 확인한 내용

- `Data/validation.json`: 확정 도면의 방·문 그래프, 12개 경로, 3,137개 표본, 겹침과 정원 침범 없음.
- `Data/mesh-route-checks.json`: 실제 Blender 메시에서 세 장면 주 경로와 홀 오른쪽 계단을 검사했다. 2,580개 표본에서 바닥·머리 위 공간·몸 주변 간섭 실패 0. 표본의 최소 머리 위 공간은 2.70 m, 계획 발 위치와 계단 디딤면 차이 최대 0.164 m다.
- `Data/mesh-door-checks.json`: 열린 문 53곳의 중심과 양쪽 접근부 검사 실패 0. 교수 구역 경계문 1곳은 의도적으로 닫아 검사 대상에서 구분했다.
- `Data/browser-checks.json`: 오프라인 3D, 층·장면·구역 선택, 50개 방 선택, 경로 재생, 이미지 로드, 데스크톱·모바일 배치 확인.

메시 검사는 정해진 크기 인형의 경로 표본 검사다. 전체 바닥의 캡슐 스윕, Unity 물리·NavMesh·실제 캐릭터 이동 또는 카메라 연출 검증을 대체하지 않는다. 다음 단계에서 플레이 가능한 씬으로 확인한다.

## 재현

Blender 5.1.2에서 생성했다. 원본을 보존한 새 작업 복사본에서 실행한다. 생성 스크립트는 해당 폴더의 파생 파일을 덮어쓴다.

```sh
python3 -B Source/validate_layout.py
python3 -B Source/build_spec.py
blender --background --factory-startup --threads 6 --python Source/build_blender.py
blender --background --factory-startup --threads 4 --python Source/validate_mesh.py
```

모형을 다시 만들지 않고 저장된 `.blend`의 이미지만 출력하려면 `Source/render_views.py`를 실행한다. `-- --only=06_Hall_Eye`처럼 이미지 이름을 지정할 수 있다. `Source/fix_glb_materials.py`는 Blender의 투명 전용 셰이더를 GLB의 표준 반투명 재질로 기록하며 생성 도구에서 자동 호출한다.

한글 메시 라벨은 macOS의 Apple SD Gothic Neo로 생성 후 메시로 변환해 보관한다. 재생성 환경에 해당 글꼴이 없다면 호환되는 한글 글꼴 경로를 지정한다. HTML과 3D 데이터는 같은 폴더에서 읽는다. 브라우저 검증은 `node Source/check_browser.cjs`이며 macOS의 Chrome을 사용한다.

이 다운로드 검토본이 승인된 다음 프로젝트에 등록한다. 현재 프로젝트에는 앞선 v2 구현과 확정 도면을 커밋·병합한 기준점이 있으며, 이 새 모형을 구현 완료로 기록하지 않았다.
