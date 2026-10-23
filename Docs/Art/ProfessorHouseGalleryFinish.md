# 교수의 집 · 전시실과 연결 복도 01

2026-10-23. `Professor_House_Connected`에 유물 전시실과 1·2층 주요 복도 마감을 추가했다. [실제 Unity 화면·서재 전후 비교](../Validation/ProfessorHouseGalleryFinish/index.html) · [검증 결과](../Validation/ProfessorHouseGalleryFinish.md).

## 교수 서재 높이 재확인

완성 시안과 실제 화면의 차이를 층고·창 높이·책장 높이·카메라로 나누어 확인했다. 생성 메시를 수직 BVH 광선으로 측정한 결과는 다음과 같다.

| 기준 | 높이 |
| --- | --- |
| 구조 바닥 / 다락 바닥 | +4.32 / +9.00 m, 층간 4.68 m |
| 마감 바닥 / 천장 하단 | +4.337 / +8.760 m |
| 실내 천장 높이 | 4.423 m |
| 가장 낮은 보 아래 | 4.158 m |
| 창 하단 / 상단 | 구조 바닥 기준 +1.00 / +2.80 m |

확정 구조와 일치하므로 바닥·천장·창 개구부는 유지했다. 시안은 원근 이미지이므로 절대 층고를 역산하거나 실측값과 같다고 단정하지 않는다. 기존 책장 상단 약 2.98 m를 약 3.98 m로 높이고 상부 목재 마감·천장등을 추가했다. 북벽 그림은 책장과 겹치지 않도록 서쪽 두 창 사이로 옮겼다. 창의 납선 끝에는 테두리를 연결했다. 전후 이미지는 같은 위치·70도 시야각으로 촬영한다. [측정값](../Validation/ProfessorHouseGalleryFinish/StudyHeight.json) · [높이 도해](../Validation/ProfessorHouseGalleryFinish/StudyHeight.svg).

## 추가한 구역

| 구역 | 마감 |
| --- | --- |
| 1층 D1 유물 전시실 | 석재 타일·러그·목재 하부 벽·천장 보, 진열장 5개, 천구의·도자기·트로피·청동 말·흉상·갑옷·그림·벽 촛대 |
| 1층 F0/F1/F2 | 동쪽 연결 복도·전면 갤러리·동측 회랑의 벽·천장·마루·러너·그림·벤치 |
| 2층 E3/F0/F1/F2/K0 | 가족 복도와 동측 연결 통로·아이들 침실 복도의 같은 마감 |

사고 창문은 서쪽 X=0, Y=18, 폭 2.3 m, 갑옷 중심은 X=2.3, Y=18을 유지한다. 창 안쪽에서 투구로 향하는 선에 진열장을 놓지 않는다. 갑옷은 받침·몸통·어깨·투구 메시로 분리했으며 현재는 고정 장식이다. 파손 물리·애니메이션·퀘스트 이벤트를 구현한 상태가 아니다. 유리와 진열장은 단순 충돌체로 처리한다.

복도 벤치는 벽 방향으로 놓고, 폭 2 m인 K0에는 추가 가구를 두지 않는다. 기존 끝 수납장을 유지한다. 동쪽 전면 갤러리의 경계에 걸친 반쪽 창은 기존 개구부를 옮기지 않고 안쪽 좁은 창틀과 셔터로 마감했다. 원래의 방·문·계단·교수 구역 경계와 S2/K0 분리 구조는 유지한다.

## 제작 원본과 리소스

- 원본: `ArtSource/ProfessorHouse/GalleryFinish_v1/ProfessorHouse_GalleryFinish_v1.blend`.
- 생성: `Tools/Art/build_professor_house_gallery_finish.py`; 공통 메시 도구 `professor_house_finish_kit.py` 재사용.
- Unity: `Assets/@Project/10.Models/ProfessorHouseGalleryFinish`, `OBJ_ProfessorHouse_GalleryFinish`, `ProfessorHouseGalleryFinishBuilder`.
- 전시실·복도 레이어: 22개 시각 메시, 218,232삼각형, BoxCollider 14개. 보완한 2층 레이어: 16개 메시, 277,860삼각형, 충돌 39개.
- 그림 아틀라스와 Hall 재질을 재사용한다. 진열 유리는 투명 재질과 별도 메시이며 그림자 투사를 끈다. 새 래스터 이미지 생성·Addressables 그룹·Resources 로드는 추가하지 않는다. 고정 환경은 씬/프리팹 직렬화 참조가 소유한다.
- 원본 연결 모델의 `ArtifactArmor`, `ArtifactCases`는 보존하고 Unity 파생 GroundRooms FBX에서만 제외한다. 기존 침대·책상 교체와 합쳐 제외 파트는 다섯 개다.
- 연결 씬 전용 실시간 보조광을 조정했다. LDR URP와 기본 렌더러 인덱스 0, 제품 Build Settings는 유지한다.

```sh
/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup --threads 4 --python-exit-code 1 --python Tools/Art/export_professor_house_connected.py -- --project . --chunks GroundRooms
/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup --threads 4 --python-exit-code 1 --python Tools/Art/build_professor_house_upper_finish.py -- --project .
/Applications/Blender.app/Contents/MacOS/Blender --background --factory-startup --threads 4 --python-exit-code 1 --python Tools/Art/build_professor_house_gallery_finish.py -- --project .
```

그 뒤 Unity **Narnia → Professor House → Connected Layout → Build Walkthrough**로 재생성한다. **Open Walkthrough**에서 Lucy로 이동하며 **Capture Unity Previews**는 그래픽 모드에서 현재 구역 11장을 촬영한다. 새 원본 체크아웃에서 모든 파생 구조를 생성할 때는 `--chunks`를 생략한다. 이전 홀 마감 생성 명령은 [홀 문서](ProfessorHouseHallFinish.md)를 따른다.

## 현재 완성도와 다음 단계

구조만 있는 단계는 지났지만 저택 전체의 최종 아트는 아니다. 이번 구역도 세부 재질·생활감·창밖 풍경·최종 조명이 남아 있다. 이어 응접실·식당·공용 도서실 등 남은 방, 다락·옷장방과 외관·정원을 마감한다. 이후 문과 유물 사고 상호작용, 옷장방 씬 경계, 조명 베이크·구역별 표시·성능 예산과 제품 씬 통합을 진행한다. Lucy 모델·리깅 보완은 환경 구조와 동선이 안정된 뒤 진행한다. 이번에는 라이트맵 베이크나 플레이어 빌드를 수행하지 않았다.
