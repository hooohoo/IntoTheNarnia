# 교수의 집 · 홀 교차 수정과 2층 마감 01

2026-10-23. 확정 도면의 크기·문·층고와 서사 동선을 유지하며 `Professor_House_Connected`에 적용했다. [Unity 실제 화면](../Validation/ProfessorHouseUpperFinish/index.html) · [검증 결과](../Validation/ProfessorHouseUpperFinish.md).

후속 [전시실·연결 복도 01](ProfessorHouseGalleryFinish.md)에서 서재 층고를 측정하고 책장을 높여 상부 비례를 보완했다. 아래 첫 구현 화면·검사 수치는 해당 시점의 기록이며, 현재 2층 레이어는 16개 메시·277,860삼각형·충돌 39개다. [최신 서재 화면·높이 측정](../Validation/ProfessorHouseGalleryFinish/index.html)을 함께 본다.

## 이번 구현

| 구역 | 구현 |
| --- | --- |
| 중앙 홀 | 계단 아래 풍경화 축소·이동, 남성 초상화 상향 이동, 액자 뒷판과 벽 목재·몰딩의 간격 조정 |
| P-STUDY 서재 | 책장·책·양쪽 서랍 책상·가죽 패드·펼친 책·잉크병·스탠드·독서 의자·지구본·커튼·러그 |
| P-LIB 개인 서고 | 벽 책장, 자료 정리용 가구와 의자 |
| P-BED 교수 침실 | 아치형 침대, 협탁·스탠드·서랍장·의자·커튼·러그 |
| E1 여자아이 침실 | 두 침대, 분홍 침구, 협탁·스탠드, 옷장·서랍장·여행 가방, 커튼·그림·러그 |
| K-BOYS 남자아이 침실 | 같은 가구 규격과 파란 침구·커튼, 초상화와 풍경화, 모서리 보조창 셔터 |
| 다섯 방 공통 | 문·창 개구부를 따른 월넛 하부 벽, 밝은 벽·천장, 목재 보와 마룻바닥, 방별 실시간 보조광 |

[서재](ProfessorHouse_FinishedConcepts_v1/Images/03_Professor_Study_Finished.png)와 [아이들 침실](ProfessorHouse_FinishedConcepts_v1/Images/04_Children_Bedroom_Finished.png) 시안을 재질·배치 참고로 사용한다. 시안의 사진 같은 세부 묘사와 생성된 3D 모델의 완성도를 동일하다고 취급하지 않는다. 침대 머리판은 아치 메시 하나로 만들고, 반복 부품은 둥근 모서리를 공유한다.

남자아이 방과 욕실의 경계에 걸치는 북쪽 보조창은 기존 개구부를 이동하지 않고 방 안쪽의 좁은 창틀과 닫힌 셔터로 마감한다. 교수 전용 복도의 경계문은 계속 닫혀 있으며 아이들 안내 경로가 교수 공간을 통과하지 않는다. 이 씬의 가구와 셔터는 고정 장식이다.

## 생성·소유권

- 제작 원본: `ArtSource/ProfessorHouse/UpperFinish_v1/ProfessorHouse_UpperFinish_v1.blend`.
- 생성 도구: `Tools/Art/build_professor_house_upper_finish.py`, 공통 부품·검사·내보내기는 `professor_house_finish_kit.py`.
- Unity FBX와 배치 데이터: `Assets/@Project/10.Models/ProfessorHouseUpperFinish`.
- 프리팹: `OBJ_ProfessorHouse_UpperFinish`. 방/용도별 14개 시각 메시, 253,668삼각형, 39개 단순 BoxCollider. FBX Read/Write는 끈다.
- 기존 그림 아틀라스와 Hall 계열 재질을 재사용한다. 새 그림 텍스처·Addressables 그룹·Resources 로드는 추가하지 않는다. 고정 모델과 그림은 씬/프리팹의 직렬화 참조가 소유한다.
- 기존 구조 원본은 보존한다. Unity 파생 FBX의 `ProfessorDesk`, `Beds_E1`, `Beds_K-BOYS` 임시 가구만 제외해 새 가구와 겹치지 않게 한다.
- 반복 부품은 생성 중 메시 템플릿을 공유하고 최종 그룹에서는 독립 메시로 합친다. 템플릿을 직접 변형하지 않으며 삼각형 수 일치와 마감 레이어당 500,000삼각형 상한을 검사한다. 이 상한은 생성 오류를 막는 검토 한도이며 출시 성능 목표가 아니다.
- 방별 보조광과 로컬 색 보정 Volume은 환경 충돌 수집 뒤에 구성한다. LDR URP, 기본 렌더러와 기존 게임 진입 흐름은 유지한다.

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup --threads 4 --python-exit-code 1 --python Tools/Art/export_professor_house_connected.py -- --project . --chunks UpperRooms
/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup --threads 4 --python-exit-code 1 --python Tools/Art/build_professor_house_hall_finish.py -- --project .
/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup --threads 4 --python-exit-code 1 --python Tools/Art/build_professor_house_upper_finish.py -- --project .
```

그 뒤 Unity 메뉴 **Narnia → Professor House → Connected Layout → Build Walkthrough**로 씬·프리팹·NavMesh를 재생성한다. **Open Walkthrough**에서 Play하면 기존 Lucy로 이동한다. 교수 공간은 닫힌 이야기 경계 안에 있으므로 Editor Scene 뷰 또는 확인용 이미지로 내부를 검토한다. **Capture Unity Previews**는 그래픽 모드에서 11개 시점을 촬영한다. 수동 수정은 재생성 전에 원본 코드에 반영해야 한다.

## 검사 범위와 남은 작업

홀 액자의 계단·난간 교차를 먼저 재현했다. 수정 후 홀 장식 472개와 2층 가구 부품 2,725개를 승인 건축의 벽·창·문틀·계단·난간·회랑·받침 메시와 BVH 삼각형 교차 검사했다. 가구와 커튼·창 장식 사이도 방별 네 그룹 쌍으로 검사해 교차가 없는지 확인한다. 방의 벽 마감·바닥 접촉과 창틀의 붙임 마감은 의도된 결합이라 별도 영역으로 두고 화면으로 확인한다. 이 검사는 선택한 가구와 건축 표면의 교차에 대한 근거이며, 모든 메시의 내부 포함·모든 가구 쌍의 접촉을 증명하지 않는다. 새 방은 NavMesh 경로와 캐릭터 캡슐 여유 공간도 검사한다.

저택 전체 아트 완료 상태는 아니다. 다음 단계는 유물 전시실·공용 복도 등 나머지 구역의 마감과 생활 소품, 창밖 정원·외관을 보강하는 일이다. 옷장방 독립 씬 경계, 문·유물 사고 상호작용, 조명 베이크·구역별 표시와 성능 예산, 제품 씬 통합이 남아 있다. Lucy 모델·리깅 정리는 환경 구조와 동선을 안정시킨 뒤 진행한다. 이번에는 macOS 플레이어 빌드나 라이트맵 베이크를 수행하지 않았다.
